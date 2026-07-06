// "Copyright (c) YOUR_COMPANY. All rights reserved."

using ApplicationCore.Domain.CEN;
using ApplicationCore.Domain.EN;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Extensions.DependencyInjection;
using System.Security.Claims;

namespace WebMarkerSpace.Security;

/// <summary>
/// Traduce el resultado de un login con OAuth2 / OpenID Connect a una cuenta local
/// de WebMarkerSpace: si el usuario ya existía se reconoce (por IdExterno o, en su
/// defecto, por email), y si no existe se crea de alta con rol "Usuario".
///
/// El principal que llega del proveedor externo NUNCA se usa tal cual para
/// autorizar nada dentro de la aplicación: los roles de WebMarkerSpace
/// (Administrador / Usuario) los decide siempre nuestra base de datos, no el
/// proveedor de identidad. Por eso este método sustituye el principal completo
/// por uno construido a partir del <see cref="Usuario"/> local antes de que se
/// emita la cookie de sesión.
/// </summary>
public static class OidcAccountProvisioning
{
    public const string NombreProveedor = "oidc";

    public static async Task ProvisionarYSustituirPrincipalAsync(TokenValidatedContext context)
    {
        var externalPrincipal = context.Principal
            ?? throw new InvalidOperationException("El proveedor externo no devolvió información de usuario.");

        string idExterno = externalPrincipal.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? externalPrincipal.FindFirstValue("sub")
            ?? throw new InvalidOperationException("El proveedor externo no incluyó un identificador de usuario (claim 'sub').");

        string email = externalPrincipal.FindFirstValue(ClaimTypes.Email)
            ?? externalPrincipal.FindFirstValue("email")
            ?? throw new InvalidOperationException("El proveedor externo no incluyó un email. Revisa el scope 'email' en la configuración de OIDC.");

        string nombre = externalPrincipal.FindFirstValue(ClaimTypes.Name)
            ?? externalPrincipal.FindFirstValue("name")
            ?? email;

        var usuarioCEN = context.HttpContext.RequestServices.GetRequiredService<UsuarioCEN>();
        var session = context.HttpContext.RequestServices.GetRequiredService<NHibernate.ISession>();

        // 1) ¿Ya conocemos a este usuario por su identificador estable del proveedor?
        var usuario = usuarioCEN.ObtenerPorIdExterno(NombreProveedor, idExterno);

        if (usuario == null)
        {
            // 2) ¿Existe ya una cuenta local con ese email (creada con email/contraseña)?
            //    Si es así, la vinculamos en vez de crear un duplicado.
            var existentePorEmail = usuarioCEN.ObtenerPorEmail(email);

            using var tx = session.BeginTransaction();
            try
            {
                if (existentePorEmail != null)
                {
                    usuarioCEN.VincularExterno(existentePorEmail.Id, NombreProveedor, idExterno);
                    usuario = existentePorEmail;
                }
                else
                {
                    // 3) Aprovisionamiento "just-in-time": primera vez que vemos a este usuario.
                    //    Se le asigna siempre el rol "Usuario"; nunca Administrador.
                    long nuevoId = usuarioCEN.CrearExterno(nombre, email, NombreProveedor, idExterno);
                    usuario = usuarioCEN.ObtenerPorId(nuevoId);
                }
                tx.Commit();
            }
            catch
            {
                tx.Rollback();
                throw;
            }
        }

        if (usuario == null)
        {
            throw new InvalidOperationException("No fue posible aprovisionar el usuario a partir del login externo.");
        }

        // Construimos NUESTRO propio principal (mismas claims que usa el login local),
        // para que el resto de la aplicación (roles, [Authorize], vistas) funcione
        // exactamente igual sin importar si el usuario entró con contraseña local o
        // con un proveedor externo.
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
            new Claim(ClaimTypes.Name, usuario.Nombre),
            new Claim(ClaimTypes.Email, usuario.Email),
            new Claim(ClaimTypes.Role, usuario.Rol.ToString()),
            new Claim("auth_method", NombreProveedor)
        };

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        context.Principal = new ClaimsPrincipal(identity);

        // El handler de OIDC ya está configurado con SignInScheme = Cookie, así que al
        // continuar el pipeline se firmará la cookie de sesión con este principal local.
        await Task.CompletedTask;
    }
}


using ApplicationCore.Domain.CEN;
using ApplicationCore.Domain.EN;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Extensions.DependencyInjection;
using System.Security.Claims;

namespace WebMarkerSpace.Security;

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

        var usuario = usuarioCEN.ObtenerPorIdExterno(NombreProveedor, idExterno);

        if (usuario == null)
        {

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

        await Task.CompletedTask;
    }
}

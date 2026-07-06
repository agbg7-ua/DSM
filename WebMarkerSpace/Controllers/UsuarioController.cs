// "Copyright (c) YOUR_COMPANY. All rights reserved."

using ApplicationCore.Domain.CEN;
using ApplicationCore.Domain.EN;
using ApplicationCore.Domain.Enums;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using WebMarkerSpace.Assemblers;
using WebMarkerSpace.Models;
using WebMarkerSpace.Security;

namespace WebMarkerSpace.Controllers {
    public class UsuarioController : Controller {
        private readonly UsuarioCEN _usuarioCEN;
        private readonly NHibernate.ISession _session;
        private readonly OidcSettings _oidcSettings;

        public UsuarioController(UsuarioCEN usuarioCEN, NHibernate.ISession session, OidcSettings oidcSettings) {
            _usuarioCEN = usuarioCEN;
            _session = session;
            _oidcSettings = oidcSettings;
        }

        private async Task IniciarSesionComo(long id, string nombre, string email, RolUsuario rol) {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, id.ToString()),
                new Claim(ClaimTypes.Name, nombre),
                new Claim(ClaimTypes.Email, email),
                new Claim(ClaimTypes.Role, rol.ToString())
            };
            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal,
                new AuthenticationProperties { IsPersistent = false });
        }

        // GET: UsuarioController/Login
        [AllowAnonymous]
        public ActionResult Login(string? returnUrl, string? error) {
            ViewBag.OidcHabilitado = _oidcSettings.Habilitado;
            ViewBag.OidcNombre = _oidcSettings.NombreProveedor;
            ViewBag.ReturnUrl = returnUrl;

            if (error == "oidc") {
                ModelState.AddModelError("", "No se ha podido completar el inicio de sesión con el proveedor externo. Inténtalo de nuevo.");
            }

            return View(new LoginUsuarioViewModel());
        }

        // GET: UsuarioController/ExternalLogin
        // Redirige al proveedor OAuth2 / OpenID Connect configurado. El resto del
        // flujo (callback, creación/vinculación de cuenta) lo gestiona el middleware
        // de autenticación configurado en Program.cs junto con OidcAccountProvisioning.
        [AllowAnonymous]
        public ActionResult ExternalLogin(string? returnUrl) {
            if (!_oidcSettings.Habilitado) {
                return NotFound();
            }

            var destino = string.IsNullOrWhiteSpace(returnUrl) ? Url.Action("Index", "Home")! : returnUrl;
            var properties = new AuthenticationProperties { RedirectUri = destino };
            return Challenge(properties, OpenIdConnectDefaults.AuthenticationScheme);
        }

        // POST: UsuarioController/Login
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Login(LoginUsuarioViewModel login) {
            ViewBag.OidcHabilitado = _oidcSettings.Habilitado;
            ViewBag.OidcNombre = _oidcSettings.NombreProveedor;

            if (!ModelState.IsValid)
                return View(login);

            bool loginOk = _usuarioCEN.Login(login.Email, login.Contrasenia);
            var usuEN = loginOk ? _usuarioCEN.ObtenerTodos().FirstOrDefault(u => u.Email == login.Email) : null;

            if (!loginOk || usuEN == null) {
                ModelState.AddModelError("", "Email o contraseña incorrectos.");
                return View(login);
            }

            await IniciarSesionComo(usuEN.Id, usuEN.Nombre, usuEN.Email, usuEN.Rol);
            return RedirectToAction("Index", "Home");
        }

        // POST: UsuarioController/Logout
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Logout() {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login");
        }

        // GET: UsuarioController/Register
        // Formulario de registro público (cualquier persona puede darse de alta).
        // También se ofrece aquí la opción de registrarse con el proveedor OAuth2 /
        // OpenID Connect configurado: usa el mismo endpoint ExternalLogin que el
        // login, ya que OidcAccountProvisioning crea la cuenta local automáticamente
        // ("just-in-time") la primera vez que el proveedor externo confirma el login.
        [AllowAnonymous]
        public ActionResult Register(string? returnUrl) {
            ViewBag.OidcHabilitado = _oidcSettings.Habilitado;
            ViewBag.OidcNombre = _oidcSettings.NombreProveedor;
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        // POST: UsuarioController/Register
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Register(RegistroUsuarioViewModel model) {
            ViewBag.OidcHabilitado = _oidcSettings.Habilitado;
            ViewBag.OidcNombre = _oidcSettings.NombreProveedor;

            if (!ModelState.IsValid)
                return View(model);

            bool emailEnUso = _usuarioCEN.ObtenerTodos().Any(u => u.Email == model.Email);
            if (emailEnUso) {
                ModelState.AddModelError(nameof(model.Email), "Ya existe una cuenta con ese correo.");
                return View(model);
            }

            using var tx = _session.BeginTransaction();
            long nuevoId;
            try {
                // El rol siempre se fija a "Usuario": un registro público nunca
                // puede crear una cuenta de Administrador.
                nuevoId = _usuarioCEN.Crear(model.Nombre, model.Email, model.Contrasenia, RolUsuario.Usuario);
                tx.Commit();
            }
            catch (Exception ex) {
                tx.Rollback();
                ModelState.AddModelError("", "Error al registrar: " + ex.Message);
                return View(model);
            }

            await IniciarSesionComo(nuevoId, model.Nombre, model.Email, RolUsuario.Usuario);
            return RedirectToAction("Index", "Home");
        }

        // GET: UsuarioController/Perfil
        // Cualquier usuario logueado puede ver y editar SUS PROPIOS datos (no los de nadie más).
        [Authorize]
        public ActionResult Perfil() {
            long miId = long.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var usuarioEN = _usuarioCEN.ObtenerPorId(miId);
            if (usuarioEN == null) {
                return NotFound();
            }

            var model = new PerfilViewModel {
                Id = usuarioEN.Id,
                Nombre = usuarioEN.Nombre,
                Email = usuarioEN.Email,
                Rol = usuarioEN.Rol
            };
            return View(model);
        }

        // POST: UsuarioController/Perfil
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Perfil(PerfilViewModel model) {
            long miId = long.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            // Nunca nos fiamos del Id que venga del formulario: siempre editamos
            // al usuario autenticado, nunca a otro.
            model.Id = miId;

            if (!ModelState.IsValid) {
                return View(model);
            }

            var usuarioActual = _usuarioCEN.ObtenerPorId(miId);
            if (usuarioActual == null) {
                return NotFound();
            }

            // Si el usuario no ha escrito una contraseña nueva, mantenemos la actual.
            string contraseniaFinal = string.IsNullOrWhiteSpace(model.NuevaContrasenia)
                ? usuarioActual.Contrasenia
                : model.NuevaContrasenia;

            using var tx = _session.BeginTransaction();
            try {
                // El rol nunca se toca aquí: se conserva el que ya tenía.
                _usuarioCEN.Modificar(miId, model.Nombre, model.Email, contraseniaFinal, usuarioActual.Rol);
                tx.Commit();
            }
            catch (Exception ex) {
                tx.Rollback();
                ModelState.AddModelError("", "Error al actualizar el perfil: " + ex.Message);
                return View(model);
            }

            // Refrescamos la cookie por si ha cambiado el nombre (aparece en la barra de navegación).
            await IniciarSesionComo(miId, model.Nombre, model.Email, usuarioActual.Rol);

            TempData["MensajeExito"] = "Tus datos se han actualizado correctamente.";
            return RedirectToAction(nameof(Perfil));
        }

        // GET: UsuarioController
        [Authorize(Roles = "Administrador")]
        public ActionResult Index(string? texto, RolUsuario? rol) {
            IEnumerable<Usuario> usuarios = _usuarioCEN.ObtenerTodos();

            if (!string.IsNullOrWhiteSpace(texto)) {
                usuarios = usuarios.Where(u =>
                    u.Nombre.Contains(texto, StringComparison.OrdinalIgnoreCase) ||
                    u.Email.Contains(texto, StringComparison.OrdinalIgnoreCase));
            }
            if (rol.HasValue) {
                usuarios = usuarios.Where(u => u.Rol == rol.Value);
            }

            IEnumerable<UsuarioViewModel> listUsers = new UsuarioAssembler().ConvertirListaENToViewModel(usuarios.ToList());

            ViewBag.FiltroTexto = texto;
            ViewBag.FiltroRol = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(Enum.GetValues(typeof(RolUsuario)), rol);
            return View(listUsers);
        }

        // GET: UsuarioController/Details/5
        [Authorize(Roles = "Administrador")]
        public ActionResult Details(int id) {
            var usuarioEN = _usuarioCEN.ObtenerPorId(id);
            if (usuarioEN == null) {
                return NotFound();
            }
            var model = new UsuarioAssembler().ConvertirENToViewModel(usuarioEN);
            return View(model);
        }

        // GET: UsuarioController/Edit/5
        [Authorize(Roles = "Administrador")]
        public ActionResult Edit(int id) {
            var usuarioEN = _usuarioCEN.ObtenerPorId(id);
            if (usuarioEN == null) {
                return NotFound();
            }
            var model = new UsuarioAssembler().ConvertirENToViewModel(usuarioEN);
            return View(model);
        }

        // POST: UsuarioController/Edit/5
        [HttpPost]
        [Authorize(Roles = "Administrador")]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, UsuarioViewModel user) {
            using var tx = _session.BeginTransaction();
            try {
                _usuarioCEN.Modificar(user.Id, user.Nombre, user.Email, user.Contrasenia, user.Rol);
                tx.Commit();
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex) {
                tx.Rollback();
                ModelState.AddModelError("", "Error al editar: " + ex.Message);
                return View(user);
            }
        }

        // GET: UsuarioController/Delete/5
        [Authorize(Roles = "Administrador")]
        public ActionResult Delete(int id) {
            var usuarioEN = _usuarioCEN.ObtenerPorId(id);
            if (usuarioEN == null) {
                return NotFound();
            }
            var model = new UsuarioAssembler().ConvertirENToViewModel(usuarioEN);
            return View(model);
        }

        // POST: UsuarioController/Delete/5
        [HttpPost]
        [Authorize(Roles = "Administrador")]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, UsuarioViewModel user) {
            using var tx = _session.BeginTransaction();
            try {
                _usuarioCEN.Eliminar(id);
                tx.Commit();
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex) {
                tx.Rollback();
                ModelState.AddModelError("", "Error al eliminar: " + ex.Message);
                return View(user);
            }
        }
    }
}

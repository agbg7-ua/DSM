
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using ApplicationCore.Domain.CEN;
using ApplicationCore.Domain.EN;
using ApplicationCore.Domain.Enums;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Localization;
using WebMarkerSpace.Assemblers;
using WebMarkerSpace.Extensions;
using WebMarkerSpace.Models;
using WebMarkerSpace.Security;

namespace WebMarkerSpace.Controllers {
    public class UsuarioController : Controller {
        private readonly UsuarioCEN _usuarioCEN;
        private readonly NHibernate.ISession _session;
        private readonly OidcSettings _oidcSettings;
        private readonly IStringLocalizer<SharedResource> _localizer;

        public UsuarioController(UsuarioCEN usuarioCEN, NHibernate.ISession session, OidcSettings oidcSettings, IStringLocalizer<SharedResource> localizer) {
            _usuarioCEN = usuarioCEN;
            _session = session;
            _oidcSettings = oidcSettings;
            _localizer = localizer;
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

        [AllowAnonymous]
        public ActionResult Login(string? returnUrl, string? error) {
            ViewBag.OidcHabilitado = _oidcSettings.Habilitado;
            ViewBag.OidcNombre = _oidcSettings.NombreProveedor;
            ViewBag.ReturnUrl = returnUrl;

            if (error == "oidc") {
                ModelState.AddModelError("", _localizer["Login.ExternalError"].Value);
            }

            return View(new LoginUsuarioViewModel());
        }

        [AllowAnonymous]
        public ActionResult ExternalLogin(string? returnUrl) {
            if (!_oidcSettings.Habilitado) {
                return NotFound();
            }

            var destino = string.IsNullOrWhiteSpace(returnUrl) ? Url.Action("Index", "Home")! : returnUrl;
            var properties = new AuthenticationProperties { RedirectUri = destino };
            return Challenge(properties, OpenIdConnectDefaults.AuthenticationScheme);
        }

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
                ModelState.AddModelError("", _localizer["Login.InvalidCredentials"].Value);
                return View(login);
            }

            await IniciarSesionComo(usuEN.Id, usuEN.Nombre, usuEN.Email, usuEN.Rol);
            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Logout() {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login");
        }

        [AllowAnonymous]
        public ActionResult Register(string? returnUrl) {
            ViewBag.OidcHabilitado = _oidcSettings.Habilitado;
            ViewBag.OidcNombre = _oidcSettings.NombreProveedor;
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

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
                ModelState.AddModelError(nameof(model.Email), _localizer["Register.EmailTaken"].Value);
                return View(model);
            }

            using var tx = _session.BeginTransaction();
            long nuevoId;
            try {

                nuevoId = _usuarioCEN.Crear(model.Nombre, model.Email, model.Contrasenia, RolUsuario.Usuario);
                tx.Commit();
            }
            catch (Exception ex) {
                tx.Rollback();
                ModelState.AddModelError("", _localizer["Register.Error", ex.Message].Value);
                return View(model);
            }

            await IniciarSesionComo(nuevoId, model.Nombre, model.Email, RolUsuario.Usuario);
            return RedirectToAction("Index", "Home");
        }

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
                Rol = usuarioEN.Rol,
                EsCuentaExterna = !string.IsNullOrWhiteSpace(usuarioEN.ProveedorExterno)
            };
            return View(model);
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Perfil(PerfilViewModel model) {
            long miId = long.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            model.Id = miId;
            model.EsCuentaExterna = _usuarioCEN.EsCuentaExterna(miId);

            if (!ModelState.IsValid) {
                return View(model);
            }

            var usuarioActual = _usuarioCEN.ObtenerPorId(miId);
            if (usuarioActual == null) {
                return NotFound();
            }

            using var tx = _session.BeginTransaction();
            try {

                _usuarioCEN.Modificar(miId, model.Nombre, model.Email, model.NuevaContrasenia, usuarioActual.Rol);
                tx.Commit();
            }
            catch (Exception ex) {
                tx.Rollback();
                ModelState.AddModelError("", _localizer["Perfil.UpdateError", ex.Message].Value);
                return View(model);
            }

            await IniciarSesionComo(miId, model.Nombre, model.Email, usuarioActual.Rol);

            TempData["MensajeExito"] = _localizer["Perfil.UpdateSuccess"].Value;
            return RedirectToAction(nameof(Perfil));
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> EliminarPerfil(string? contrasenia) {
            long miId = long.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            // Si la cuenta no es externa (OIDC), se exige confirmar con la contraseña actual.
            if (!_usuarioCEN.EsCuentaExterna(miId) && !_usuarioCEN.VerificarContrasenia(miId, contrasenia ?? string.Empty)) {
                if (EsPeticionAjax()) {
                    return Json(new { success = false, message = _localizer["Perfil.Delete.WrongPassword"].Value });
                }

                ModelState.AddModelError("", _localizer["Perfil.Delete.WrongPassword"].Value);

                var usuarioActualPwd = _usuarioCEN.ObtenerPorId(miId);
                var modelPwd = new PerfilViewModel {
                    Id = miId,
                    Nombre = usuarioActualPwd?.Nombre ?? string.Empty,
                    Email = usuarioActualPwd?.Email ?? string.Empty,
                    Rol = usuarioActualPwd?.Rol ?? RolUsuario.Usuario,
                    EsCuentaExterna = !string.IsNullOrWhiteSpace(usuarioActualPwd?.ProveedorExterno)
                };
                return View(nameof(Perfil), modelPwd);
            }

            using var tx = _session.BeginTransaction();
            try {
                _usuarioCEN.Eliminar(miId);
                tx.Commit();
            }
            catch (Exception ex) {
                tx.Rollback();

                if (EsPeticionAjax()) {
                    return Json(new { success = false, message = _localizer["Perfil.Delete.Error", MensajeDeError(ex)].Value });
                }

                ModelState.AddModelError("", _localizer["Perfil.Delete.Error", MensajeDeError(ex)].Value);

                var usuarioActual = _usuarioCEN.ObtenerPorId(miId);
                var model = new PerfilViewModel {
                    Id = miId,
                    Nombre = usuarioActual?.Nombre ?? string.Empty,
                    Email = usuarioActual?.Email ?? string.Empty,
                    Rol = usuarioActual?.Rol ?? RolUsuario.Usuario,
                    EsCuentaExterna = !string.IsNullOrWhiteSpace(usuarioActual?.ProveedorExterno)
                };
                return View(nameof(Perfil), model);
            }

            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            if (EsPeticionAjax()) {
                return Json(new { success = true });
            }

            return RedirectToAction("Login");
        }

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

            ViewBag.FiltroRol = new SelectList(
                Enum.GetValues(typeof(RolUsuario)).Cast<RolUsuario>()
                    .Select(r => new SelectListItem(_localizer.Localize(r), r.ToString(), r.Equals(rol))),
                "Value", "Text", rol);

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest") {
                return PartialView("_UsuarioListPartial", listUsers);
            }

            return View(listUsers);
        }

        [Authorize(Roles = "Administrador")]
        public ActionResult Details(int id) {
            var usuarioEN = _usuarioCEN.ObtenerPorId(id);
            if (usuarioEN == null) {
                return NotFound();
            }
            var model = new UsuarioAssembler().ConvertirENToViewModel(usuarioEN);
            return View(model);
        }

        [Authorize(Roles = "Administrador")]
        public ActionResult Edit(int id) {
            var usuarioEN = _usuarioCEN.ObtenerPorId(id);
            if (usuarioEN == null) {
                return NotFound();
            }
            var model = new UsuarioAssembler().ConvertirENToViewModel(usuarioEN);
            return View(model);
        }

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
                ModelState.AddModelError("", _localizer["Usuario.Edit.Error", ex.Message].Value);
                return View(user);
            }
        }

        [Authorize(Roles = "Administrador")]
        public ActionResult Delete(int id) {
            var usuarioEN = _usuarioCEN.ObtenerPorId(id);
            if (usuarioEN == null) {
                return NotFound();
            }
            var model = new UsuarioAssembler().ConvertirENToViewModel(usuarioEN);
            return View(model);
        }

        [HttpPost]
        [Authorize(Roles = "Administrador")]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, UsuarioViewModel user) {
            using var tx = _session.BeginTransaction();
            try {
                _usuarioCEN.Eliminar(id);
                tx.Commit();

                if (EsPeticionAjax()) {
                    return Json(new { success = true });
                }
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex) {
                tx.Rollback();

                if (EsPeticionAjax()) {
                    return Json(new { success = false, message = _localizer["Usuario.Delete.Error", MensajeDeError(ex)].Value });
                }
                ModelState.AddModelError("", _localizer["Usuario.Delete.Error", MensajeDeError(ex)].Value);
                return View(user);
            }
        }

        private bool EsPeticionAjax() {
            return Request.Headers["X-Requested-With"] == "XMLHttpRequest";
        }

        private static string MensajeDeError(Exception ex) => ex.GetBaseException().Message;
    }
}

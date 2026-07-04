// "Copyright (c) YOUR_COMPANY. All rights reserved."

using ApplicationCore.Domain.CEN;
using ApplicationCore.Domain.EN;
using ApplicationCore.Domain.Enums;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using WebMarkerSpace.Assemblers;
using WebMarkerSpace.Models;

namespace WebMarkerSpace.Controllers {
    public class UsuarioController : Controller {
        private readonly UsuarioCEN _usuarioCEN;
        private readonly NHibernate.ISession _session;

        public UsuarioController(UsuarioCEN usuarioCEN, NHibernate.ISession session) {
            _usuarioCEN = usuarioCEN;
            _session = session;
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
        public ActionResult Login() {
            return View();
        }

        // POST: UsuarioController/Login
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Login(LoginUsuarioViewModel login) {
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
        [AllowAnonymous]
        public ActionResult Register() {
            return View();
        }

        // POST: UsuarioController/Register
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Register(RegistroUsuarioViewModel model) {
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

        // GET: UsuarioController
        [Authorize(Roles = "Administrador")]
        public ActionResult Index() {
            IList<Usuario> usuarios = _usuarioCEN.ObtenerTodos();
            IEnumerable<UsuarioViewModel> listUsers = new UsuarioAssembler().ConvertirListaENToViewModel(usuarios);
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

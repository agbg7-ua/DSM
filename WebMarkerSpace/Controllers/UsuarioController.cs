// "Copyright (c) YOUR_COMPANY. All rights reserved."

using ApplicationCore.Domain.CEN;
using ApplicationCore.Domain.EN;
using Infrastructure.NHibernate.Repositories;
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
using WebMarkerSpace.Extensions;
using WebMarkerSpace.Models;

namespace WebMarkerSpace.Controllers {
    public class UsuarioController : BasicController {
        private readonly UsuarioCEN _usuarioCEN;

        public UsuarioController(UsuarioCEN usuarioCEN) {
            _usuarioCEN = usuarioCEN;
        }

        // GET: UsuarioController/Login
        [AllowAnonymous]
        public ActionResult Login()
        {
            return View();
        }

        // POST: UsuarioController/Login
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Login(LoginUsuarioViewModel login)
        {
            if (!ModelState.IsValid)
                return View(login);

            SessionInitialize();
            bool loginOk = _usuarioCEN.Login(login.Email, login.Contrasenia);
            var usuEN = loginOk ? _usuarioCEN.ObtenerTodos().FirstOrDefault(u => u.Email == login.Email) : null;
            SessionClose();

            if (!loginOk || usuEN == null)
            {
                ModelState.AddModelError("", "Email o contraseña incorrectos.");
                return View(login);
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, usuEN.Id.ToString()),
                new Claim(ClaimTypes.Name, usuEN.Nombre),
                new Claim(ClaimTypes.Email, usuEN.Email),
                new Claim(ClaimTypes.Role, usuEN.Rol.ToString())
            };
            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal,
                new AuthenticationProperties { IsPersistent = false });

            return RedirectToAction("Index", "Home");
        }

        // POST: UsuarioController/Logout
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login");
        }

        // GET: UsuarioController
        [Authorize(Roles = "Administrador")]
        public ActionResult Index() {
            SessionInitialize();
            IList<Usuario> usuarios = _usuarioCEN.ObtenerTodos();
            IEnumerable<UsuarioViewModel> listUsers = new UsuarioAssembler().ConvertirListaENToViewModel(usuarios);
            SessionClose();
            return View(listUsers);
        }
        // GET: UsuarioController/Details/5
        [Authorize(Roles = "Administrador")]
        public ActionResult Details(int id) {
            SessionInitialize();
            var usuarioEN = _usuarioCEN.ObtenerPorId(id);
            if (usuarioEN == null) {
                SessionClose();
                return NotFound();
            }
            var model = new UsuarioAssembler().ConvertirENToViewModel(usuarioEN);
            SessionClose();
            return View(model);
        }

        // GET: UsuarioController/Create
        [Authorize(Roles = "Administrador")]
        public ActionResult Create() {
            return View();
        }

        // POST: UsuarioController/Create
        [HttpPost]
        [Authorize(Roles = "Administrador")]
        [ValidateAntiForgeryToken]
        public ActionResult Create(UsuarioViewModel user) {
            NHibernate.ITransaction tx = null;
            try {
                SessionInitialize();

                var campoSesion = typeof(BasicController).GetField("sessionInside", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                var nHibernateSession = (NHibernate.ISession)campoSesion.GetValue(this);

                tx = nHibernateSession.BeginTransaction();

                var usuarioRepository = new UsuarioRepository(nHibernateSession);
                var cenTemporal = new UsuarioCEN(usuarioRepository);
                cenTemporal.Crear(user.Nombre, user.Email, user.Contrasenia, user.Rol);

                tx.Commit();
                SessionClose();
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex) {
                if (tx != null && tx.IsActive)
                    tx.Rollback();
                SessionClose();
                ModelState.AddModelError("", "Error al crear: " + ex.Message);
                return View(user);
            }
        }

        // GET: UsuarioController/Edit/5
        [Authorize(Roles = "Administrador")]
        public ActionResult Edit(int id) {
            SessionInitialize();
            var usuarioEN = _usuarioCEN.ObtenerPorId(id);
            if (usuarioEN == null) {
                SessionClose();
                return NotFound();
            }
            var model = new UsuarioAssembler().ConvertirENToViewModel(usuarioEN);
            SessionClose();
            return View(model);
        }

        // POST: UsuarioController/Edit/5
        [HttpPost]
        [Authorize(Roles = "Administrador")]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, UsuarioViewModel user) {
            NHibernate.ITransaction tx = null;
            try {
                SessionInitialize();

                var campoSesion = typeof(BasicController).GetField("sessionInside", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                var nHibernateSession = (NHibernate.ISession)campoSesion.GetValue(this);

                tx = nHibernateSession.BeginTransaction();

                var usuarioRepository = new UsuarioRepository(nHibernateSession);
                var cenTemporal = new UsuarioCEN(usuarioRepository);
                cenTemporal.Modificar(user.Id, user.Nombre, user.Email, user.Contrasenia, user.Rol);

                tx.Commit();
                SessionClose();
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex) {
                if (tx != null && tx.IsActive)
                    tx.Rollback();
                SessionClose();
                ModelState.AddModelError("", "Error al editar: " + ex.Message);
                return View(user);
            }
        }

        // GET: UsuarioController/Delete/5
        [Authorize(Roles = "Administrador")]
        public ActionResult Delete(int id) {
            SessionInitialize();
            var usuarioEN = _usuarioCEN.ObtenerPorId(id);
            if (usuarioEN == null) {
                SessionClose();
                return NotFound();
            }
            var model = new UsuarioAssembler().ConvertirENToViewModel(usuarioEN);
            SessionClose();
            return View(model);
        }

        // POST: UsuarioController/Delete/5
        [HttpPost]
        [Authorize(Roles = "Administrador")]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, UsuarioViewModel user) {
            NHibernate.ITransaction tx = null;
            try {
                SessionInitialize();

                var campoSesion = typeof(BasicController).GetField("sessionInside", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                var nHibernateSession = (NHibernate.ISession)campoSesion.GetValue(this);

                tx = nHibernateSession.BeginTransaction();

                var usuarioRepository = new UsuarioRepository(nHibernateSession);
                var cenTemporal = new UsuarioCEN(usuarioRepository);

                cenTemporal.Eliminar(id);

                tx.Commit();
                SessionClose();
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex) {
                if (tx != null && tx.IsActive)
                    tx.Rollback();
                SessionClose();
                ModelState.AddModelError("", "Error al eliminar: " + ex.Message);
                return View(user);
            }
        }
    }
}

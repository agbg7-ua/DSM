// "Copyright (c) YOUR_COMPANY. All rights reserved."

using ApplicationCore.Domain.CEN;
using ApplicationCore.Domain.EN;
using Infrastructure.NHibernate.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using WebMarkerSpace.Assemblers;
using WebMarkerSpace.Models;

namespace WebMarkerSpace.Controllers {
    public class UsuarioController : BasicController {
        private readonly UsuarioCEN _usuarioCEN;

        public UsuarioController(UsuarioCEN usuarioCEN) {
            _usuarioCEN = usuarioCEN;
        }

        // GET: UsuarioController
        public ActionResult Index() {
            SessionInitialize();
            IList<Usuario> usuarios = _usuarioCEN.ObtenerTodos();
            IEnumerable<UsuarioViewModel> listUsers = new UsuarioAssembler().ConvertirListaENToViewModel(usuarios);
            SessionClose();
            return View(listUsers);
        }
        // GET: UsuarioController/Details/5
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
        public ActionResult Create() {
            return View();
        }

        // POST: UsuarioController/Create
        [HttpPost]
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

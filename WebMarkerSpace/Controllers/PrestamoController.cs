// "Copyright (c) YOUR_COMPANY. All rights reserved."

using ApplicationCore.Domain.CEN;
using ApplicationCore.Domain.EN;
using Infrastructure.NHibernate.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using WebMarkerSpace.Assemblers;
using WebMarkerSpace.Models;

namespace WebMarkerSpace.Controllers {
    public class PrestamoController : BasicController {
        private readonly PrestamoCEN _prestamoCEN;
        private readonly UsuarioCEN _usuarioCEN; // Lo necesitamos para los desplegables de usuarios

        public PrestamoController(PrestamoCEN prestamoCEN, UsuarioCEN usuarioCEN) {
            _prestamoCEN = prestamoCEN;
            _usuarioCEN = usuarioCEN;
        }

        // GET: Prestamo
        public ActionResult Index() {
            SessionInitialize();
            IList<Prestamo> prestamos = _prestamoCEN.ObtenerTodos();
            IEnumerable<PrestamoViewModel> modelList = new PrestamoAssembler().ConvertirListaENToViewModel(prestamos);
            SessionClose();
            return View(modelList);
        }

        // GET: PrestamoController/Details/5
        public ActionResult Details(int id) {
            SessionInitialize();
            Prestamo prestamoEN = _prestamoCEN.ObtenerPorId(id);
            if (prestamoEN == null) {
                SessionClose();
                return NotFound();
            }
            PrestamoViewModel model = new PrestamoAssembler().ConvertirENToViewModel(prestamoEN);
            SessionClose();
            return View(model);
        }


        // POST: PrestamoController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(PrestamoViewModel model) {
            NHibernate.ITransaction tx = null;
            try {
                SessionInitialize();

                var campoSesion = typeof(BasicController).GetField("sessionInside", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                var nHibernateSession = (NHibernate.ISession)campoSesion.GetValue(this);

                tx = nHibernateSession.BeginTransaction();

                var prestamoRepository = new PrestamoRepository(nHibernateSession);
                var usuarioRepository = new UsuarioRepository(nHibernateSession);
                var cenTemporal = new PrestamoCEN(prestamoRepository, usuarioRepository);

                // IMPORTANTE: Ajusta el orden según los parámetros exactos de tu PrestamoCEN.Crear(...)
                cenTemporal.Crear(model.UsuarioId, model.FechaCreacion, model.Estado, model.TotalDias);

                tx.Commit();
                SessionClose();
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex) {
                if (tx != null && tx.IsActive)
                    tx.Rollback();
                SessionClose();

                SessionInitialize();
                ViewBag.UsuarioId = new SelectList(_usuarioCEN.ObtenerTodos(), "Id", "Nombre", model.UsuarioId);
                SessionClose();

                ModelState.AddModelError("", "Error al crear el préstamo: " + ex.Message);
                return View(model);
            }
        }


        // POST: PrestamoController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, PrestamoViewModel model) {
            NHibernate.ITransaction tx = null;
            try {
                SessionInitialize();

                var campoSesion = typeof(BasicController).GetField("sessionInside", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                var nHibernateSession = (NHibernate.ISession)campoSesion.GetValue(this);

                tx = nHibernateSession.BeginTransaction();

                var prestamoRepository = new PrestamoRepository(nHibernateSession);
                var usuarioRepository = new UsuarioRepository(nHibernateSession);
                var cenTemporal = new PrestamoCEN(prestamoRepository, usuarioRepository);

                // Ajusta el orden según tu PrestamoCEN.Modificar(...)
                cenTemporal.Modificar(model.Id, model.UsuarioId, model.FechaCreacion, model.Estado, model.TotalDias);

                tx.Commit();
                SessionClose();
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex) {
                if (tx != null && tx.IsActive)
                    tx.Rollback();
                SessionClose();

                SessionInitialize();
                ViewBag.UsuarioId = new SelectList(_usuarioCEN.ObtenerTodos(), "Id", "Nombre", model.UsuarioId);
                SessionClose();

                ModelState.AddModelError("", "Error al modificar: " + ex.Message);
                return View(model);
            }
        }


        // POST: PrestamoController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, PrestamoViewModel model) {
            NHibernate.ITransaction tx = null;
            try {
                SessionInitialize();

                var campoSesion = typeof(BasicController).GetField("sessionInside", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                var nHibernateSession = (NHibernate.ISession)campoSesion.GetValue(this);

                tx = nHibernateSession.BeginTransaction();

                var prestamoRepository = new PrestamoRepository(nHibernateSession);
                var usuarioRepository = new UsuarioRepository(nHibernateSession);
                var cenTemporal = new PrestamoCEN(prestamoRepository, usuarioRepository);

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
                return View(model);
            }
        }
    }
}

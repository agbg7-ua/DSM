// "Copyright (c) YOUR_COMPANY. All rights reserved."

using System;
using ApplicationCore.Domain.CEN;
using ApplicationCore.Domain.EN;
using Infrastructure.NHibernate.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using WebMarkerSpace.Models;

namespace WebMarkerSpace.Controllers {
    public class LineaPrestamoController : BasicController {
        private readonly LineaPrestamoCEN _lineaPrestamoCEN;
        private readonly MaterialCEN _materialCEN;

        public LineaPrestamoController(LineaPrestamoCEN lineaPrestamoCEN, MaterialCEN materialCEN) {
            _lineaPrestamoCEN = lineaPrestamoCEN;
            _materialCEN = materialCEN;
        }

        // GET: LineaPrestamoController/Details/5
        public ActionResult Details(int id) {
            return View();
        }

        // GET: LineaPrestamoController/Create
        public ActionResult Create(long prestamoId) {
            SessionInitialize();

            // Pasamos el ID del préstamo padre mediante el modelo
            var model = new LineaPrestamoViewModel { PrestamoId = prestamoId };

            // Cargamos solo los materiales disponibles para el desplegable
            var materiales = _materialCEN.ObtenerTodos();
            ViewBag.MaterialId = new SelectList(materiales, "Id", "Nombre");

            SessionClose();
            return View(model);
        }

        // POST: LineaPrestamoController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(LineaPrestamoViewModel model) {
            NHibernate.ITransaction tx = null;
            try {
                SessionInitialize();
                var campoSesion = typeof(BasicController).GetField("sessionInside", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                var nHibernateSession = (NHibernate.ISession)campoSesion.GetValue(this);

                tx = nHibernateSession.BeginTransaction();


                var lineaRepository = new LineaPrestamoRepository(nHibernateSession);
                var prestamoRepository = new PrestamoRepository(nHibernateSession);
                var materialRepository = new MaterialRepository(nHibernateSession);

                var cenTemporal = new LineaPrestamoCEN(lineaRepository, prestamoRepository, materialRepository);

                cenTemporal.Crear(model.PrestamoId, model.MaterialId, model.DiasEstimados);

                tx.Commit();
                SessionClose();
                return RedirectToAction("Details", "Prestamo", new { id = model.PrestamoId });
            }
            catch (Exception ex) {
                if (tx != null && tx.IsActive)
                    tx.Rollback();
                SessionClose();

                SessionInitialize();
                ViewBag.MaterialId = new SelectList(_materialCEN.ObtenerTodos(), "Id", "Nombre", model.MaterialId);
                SessionClose();

                ModelState.AddModelError("", "Error al añadir el material: " + ex.Message);
                return View(model);
            }
        }
        // GET: LineaPrestamoController/Delete/5
        public ActionResult Delete(int id) {
            return View();
        }

        // POST: LineaPrestamoController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, long prestamoId) {
            NHibernate.ITransaction tx = null;
            try {
                SessionInitialize();

                var campoSesion = typeof(BasicController).GetField("sessionInside", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                var nHibernateSession = (NHibernate.ISession)campoSesion.GetValue(this);

                tx = nHibernateSession.BeginTransaction();

                var lineaRepository = new LineaPrestamoRepository(nHibernateSession);
                var prestamoRepository = new PrestamoRepository(nHibernateSession);
                var materialRepository = new MaterialRepository(nHibernateSession);

                var cenTemporal = new LineaPrestamoCEN(lineaRepository, prestamoRepository, materialRepository);


                cenTemporal.Eliminar(id);

                tx.Commit();
                SessionClose();
            }
            catch {
                if (tx != null && tx.IsActive)
                    tx.Rollback();
                SessionClose();
            }

            // Volvemos a la pantalla del préstamo padre
            return RedirectToAction("Details", "Prestamo", new { id = prestamoId });
        }
    }
}

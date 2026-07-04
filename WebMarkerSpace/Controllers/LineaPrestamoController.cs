// "Copyright (c) YOUR_COMPANY. All rights reserved."

using ApplicationCore.Domain.CEN;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using WebMarkerSpace.Models;

namespace WebMarkerSpace.Controllers {
    [Authorize]
    public class LineaPrestamoController : Controller {
        private readonly LineaPrestamoCEN _lineaPrestamoCEN;
        private readonly MaterialCEN _materialCEN;
        private readonly NHibernate.ISession _session;

        public LineaPrestamoController(LineaPrestamoCEN lineaPrestamoCEN, MaterialCEN materialCEN, NHibernate.ISession session) {
            _lineaPrestamoCEN = lineaPrestamoCEN;
            _materialCEN = materialCEN;
            _session = session;
        }

        // GET: LineaPrestamoController/Create
        public ActionResult Create(long prestamoId) {
            // Pasamos el ID del préstamo padre mediante el modelo
            var model = new LineaPrestamoViewModel { PrestamoId = prestamoId };

            // Desplegable de materiales disponibles
            var materiales = _materialCEN.ObtenerTodos();
            ViewBag.MaterialId = new SelectList(materiales, "Id", "Nombre");

            return View(model);
        }

        // POST: LineaPrestamoController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(LineaPrestamoViewModel model) {
            using var tx = _session.BeginTransaction();
            try {
                _lineaPrestamoCEN.Crear(model.PrestamoId, model.MaterialId, model.DiasEstimados);
                tx.Commit();
                return RedirectToAction("Details", "Prestamo", new { id = model.PrestamoId });
            }
            catch (Exception ex) {
                tx.Rollback();
                ViewBag.MaterialId = new SelectList(_materialCEN.ObtenerTodos(), "Id", "Nombre", model.MaterialId);
                ModelState.AddModelError("", "Error al añadir el material: " + ex.Message);
                return View(model);
            }
        }

        // POST: LineaPrestamoController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, long prestamoId) {
            using var tx = _session.BeginTransaction();
            try {
                _lineaPrestamoCEN.Eliminar(id);
                tx.Commit();
            }
            catch {
                tx.Rollback();
            }

            // Volvemos a la pantalla del préstamo padre
            return RedirectToAction("Details", "Prestamo", new { id = prestamoId });
        }
    }
}

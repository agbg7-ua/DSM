// "Copyright (c) YOUR_COMPANY. All rights reserved."

using ApplicationCore.Domain.CEN;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Security.Claims;
using WebMarkerSpace.Models;

namespace WebMarkerSpace.Controllers {
    [Authorize]
    public class LineaPrestamoController : Controller {
        private readonly LineaPrestamoCEN _lineaPrestamoCEN;
        private readonly MaterialCEN _materialCEN;
        private readonly PrestamoCEN _prestamoCEN;
        private readonly NHibernate.ISession _session;

        public LineaPrestamoController(LineaPrestamoCEN lineaPrestamoCEN, MaterialCEN materialCEN, PrestamoCEN prestamoCEN, NHibernate.ISession session) {
            _lineaPrestamoCEN = lineaPrestamoCEN;
            _materialCEN = materialCEN;
            _prestamoCEN = prestamoCEN;
            _session = session;
        }

        // Un usuario normal solo puede tocar las líneas de SUS PROPIOS préstamos.
        private bool PuedeGestionar(long prestamoId) {
            if (User.IsInRole("Administrador")) return true;

            var prestamo = _prestamoCEN.ObtenerPorId(prestamoId);
            if (prestamo == null) return false;

            long miId = long.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            return prestamo.Usuario.Id == miId;
        }

        // GET: LineaPrestamoController/Create
        public ActionResult Create(long prestamoId) {
            if (!PuedeGestionar(prestamoId)) {
                return Forbid();
            }

            var model = new LineaPrestamoViewModel { PrestamoId = prestamoId };

            var materiales = _materialCEN.ObtenerTodos();
            ViewBag.MaterialId = new SelectList(materiales, "Id", "Nombre");

            return View(model);
        }

        // POST: LineaPrestamoController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(LineaPrestamoViewModel model) {
            if (!PuedeGestionar(model.PrestamoId)) {
                return Forbid();
            }

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
            if (!PuedeGestionar(prestamoId)) {
                return Forbid();
            }

            using var tx = _session.BeginTransaction();
            try {
                _lineaPrestamoCEN.Eliminar(id);
                tx.Commit();
            }
            catch {
                tx.Rollback();
            }

            return RedirectToAction("Details", "Prestamo", new { id = prestamoId });
        }
    }
}

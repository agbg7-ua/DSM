
using ApplicationCore.Domain.CEN;
using ApplicationCore.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Linq;
using System.Security.Claims;
using WebMarkerSpace.Assemblers;
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

        private bool PuedeGestionar(long prestamoId) {
            if (User.IsInRole("Administrador")) return true;

            var prestamo = _prestamoCEN.ObtenerPorId(prestamoId);
            if (prestamo == null) return false;

            long miId = long.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            return prestamo.Usuario.Id == miId;
        }

        private SelectList MaterialesDisponibles(long? seleccionado = null) {

            var disponibles = _materialCEN.ObtenerTodos().Where(m => m.Estado == EstadoMaterial.Disponible);
            return new SelectList(disponibles, "Id", "Nombre", seleccionado);
        }

        public ActionResult Create(long prestamoId) {
            if (!PuedeGestionar(prestamoId)) {
                return Forbid();
            }

            var model = new LineaPrestamoViewModel { PrestamoId = prestamoId };
            ViewBag.MaterialId = MaterialesDisponibles();

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(LineaPrestamoViewModel model) {
            if (!PuedeGestionar(model.PrestamoId)) {
                return Forbid();
            }

            var prestamo = _prestamoCEN.ObtenerPorId(model.PrestamoId);
            var material = _materialCEN.ObtenerPorId(model.MaterialId);

            if (prestamo == null || material == null) {
                return NotFound();
            }
            if (material.Estado != EstadoMaterial.Disponible) {
                ModelState.AddModelError("", "Ese material ya no está disponible.");
                ViewBag.MaterialId = MaterialesDisponibles(model.MaterialId);
                return View(model);
            }

            using var tx = _session.BeginTransaction();
            try {
                _lineaPrestamoCEN.Crear(model.PrestamoId, model.MaterialId, model.DiasEstimados);

                _materialCEN.Modificar(material.Id, material.Nombre, material.Descripcion, EstadoMaterial.Prestado, material.Categoria, material.Imagen, prestamo.Usuario.Id);
                tx.Commit();
                return RedirectToAction("Details", "Prestamo", new { id = model.PrestamoId });
            }
            catch (Exception ex) {
                tx.Rollback();
                ViewBag.MaterialId = MaterialesDisponibles(model.MaterialId);
                ModelState.AddModelError("", "Error al añadir el material: " + ex.Message);
                return View(model);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, long prestamoId) {
            if (!PuedeGestionar(prestamoId)) {
                return Forbid();
            }

            var linea = _lineaPrestamoCEN.ObtenerPorId(id);
            string? error = null;

            using var tx = _session.BeginTransaction();
            try {
                _lineaPrestamoCEN.Eliminar(id);

                if (linea != null && linea.Material.Estado == EstadoMaterial.Prestado) {
                    var material = linea.Material;
                    _materialCEN.Modificar(material.Id, material.Nombre, material.Descripcion, EstadoMaterial.Disponible, material.Categoria, material.Imagen, null);
                }

                tx.Commit();
            }
            catch {
                tx.Rollback();
                error = "No se pudo eliminar el material del préstamo.";
            }

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest") {
                var prestamoActualizado = _prestamoCEN.ObtenerPorId(prestamoId);
                if (prestamoActualizado == null) {
                    return NotFound();
                }
                var modelActualizado = new PrestamoAssembler().ConvertirENToViewModel(prestamoActualizado);
                ViewBag.MensajeError = error;
                return PartialView("~/Views/Prestamo/_PrestamoDetallesPartial.cshtml", modelActualizado);
            }

            return RedirectToAction("Details", "Prestamo", new { id = prestamoId });
        }
    }
}


using ApplicationCore.Domain.CEN;
using ApplicationCore.Domain.CP;
using ApplicationCore.Domain.EN;
using ApplicationCore.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using WebMarkerSpace.Assemblers;
using WebMarkerSpace.Extensions;
using WebMarkerSpace.Models;

namespace WebMarkerSpace.Controllers {

    [Authorize]
    public class PrestamoController : Controller {
        private readonly PrestamoCEN _prestamoCEN;
        private readonly UsuarioCEN _usuarioCEN;
        private readonly LineaPrestamoCEN _lineaPrestamoCEN;
        private readonly MaterialCEN _materialCEN;
        private readonly NHibernate.ISession _session;
        private readonly IStringLocalizer<SharedResource> _localizer;

        public PrestamoController(PrestamoCEN prestamoCEN, UsuarioCEN usuarioCEN, LineaPrestamoCEN lineaPrestamoCEN, MaterialCEN materialCEN, NHibernate.ISession session, IStringLocalizer<SharedResource> localizer) {
            _prestamoCEN = prestamoCEN;
            _usuarioCEN = usuarioCEN;
            _lineaPrestamoCEN = lineaPrestamoCEN;
            _materialCEN = materialCEN;
            _session = session;
            _localizer = localizer;
        }

        private long ObtenerIdUsuarioActual() {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier);
            return claim != null ? long.Parse(claim.Value) : 0;
        }

        public ActionResult Index(EstadoPrestamo? estado, long? usuarioId) {
            bool esAdmin = User.IsInRole("Administrador");
            IEnumerable<Prestamo> prestamos = _prestamoCEN.ObtenerTodos();

            if (!esAdmin) {
                long miId = ObtenerIdUsuarioActual();
                prestamos = prestamos.Where(p => p.Usuario.Id == miId);
            } else if (usuarioId.HasValue) {
                prestamos = prestamos.Where(p => p.Usuario.Id == usuarioId.Value);
            }

            if (estado.HasValue) {
                prestamos = prestamos.Where(p => p.Estado == estado.Value);
            }

            IEnumerable<PrestamoViewModel> modelList = new PrestamoAssembler().ConvertirListaENToViewModel(prestamos.ToList());

            ViewBag.EsAdmin = esAdmin;

            ViewBag.FiltroEstado = new SelectList(
                Enum.GetValues(typeof(EstadoPrestamo)).Cast<EstadoPrestamo>()
                    .Select(e => new SelectListItem(_localizer.Localize(e), e.ToString(), e.Equals(estado))),
                "Value", "Text", estado);
            if (esAdmin) {
                ViewBag.FiltroUsuarioId = new SelectList(_usuarioCEN.ObtenerTodos(), "Id", "Nombre", usuarioId);
            }

            if (EsPeticionAjax()) {
                return PartialView("_PrestamoListPartial", modelList);
            }

            return View(modelList);
        }

        public ActionResult Details(int id) {
            Prestamo prestamoEN = _prestamoCEN.ObtenerPorId(id);
            if (prestamoEN == null) {
                return NotFound();
            }

            bool esAdmin = User.IsInRole("Administrador");
            if (!esAdmin && prestamoEN.Usuario.Id != ObtenerIdUsuarioActual()) {
                return Forbid();
            }

            PrestamoViewModel model = new PrestamoAssembler().ConvertirENToViewModel(prestamoEN);
            ViewBag.MensajeError = TempData["Error"];
            return View(model);
        }

        public ActionResult Create(long? materialId) {
            bool esAdmin = User.IsInRole("Administrador");

            var model = new PrestamoViewModel {
                FechaCreacion = DateTime.Now,
                Estado = EstadoPrestamo.Pendiente,
                TotalDias = 7
            };

            if (esAdmin) {
                ViewBag.UsuarioId = new SelectList(_usuarioCEN.ObtenerTodos(), "Id", "Nombre");
            } else {

                model.UsuarioId = ObtenerIdUsuarioActual();
            }

            ViewBag.EsAdmin = esAdmin;
            ViewBag.MaterialId = materialId;
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(PrestamoViewModel model, long? materialId) {
            bool esAdmin = User.IsInRole("Administrador");
            if (!esAdmin) {

                model.UsuarioId = ObtenerIdUsuarioActual();
                model.Estado = EstadoPrestamo.Pendiente;
            }

            Material? material = null;
            if (materialId.HasValue) {
                material = _materialCEN.ObtenerPorId(materialId.Value);
                if (material == null || material.Estado != EstadoMaterial.Disponible) {
                    ModelState.AddModelError("", "El material seleccionado ya no está disponible.");
                }
            }

            if (!ModelState.IsValid) {
                if (esAdmin) {
                    ViewBag.UsuarioId = new SelectList(_usuarioCEN.ObtenerTodos(), "Id", "Nombre", model.UsuarioId);
                }
                ViewBag.EsAdmin = esAdmin;
                ViewBag.MaterialId = materialId;
                return View(model);
            }

            using var tx = _session.BeginTransaction();
            try {
                long nuevoPrestamoId = _prestamoCEN.Crear(model.UsuarioId, model.FechaCreacion, model.Estado, model.TotalDias);

                if (material != null) {
                    _lineaPrestamoCEN.Crear(nuevoPrestamoId, material.Id, model.TotalDias);

                    _materialCEN.Modificar(material.Id, material.Nombre, material.Descripcion, EstadoMaterial.Prestado, material.Categoria, material.Imagen, model.UsuarioId);
                }

                tx.Commit();
                return RedirectToAction(nameof(Details), new { id = nuevoPrestamoId });
            }
            catch (Exception ex) {
                tx.Rollback();

                if (esAdmin) {
                    ViewBag.UsuarioId = new SelectList(_usuarioCEN.ObtenerTodos(), "Id", "Nombre", model.UsuarioId);
                }
                ViewBag.EsAdmin = esAdmin;
                ViewBag.MaterialId = materialId;

                ModelState.AddModelError("", "Error al crear el préstamo: " + ex.Message);
                return View(model);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Devolver(long id, [FromServices] CasosProceso casosProceso) {
            var prestamoEN = _prestamoCEN.ObtenerPorId(id);
            if (prestamoEN == null) {
                return NotFound();
            }

            bool esAdmin = User.IsInRole("Administrador");
            if (!esAdmin && prestamoEN.Usuario.Id != ObtenerIdUsuarioActual()) {
                return Forbid();
            }

            string? error = null;
            try {
                casosProceso.DevolverMaterial(id);
            }
            catch (Exception) {
                error = "No se pudo marcar el préstamo como devuelto.";
            }

            if (EsPeticionAjax()) {
                var actualizado = _prestamoCEN.ObtenerPorId(id);
                if (actualizado == null) {
                    return NotFound();
                }
                var modelActualizado = new PrestamoAssembler().ConvertirENToViewModel(actualizado);
                ViewBag.MensajeError = error;
                return PartialView("_PrestamoDetallesPartial", modelActualizado);
            }

            TempData["Error"] = error;
            return RedirectToAction(nameof(Details), new { id });
        }

        [Authorize(Roles = "Administrador")]
        public ActionResult Edit(int id) {
            var prestamoEN = _prestamoCEN.ObtenerPorId(id);
            if (prestamoEN == null) {
                return NotFound();
            }
            var model = new PrestamoAssembler().ConvertirENToViewModel(prestamoEN);
            ViewBag.UsuarioId = new SelectList(_usuarioCEN.ObtenerTodos(), "Id", "Nombre", model.UsuarioId);
            return View(model);
        }

        [HttpPost]
        [Authorize(Roles = "Administrador")]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, PrestamoViewModel model) {
            using var tx = _session.BeginTransaction();
            try {
                _prestamoCEN.Modificar(model.Id, model.UsuarioId, model.FechaCreacion, model.Estado, model.TotalDias);
                tx.Commit();
                return RedirectToAction(nameof(Details), new { id = model.Id });
            }
            catch (Exception ex) {
                tx.Rollback();
                ViewBag.UsuarioId = new SelectList(_usuarioCEN.ObtenerTodos(), "Id", "Nombre", model.UsuarioId);
                ModelState.AddModelError("", "Error al modificar: " + ex.Message);
                return View(model);
            }
        }

        [Authorize(Roles = "Administrador")]
        public ActionResult Delete(int id) {
            var prestamoEN = _prestamoCEN.ObtenerPorId(id);
            if (prestamoEN == null) {
                return NotFound();
            }
            var model = new PrestamoAssembler().ConvertirENToViewModel(prestamoEN);
            return View(model);
        }

        [HttpPost]
        [Authorize(Roles = "Administrador")]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, PrestamoViewModel model) {
            using var tx = _session.BeginTransaction();
            try {
                _prestamoCEN.Eliminar(id);
                tx.Commit();

                if (EsPeticionAjax()) {
                    return Json(new { success = true });
                }
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex) {
                tx.Rollback();

                if (EsPeticionAjax()) {
                    return Json(new { success = false, message = "Error al eliminar: " + ex.Message });
                }
                ModelState.AddModelError("", "Error al eliminar: " + ex.Message);
                return View(model);
            }
        }

        private bool EsPeticionAjax() {
            return Request.Headers["X-Requested-With"] == "XMLHttpRequest";
        }
    }
}

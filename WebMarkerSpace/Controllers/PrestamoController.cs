// "Copyright (c) YOUR_COMPANY. All rights reserved."

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
    // Cualquier usuario logueado (Administrador o Usuario) puede ver y pedir
    // préstamos. Editar/eliminar préstamos ya existentes queda restringido a
    // Administrador (ver atributos concretos más abajo).
    [Authorize]
    public class PrestamoController : Controller {
        private readonly PrestamoCEN _prestamoCEN;
        private readonly UsuarioCEN _usuarioCEN; // Lo necesitamos para los desplegables de usuarios
        private readonly LineaPrestamoCEN _lineaPrestamoCEN;
        private readonly MaterialCEN _materialCEN;
        private readonly NHibernate.ISession _session;
        private readonly IStringLocalizer<SharedResource> _localizer;

        // OJO: CasosProceso (y su IUnitOfWork) NO se inyectan aquí por
        // constructor a propósito: IUnitOfWork abre una transacción en cuanto
        // se construye, y si estuviera en el constructor se abriría en TODAS
        // las acciones de este controlador, chocando con nuestras propias
        // transacciones manuales de Create/Edit/Delete. Por eso en Devolver()
        // se pide como parámetro de acción ([FromServices]), solo cuando hace falta.
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

        // GET: Prestamo
        // Un usuario normal solo ve SUS préstamos; un Administrador los ve todos
        // y además puede filtrar por usuario. Ambos pueden filtrar por estado.
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
            // Igual que en MaterialController.Index: el <option> debe mostrar
            // el nombre TRADUCIDO del estado (Enum.EstadoPrestamo.* en
            // SharedResource), no Enum.GetValues(...).ToString() en crudo.
            ViewBag.FiltroEstado = new SelectList(
                Enum.GetValues(typeof(EstadoPrestamo)).Cast<EstadoPrestamo>()
                    .Select(e => new SelectListItem(_localizer.Localize(e), e.ToString(), e.Equals(estado))),
                "Value", "Text", estado);
            if (esAdmin) {
                ViewBag.FiltroUsuarioId = new SelectList(_usuarioCEN.ObtenerTodos(), "Id", "Nombre", usuarioId);
            }

            // Peticiones AJAX (filtro dinámico) solo necesitan la tabla.
            if (EsPeticionAjax()) {
                return PartialView("_PrestamoListPartial", modelList);
            }

            return View(modelList);
        }

        // GET: PrestamoController/Details/5
        // Un usuario normal solo puede ver el detalle de sus propios préstamos.
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

        // GET: PrestamoController/Create
        // materialId es opcional: si venimos del botón "Solicitar préstamo" de un
        // material concreto, lo añadimos automáticamente como primera línea del préstamo.
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
                // Un usuario normal solo puede pedir préstamos para sí mismo.
                model.UsuarioId = ObtenerIdUsuarioActual();
            }

            ViewBag.EsAdmin = esAdmin;
            ViewBag.MaterialId = materialId;
            return View(model);
        }

        // POST: PrestamoController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(PrestamoViewModel model, long? materialId) {
            bool esAdmin = User.IsInRole("Administrador");
            if (!esAdmin) {
                // No nos fiamos de lo que llegue del formulario: un usuario normal
                // siempre pide el préstamo para sí mismo y siempre empieza Pendiente.
                model.UsuarioId = ObtenerIdUsuarioActual();
                model.Estado = EstadoPrestamo.Pendiente;
            }

            // Un material solo se puede prestar si está Disponible (no si ya
            // está Prestado, En Mantenimiento o Roto).
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
                    // El material pasa a estar Prestado y queda asignado a quien lo pide.
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

        // POST: PrestamoController/Devolver/5
        // Marca el préstamo como Devuelto y libera todos sus materiales (vuelven
        // a Disponible y se les quita el usuario asignado). Lo puede hacer un
        // Administrador o el propio dueño del préstamo.
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

        // GET: PrestamoController/Edit/5
        // Editar el estado/datos de un préstamo ya existente es una tarea de
        // administración (marcar como Activo/Devuelto/Retrasado, corregir días, etc.).
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

        // POST: PrestamoController/Edit/5
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

        // GET: PrestamoController/Delete/5
        [Authorize(Roles = "Administrador")]
        public ActionResult Delete(int id) {
            var prestamoEN = _prestamoCEN.ObtenerPorId(id);
            if (prestamoEN == null) {
                return NotFound();
            }
            var model = new PrestamoAssembler().ConvertirENToViewModel(prestamoEN);
            return View(model);
        }

        // POST: PrestamoController/Delete/5
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

        // Indica si la petición viene de una llamada AJAX (fetch/$.ajax) en vez
        // de una navegación normal del navegador.
        private bool EsPeticionAjax() {
            return Request.Headers["X-Requested-With"] == "XMLHttpRequest";
        }
    }
}

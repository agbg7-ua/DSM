// "Copyright (c) YOUR_COMPANY. All rights reserved."

using ApplicationCore.Domain.CEN;
using ApplicationCore.Domain.EN;
using ApplicationCore.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using WebMarkerSpace.Assemblers;
using WebMarkerSpace.Models;

namespace WebMarkerSpace.Controllers {
    public class MaterialController : Controller {
        private readonly MaterialCEN _materialCEN;
        private readonly IWebHostEnvironment _webHost;
        private readonly NHibernate.ISession _session;

        public MaterialController(MaterialCEN materialCEN, IWebHostEnvironment webHost, NHibernate.ISession session) {
            _materialCEN = materialCEN;
            _webHost = webHost;
            _session = session;
        }

        // GET: MaterialController
        // Búsqueda con filtro por nombre, estado y categoría.
        [AllowAnonymous]
        public ActionResult Index(string? nombre, EstadoMaterial? estado, CategoriaMaterial? categoria) {
            IEnumerable<Material> materiales = _materialCEN.ObtenerTodos();

            if (!string.IsNullOrWhiteSpace(nombre)) {
                materiales = materiales.Where(m => m.Nombre.Contains(nombre, StringComparison.OrdinalIgnoreCase));
            }
            if (estado.HasValue) {
                materiales = materiales.Where(m => m.Estado == estado.Value);
            }
            if (categoria.HasValue) {
                materiales = materiales.Where(m => m.Categoria == categoria.Value);
            }

            IEnumerable<MaterialViewModel> listMats = new MaterialAssembler().ConvertirListaENToViewModel(materiales.ToList());

            ViewBag.FiltroNombre = nombre;
            ViewBag.FiltroEstado = new SelectList(Enum.GetValues(typeof(EstadoMaterial)), estado);
            ViewBag.FiltroCategoria = new SelectList(Enum.GetValues(typeof(CategoriaMaterial)), categoria);
            return View(listMats);
        }

        // GET: MaterialController/Details/5
        [AllowAnonymous]
        public ActionResult Details(int id) {
            var materialEN = _materialCEN.ObtenerPorId(id);
            if (materialEN == null) {
                return NotFound();
            }
            var model = new MaterialAssembler().ConvertirENToViewModel(materialEN);
            return View(model);
        }

        // GET: MaterialController/Create
        [Authorize(Roles = "Administrador")]
        public ActionResult Create() {
            ViewBag.Categoria = new SelectList(Enum.GetValues(typeof(CategoriaMaterial)));
            return View();
        }

        // POST: MaterialController/Create
        [HttpPost]
        [Authorize(Roles = "Administrador")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(MaterialViewModel mat) {
            if (mat.Fichero != null && mat.Fichero.Length > 0) {
                var fileName = Path.GetFileName(mat.Fichero.FileName).Trim();
                var directory = _webHost.WebRootPath + "/Images";
                var path = Path.Combine(directory, fileName);

                if (!Directory.Exists(directory))
                    Directory.CreateDirectory(directory);
                using (var stream = System.IO.File.Create(path)) {
                    await mat.Fichero.CopyToAsync(stream);
                }
                mat.Imagen = "/Images/" + fileName;
            }

            using var tx = _session.BeginTransaction();
            try {
                _materialCEN.Crear(mat.Nombre, mat.Descripcion, mat.Estado, mat.Categoria, mat.Imagen ?? string.Empty, null);
                tx.Commit();
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex) {
                tx.Rollback();
                ViewBag.Categoria = new SelectList(Enum.GetValues(typeof(CategoriaMaterial)), mat.Categoria);
                ModelState.AddModelError("", "Error al crear: " + ex.Message);
                return View(mat);
            }
        }

        // GET: MaterialController/Edit/5
        [Authorize(Roles = "Administrador")]
        public ActionResult Edit(int id) {
            var materialEN = _materialCEN.ObtenerPorId(id);
            if (materialEN == null) {
                return NotFound();
            }
            var model = new MaterialAssembler().ConvertirENToViewModel(materialEN);
            ViewBag.Categoria = new SelectList(Enum.GetValues(typeof(CategoriaMaterial)), model.Categoria);
            return View(model);
        }

        // POST: MaterialController/Edit/5
        [HttpPost]
        [Authorize(Roles = "Administrador")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(int id, MaterialViewModel mat) {
            if (mat.Fichero != null && mat.Fichero.Length > 0) {
                var fileName = Path.GetFileName(mat.Fichero.FileName).Trim();
                var directory = _webHost.WebRootPath + "/Images";
                var path = Path.Combine(directory, fileName);

                if (!Directory.Exists(directory))
                    Directory.CreateDirectory(directory);
                using (var stream = System.IO.File.Create(path)) {
                    await mat.Fichero.CopyToAsync(stream);
                }
                mat.Imagen = "/Images/" + fileName;
            }

            using var tx = _session.BeginTransaction();
            try {
                // El admin puede cambiar libremente Estado/Categoría, pero no
                // tocamos aquí quién lo tiene prestado (UsuarioId): eso solo lo
                // gestiona el flujo de préstamos/devoluciones.
                var actual = _materialCEN.ObtenerPorId(id);
                long? usuarioAsignadoId = actual?.UsuarioAsignado?.Id;

                _materialCEN.Modificar(id, mat.Nombre, mat.Descripcion, mat.Estado, mat.Categoria, mat.Imagen ?? string.Empty, usuarioAsignadoId);
                tx.Commit();
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex) {
                tx.Rollback();
                ViewBag.Categoria = new SelectList(Enum.GetValues(typeof(CategoriaMaterial)), mat.Categoria);
                ModelState.AddModelError("", "Error al editar: " + ex.Message);
                return View(mat);
            }
        }

        // GET: MaterialController/Delete/5
        [Authorize(Roles = "Administrador")]
        public ActionResult Delete(int id) {
            var materialEN = _materialCEN.ObtenerPorId(id);
            if (materialEN == null) {
                return NotFound();
            }
            var model = new MaterialAssembler().ConvertirENToViewModel(materialEN);
            return View(model);
        }

        // POST: MaterialController/Delete/5
        [HttpPost]
        [Authorize(Roles = "Administrador")]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, MaterialViewModel mat) {
            using var tx = _session.BeginTransaction();
            try {
                _materialCEN.Eliminar(id);
                tx.Commit();
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex) {
                tx.Rollback();
                ModelState.AddModelError("", "Error al eliminar: " + ex.Message);
                return View(mat);
            }
        }
    }
}

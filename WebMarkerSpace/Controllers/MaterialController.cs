// "Copyright (c) YOUR_COMPANY. All rights reserved."

using ApplicationCore.Domain.CEN;
using ApplicationCore.Domain.EN;
using Infrastructure.NHibernate.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebMarkerSpace.Assemblers;
using WebMarkerSpace.Models;
using System.IO;

namespace WebMarkerSpace.Controllers {
    public class MaterialController : BasicController {
        private readonly MaterialCEN _materialCEN;
        private readonly IWebHostEnvironment _webHost;

        // 2. El constructor recibe automáticamente las dependencias desde Program.cs
        public MaterialController(MaterialCEN materialCEN, IWebHostEnvironment webHost) {
            _materialCEN = materialCEN;
            _webHost = webHost;
        }
        // GET: MaterialController
        [AllowAnonymous]
        public ActionResult Index() {
            SessionInitialize();

            IList<Material> materiales = _materialCEN.ObtenerTodos();
            IEnumerable<MaterialViewModel> listMats = new MaterialAssembler().ConvertirListaENToViewModel(materiales);
            SessionClose();
            return View(listMats);
        }

        // GET: MaterialController/Details/5
        [AllowAnonymous]
        public ActionResult Details(int id) {
            SessionInitialize();

            var materialEN = _materialCEN.ObtenerPorId(id);

            if (materialEN == null) {
                SessionClose();
                return NotFound();
            }

            var model = new MaterialAssembler().ConvertirENToViewModel(materialEN);

            SessionClose();
            return View(model);
        }

        // GET: MaterialController/Create
        [Authorize(Roles = "Administrador")]
        public ActionResult Create() {
            return View();
        }

        // POST: MaterialController/Create
        [HttpPost]
        [Authorize(Roles = "Administrador")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(MaterialViewModel mat) {
            NHibernate.ITransaction tx = null;
            string fileName = "";
            // 1. Lógica de guardado de imagen
            if (mat.Fichero != null && mat.Fichero.Length > 0) {
                fileName = Path.GetFileName(mat.Fichero.FileName).Trim();
                string directory = _webHost.WebRootPath + "/Images";
                string path = Path.Combine(directory, fileName);

                if (!Directory.Exists(directory))
                    Directory.CreateDirectory(directory);
                using (var stream = System.IO.File.Create(path)) {
                    await mat.Fichero.CopyToAsync(stream);
                }
                mat.Imagen = "/Images/" + fileName; // Guardamos la ruta relativa para la BD
            }
            try {
                SessionInitialize();
                var campoSesion = typeof(BasicController).GetField("sessionInside", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                var nHibernateSession = (NHibernate.ISession)campoSesion.GetValue(this);
                tx = nHibernateSession.BeginTransaction();

                var materialRepository = new MaterialRepository(nHibernateSession);
                var usuarioRepository = new UsuarioRepository(nHibernateSession);
                var cenTemporal = new MaterialCEN(materialRepository, usuarioRepository);

                bool disponibleAutomatico = (mat.Estado == ApplicationCore.Domain.Enums.EstadoMaterial.Disponible);

                // Asegurar que se pasa siempre una cadena no nula para el parámetro Imagen
                cenTemporal.Crear(mat.Nombre, mat.Descripcion, mat.Estado, disponibleAutomatico, mat.Imagen ?? string.Empty, null);

                tx.Commit();

                SessionClose();
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex) {
                SessionClose();
                return View(mat);
            }
        }

        // GET: MaterialController/Edit/5
        [Authorize(Roles = "Administrador")]
        public ActionResult Edit(int id) {
            SessionInitialize();
            var materialEN = _materialCEN.ObtenerPorId(id);

            if (materialEN == null) {
                SessionClose();
                return NotFound();
            }

            var model = new MaterialAssembler().ConvertirENToViewModel(materialEN);

            SessionClose();
            return View(model);
        }

        // POST: MaterialController/Edit/5
        [HttpPost]
        [Authorize(Roles = "Administrador")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(int id, MaterialViewModel mat) {
            NHibernate.ITransaction tx = null;
            string fileName = "";
            try {
                // Procesar posible nueva imagen subida
                if (mat.Fichero != null && mat.Fichero.Length > 0) {
                    fileName = Path.GetFileName(mat.Fichero.FileName).Trim();
                    string directory = _webHost.WebRootPath + "/Images";
                    string path = Path.Combine(directory, fileName);

                    if (!Directory.Exists(directory))
                        Directory.CreateDirectory(directory);
                    using (var stream = System.IO.File.Create(path)) {
                        await mat.Fichero.CopyToAsync(stream);
                    }
                    mat.Imagen = "/Images/" + fileName; // Actualizamos la ruta para la BD
                }

                SessionInitialize();

                var campoSesion = typeof(BasicController).GetField("sessionInside", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                var nHibernateSession = (NHibernate.ISession)campoSesion.GetValue(this);


                tx = nHibernateSession.BeginTransaction();

                var materialRepository = new MaterialRepository(nHibernateSession);
                var usuarioRepository = new UsuarioRepository(nHibernateSession); 
                var cenTemporal = new MaterialCEN(materialRepository, usuarioRepository);

                bool disponibleAutomatico = (mat.Estado == ApplicationCore.Domain.Enums.EstadoMaterial.Disponible);

                // Pasar siempre una cadena no nula para Imagen
                cenTemporal.Modificar(mat.Id, mat.Nombre, mat.Descripcion, mat.Estado, disponibleAutomatico);
                

                tx.Commit();

                SessionClose();
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex) {
                SessionClose();
                return View(mat);
            }
        }

        // GET: MaterialController/Delete/5
        [Authorize(Roles = "Administrador")]
        public ActionResult Delete(int id) {
            SessionInitialize();

            var materialEN = _materialCEN.ObtenerPorId(id);

            if (materialEN == null) {
                SessionClose();
                return NotFound();
            }

            var model = new MaterialAssembler().ConvertirENToViewModel(materialEN);

            SessionClose();
            return View(model);
        }

        // POST: MaterialController/Delete/5
        [HttpPost]
        [Authorize(Roles = "Administrador")]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, MaterialViewModel mat) {
            NHibernate.ITransaction tx = null;
            try {
                SessionInitialize();

                var campoSesion = typeof(BasicController).GetField("sessionInside", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                var nHibernateSession = (NHibernate.ISession)campoSesion.GetValue(this);

                tx = nHibernateSession.BeginTransaction();

                var materialRepository = new MaterialRepository(nHibernateSession);
                var usuarioRepository = new UsuarioRepository(nHibernateSession);
                var cenTemporal = new MaterialCEN(materialRepository, usuarioRepository);

                cenTemporal.Eliminar(id);
                tx.Commit();

                SessionClose();
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex) {
                SessionClose();
                return View(mat);
            }
        }
    }
}

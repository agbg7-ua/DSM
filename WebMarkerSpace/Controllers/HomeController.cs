using ApplicationCore.Domain.CEN;
using ApplicationCore.Domain.EN;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using WebMarkerSpace.Models;

namespace WebMarkerSpace.Controllers {
    public class HomeController : Controller {
        private readonly ILogger<HomeController> _logger;
        private readonly MaterialCEN _materialCEN;

        public HomeController(ILogger<HomeController> logger, MaterialCEN materialCEN) {
            _logger = logger;
            _materialCEN = materialCEN;
        }

        public IActionResult Index() {
            // La portada solo muestra un adelanto (los últimos materiales
            // dados de alta); el catálogo completo con filtros vive en
            // /Material, así no duplicamos la misma tabla en dos sitios.
            IList<Material> ultimosMateriales = _materialCEN.ObtenerTodos()
                .OrderByDescending(m => m.Id)
                .Take(3)
                .ToList();
            return View(ultimosMateriales);
        }

        public IActionResult Privacy() {
            return View();
        }

        public IActionResult AccessDenied() {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error() {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}

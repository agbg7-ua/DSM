// "Copyright (c) YOUR_COMPANY. All rights reserved."

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;

namespace WebMarkerSpace.Controllers {

    /// <summary>
    /// Controlador dedicado a la internacionalización (i18n): permite que la
    /// persona usuaria alterne entre español e inglés desde cualquier página
    /// mediante el toggle de <c>Views/Shared/_LanguageToggle.cshtml</c>.
    ///
    /// La elección se guarda en la cookie estándar de cultura de ASP.NET Core
    /// (<see cref="CookieRequestCultureProvider"/>), que <c>UseRequestLocalization</c>
    /// (configurado en Program.cs) lee en cada petición posterior — por
    /// delante de la detección automática por cabecera Accept-Language del
    /// navegador. Es decir: una vez elegido el idioma a mano, la preferencia
    /// del navegador deja de tener efecto para esa persona (hasta que borre
    /// cookies o vuelva a cambiarlo).
    ///
    /// Funciona también SIN JavaScript: el toggle es un checkbox dentro de
    /// un &lt;form&gt; normal con un botón de envío dentro de un
    /// &lt;noscript&gt;; con JavaScript, cambiar el checkbox envía el
    /// formulario automáticamente.
    /// </summary>
    [AllowAnonymous]
    public class CultureController : Controller {

        // POST: Culture/SetLanguage
        // "ingles" es el valor de un checkbox: el navegador solo lo envía
        // ("true") cuando el toggle está marcado (inglés). Si el toggle está
        // desmarcado, el campo no llega en absoluto y el parámetro toma su
        // valor por defecto (false) => español.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SetLanguage(bool ingles, string? returnUrl) {
            string cultura = ingles ? "en-US" : "es-ES";

            Response.Cookies.Append(
                CookieRequestCultureProvider.DefaultCookieName,
                CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(cultura)),
                new CookieOptions {
                    Expires = DateTimeOffset.UtcNow.AddYears(1),
                    IsEssential = true,
                    SameSite = SameSiteMode.Lax,
                    HttpOnly = false
                });

            // Nunca redirigimos a una URL externa (evita "open redirect"):
            // solo se acepta una ruta local relativa a esta aplicación.
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl)) {
                return LocalRedirect(returnUrl);
            }
            return RedirectToAction("Index", "Home");
        }
    }
}

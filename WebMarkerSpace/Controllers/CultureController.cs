
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;

namespace WebMarkerSpace.Controllers {

    [AllowAnonymous]
    public class CultureController : Controller {

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

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl)) {
                return LocalRedirect(returnUrl);
            }
            return RedirectToAction("Index", "Home");
        }
    }
}

// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

// Accesibilidad: cuando un formulario se envía y vuelve con errores de
// validación, movemos el foco al resumen de errores para que se anuncie de
// inmediato en lectores de pantalla (WCAG 3.3.1 "Identificación de errores"
// y 4.1.3 "Mensajes de estado"), en vez de dejar el foco donde estaba antes
// del envío o al principio de la página.
document.addEventListener('DOMContentLoaded', function () {
  var resumen = document.querySelector('[asp-validation-summary], .validation-summary-errors, #resumen-errores');
  if (resumen && resumen.textContent.trim().length > 0) {
    if (!resumen.hasAttribute('tabindex')) {
      resumen.setAttribute('tabindex', '-1');
    }
    if (!resumen.hasAttribute('role')) {
      resumen.setAttribute('role', 'alert');
    }
    resumen.focus();
  }
});

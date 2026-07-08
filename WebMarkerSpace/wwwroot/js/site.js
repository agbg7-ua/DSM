

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

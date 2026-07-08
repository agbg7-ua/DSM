
(function () {
    "use strict";

    var contenedor = document.getElementById("prestamo-detalle");
    if (!contenedor) {
        return;
    }

    function getAntiForgeryToken() {
        var input = document.querySelector("#af-token-form input[name='__RequestVerificationToken']");
        return input ? input.value : null;
    }

    function enviarFormulario(form) {
        var formData = new FormData(form);
        var token = getAntiForgeryToken();
        if (token && !formData.has("__RequestVerificationToken")) {
            formData.append("__RequestVerificationToken", token);
        }

        return fetch(form.action, {
            method: "POST",
            headers: { "X-Requested-With": "XMLHttpRequest" },
            body: formData
        }).then(function (resp) {
            if (!resp.ok) { throw new Error("HTTP " + resp.status); }
            return resp.text();
        });
    }

    contenedor.addEventListener("submit", function (e) {
        var form = e.target;
        var esDevolver = form.classList.contains("js-form-devolver");
        var esEliminarLinea = form.classList.contains("js-form-eliminar-linea");

        if (!esDevolver && !esEliminarLinea) {
            return;
        }

        e.preventDefault();

        var mensajeConfirmacion = esDevolver
            ? "¿Confirmas que quieres marcar este préstamo como devuelto?"
            : "¿Seguro que quieres quitar este material del préstamo?";
        if (!window.confirm(mensajeConfirmacion)) {
            return;
        }

        var boton = form.querySelector("button[type='submit']");
        if (boton) {
            boton.disabled = true;
        }

        enviarFormulario(form)
            .then(function (html) {
                contenedor.innerHTML = html;
            })
            .catch(function () {
                window.alert("No se pudo completar la acción. Inténtalo de nuevo.");
                if (boton) {
                    boton.disabled = false;
                }
            });
    });
})();

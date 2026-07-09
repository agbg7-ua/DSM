
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

    contenedor.addEventListener("submit", async function (e) {
        var form = e.target;
        var esDevolver = form.classList.contains("js-form-devolver");
        var esEliminarLinea = form.classList.contains("js-form-eliminar-linea");

        if (!esDevolver && !esEliminarLinea) {
            return;
        }

        e.preventDefault();

        var i18n = document.getElementById("prestamo-detalle-i18n");

        if (esDevolver) {
            var resultDevolver = await Swal.fire({
                title: i18n ? i18n.dataset.devolverTitulo : "¿Marcar como devuelto?",
                text: i18n ? i18n.dataset.devolverTexto : "¿Confirmas que quieres marcar este préstamo como devuelto?",
                icon: "question",
                showCancelButton: true,
                confirmButtonText: i18n ? i18n.dataset.devolverBoton : "Sí, marcar como devuelto",
                cancelButtonText: i18n ? i18n.dataset.cancelar : "Cancelar",
                confirmButtonColor: "#198754",
                cancelButtonColor: "#6c757d",
                reverseButtons: true,
                focusCancel: true
            });

            if (!resultDevolver.isConfirmed) {
                return;
            }
        } else {
            var result = await Swal.fire({
                title: i18n ? i18n.dataset.confirmarTitulo : "¿Quitar material?",
                text: i18n ? i18n.dataset.confirmarTexto : "¿Seguro que quieres quitar este material del préstamo?",
                icon: "warning",
                showCancelButton: true,
                confirmButtonText: i18n ? i18n.dataset.confirmarBoton : "Eliminar",
                cancelButtonText: i18n ? i18n.dataset.cancelar : "Cancelar",
                confirmButtonColor: "#dc3545",
                cancelButtonColor: "#6c757d",
                reverseButtons: true,
                focusCancel: true
            });

            if (!result.isConfirmed) {
                return;
            }
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
                Swal.fire({
                    icon: "error",
                    title: "Error",
                    text: "No se pudo completar la acción. Inténtalo de nuevo."
                });
                if (boton) {
                    boton.disabled = false;
                }
            });
    });
})();

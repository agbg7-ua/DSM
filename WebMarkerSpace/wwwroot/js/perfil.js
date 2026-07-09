(function () {
    "use strict";

    var boton = document.getElementById("btn-eliminar-perfil");
    var i18n = document.getElementById("perfil-delete-i18n");

    if (!boton || !i18n) {
        return;
    }

    function getAntiForgeryToken() {
        var input = document.querySelector("#af-token-form input[name='__RequestVerificationToken']");
        return input ? input.value : null;
    }

    boton.addEventListener("click", async function () {

        var esCuentaExterna = i18n.dataset.esExterna === "true";

        var opciones = {
            title: i18n.dataset.confirmarTitulo,
            text: i18n.dataset.confirmarTexto,
            icon: "warning",
            showCancelButton: true,
            confirmButtonText: i18n.dataset.confirmarBoton,
            cancelButtonText: i18n.dataset.cancelar,
            confirmButtonColor: "#dc3545",
            cancelButtonColor: "#6c757d",
            reverseButtons: true,
            focusCancel: true
        };

        if (!esCuentaExterna) {
            opciones.input = "password";
            opciones.inputLabel = i18n.dataset.passwordLabel;
            opciones.inputPlaceholder = i18n.dataset.passwordPlaceholder;
            opciones.inputAttributes = {
                autocomplete: "current-password",
                "aria-label": i18n.dataset.passwordLabel
            };
            opciones.inputValidator = function (value) {
                if (!value) {
                    return i18n.dataset.passwordRequired;
                }
            };
        }

        const result = await Swal.fire(opciones);

        if (!result.isConfirmed) {
            return;
        }

        var token = getAntiForgeryToken();
        var body = new URLSearchParams();
        if (token) {
            body.append("__RequestVerificationToken", token);
        }
        if (!esCuentaExterna) {
            body.append("contrasenia", result.value);
        }

        boton.disabled = true;

        try {
            const response = await fetch(i18n.dataset.deleteUrl, {
                method: "POST",
                headers: {
                    "X-Requested-With": "XMLHttpRequest",
                    "Content-Type": "application/x-www-form-urlencoded"
                },
                body: body.toString()
            });

            const data = await response.json();

            if (data.success) {

                await Swal.fire({
                    icon: "success",
                    title: i18n.dataset.eliminadoTitulo,
                    text: i18n.dataset.eliminadoTexto,
                    timer: 1800,
                    showConfirmButton: false
                });

                window.location.href = i18n.dataset.redirectUrl;
            } else {

                Swal.fire({
                    icon: "error",
                    title: i18n.dataset.error,
                    text: data.message || i18n.dataset.errorBorrar
                });

                boton.disabled = false;
            }

        } catch (err) {

            Swal.fire({
                icon: "error",
                title: i18n.dataset.error,
                text: i18n.dataset.errorRed
            });

            boton.disabled = false;
        }
    });
})();

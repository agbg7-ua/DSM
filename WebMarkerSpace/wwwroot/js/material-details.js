(function () {
    "use strict";

    var link = document.querySelector(".js-borrar-material-details");
    var i18n = document.getElementById("material-details-i18n");

    if (!link || !i18n) {
        return;
    }

    function getAntiForgeryToken() {
        var input = document.querySelector("#af-token-form input[name='__RequestVerificationToken']");
        return input ? input.value : null;
    }

    link.addEventListener("click", async function (e) {
        e.preventDefault();

        var nombre = link.dataset.nombre || "";

        const result = await Swal.fire({
            title: i18n.dataset.confirmarTitulo,
            text: i18n.dataset.confirmarTexto.replace("{0}", nombre),
            icon: "warning",
            showCancelButton: true,
            confirmButtonText: i18n.dataset.confirmarBoton,
            cancelButtonText: i18n.dataset.cancelar,
            confirmButtonColor: "#dc3545",
            cancelButtonColor: "#6c757d",
            reverseButtons: true,
            focusCancel: true
        });

        if (!result.isConfirmed) {
            return;
        }

        var token = getAntiForgeryToken();
        var body = new URLSearchParams();
        body.append("id", link.dataset.id);
        if (token) {
            body.append("__RequestVerificationToken", token);
        }

        link.setAttribute("aria-disabled", "true");

        try {
            const response = await fetch(link.href, {
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
                    timer: 1500,
                    showConfirmButton: false
                });

                window.location.href = i18n.dataset.indexUrl;
            } else {
                Swal.fire({
                    icon: "error",
                    title: i18n.dataset.error,
                    text: data.message || i18n.dataset.errorBorrar
                });
                link.removeAttribute("aria-disabled");
            }
        } catch (err) {
            Swal.fire({
                icon: "error",
                title: i18n.dataset.error,
                text: i18n.dataset.errorRed
            });
            link.removeAttribute("aria-disabled");
        }
    });
})();

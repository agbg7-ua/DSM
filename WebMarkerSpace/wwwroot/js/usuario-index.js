(function () {
    "use strict";

    var form = document.getElementById("form-filtro-usuarios");
    var resultados = document.getElementById("usuarios-resultados");
    var spinner = document.getElementById("spinner-usuarios");
    var limpiarLink = document.getElementById("link-limpiar-filtro");
    var i18n = document.getElementById("usuarios-i18n");

    if (!form || !resultados) {
        return;
    }

    function getAntiForgeryToken() {
        var input = document.querySelector("#af-token-form input[name='__RequestVerificationToken']");
        return input ? input.value : null;
    }

    function mostrarSpinner(visible) {
        if (!spinner) return;
        spinner.classList.toggle("d-none", !visible);
    }

    function cargarResultados(url, actualizarHistorial) {
        mostrarSpinner(true);

        fetch(url, {
            method: "GET",
            headers: { "X-Requested-With": "XMLHttpRequest" }
        })
            .then(function (resp) {
                if (!resp.ok) { throw new Error("HTTP " + resp.status); }
                return resp.text();
            })
            .then(function (html) {
                resultados.innerHTML = html;
                if (actualizarHistorial) {
                    window.history.pushState({ url: url }, "", url);
                }
            })
            .finally(function () {
                mostrarSpinner(false);
            });
    }

    function enviarFiltro() {
        var params = new URLSearchParams(new FormData(form));
        var url = form.action + "?" + params.toString();
        cargarResultados(url, true);
    }

    form.addEventListener("input", enviarFiltro);

    form.querySelectorAll("select").forEach(function (select) {
        select.addEventListener("change", enviarFiltro);
    });

    if (limpiarLink) {
        limpiarLink.addEventListener("click", function (e) {
            e.preventDefault();
            form.reset();
            cargarResultados(limpiarLink.href, true);
        });
    }

    window.addEventListener("popstate", function () {
        cargarResultados(window.location.href, false);
    });

    resultados.addEventListener("click", async function (e) {

        var link = e.target.closest(".js-borrar-usuario");
        if (!link) return;

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

                var fila = link.closest("tr");

                if (fila) {
                    fila.remove();
                }

                if (!resultados.querySelector("tbody tr")) {
                    cargarResultados(
                        form.action + "?" + new URLSearchParams(new FormData(form)).toString(),
                        false
                    );
                }
            }
            else {

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

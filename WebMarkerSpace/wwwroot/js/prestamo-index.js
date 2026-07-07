// Interacciones AJAX para el listado de Préstamos:
// - El formulario de filtro (estado/usuario) se envía por fetch y solo
//   reemplaza la tabla.
// - El enlace "Borrar" (solo Administrador) hace la baja por fetch y quita
//   la fila, sin recargar.
// Si JS falla o está desactivado, todo sigue funcionando como navegación
// normal (progressive enhancement).
(function () {
    "use strict";

    var form = document.getElementById("form-filtro-prestamos");
    var resultados = document.getElementById("prestamos-resultados");
    var spinner = document.getElementById("spinner-prestamos");
    var limpiarLink = document.getElementById("link-limpiar-filtro-prestamos");

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
            .catch(function () {
                resultados.innerHTML = '<p class="text-danger">No se pudo cargar el listado de préstamos. Inténtalo de nuevo.</p>';
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

    form.addEventListener("submit", function (e) {
        e.preventDefault();
        enviarFiltro();
    });

    // Filtrar al cambiar estado/usuario, sin esperar al botón
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

    // Borrado inline: delegado en un contenedor estable porque la tabla
    // se reemplaza cada vez que se filtra.
    resultados.addEventListener("click", function (e) {
        var link = e.target.closest(".js-borrar-prestamo");
        if (!link) return;

        e.preventDefault();

        var descripcion = link.dataset.descripcion || "este préstamo";
        if (!window.confirm('¿Seguro que quieres borrar ' + descripcion + '?')) {
            return;
        }

        var token = getAntiForgeryToken();
        var body = new URLSearchParams();
        body.append("id", link.dataset.id);
        if (token) {
            body.append("__RequestVerificationToken", token);
        }

        link.setAttribute("aria-disabled", "true");

        fetch(link.href, {
            method: "POST",
            headers: {
                "X-Requested-With": "XMLHttpRequest",
                "Content-Type": "application/x-www-form-urlencoded"
            },
            body: body.toString()
        })
            .then(function (resp) { return resp.json(); })
            .then(function (data) {
                if (data.success) {
                    var fila = link.closest("tr");
                    if (fila) {
                        fila.parentNode.removeChild(fila);
                    }
                    var tabla = document.getElementById("tabla-prestamos");
                    if (tabla && !tabla.querySelector("tbody tr")) {
                        cargarResultados(form.action + "?" + new URLSearchParams(new FormData(form)).toString(), false);
                    }
                } else {
                    window.alert(data.message || "No se pudo borrar el préstamo.");
                    link.removeAttribute("aria-disabled");
                }
            })
            .catch(function () {
                window.alert("Error de red al borrar el préstamo.");
                link.removeAttribute("aria-disabled");
            });
    });
})();

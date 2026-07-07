// Interacciones AJAX para el listado de Materiales:
// - El formulario de filtro se envía por fetch y solo reemplaza la tabla.
// - El enlace "Borrar" hace la baja por fetch y quita la fila, sin recargar.
// Si JS falla o está desactivado, el formulario y los enlaces siguen
// funcionando como una navegación normal (progressive enhancement).
(function () {
    "use strict";

    var form = document.getElementById("form-filtro-materiales");
    var resultados = document.getElementById("materiales-resultados");
    var spinner = document.getElementById("spinner-materiales");
    var limpiarLink = document.getElementById("link-limpiar-filtro");


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
                var i18n = document.getElementById("materiales-i18n");
                var mensaje = i18n ? i18n.getAttribute("data-error-cargando") : "Couldn't load the materials list. Please try again.";
                resultados.innerHTML = '<p class="text-danger">' + mensaje + '</p>';
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

    // Envío normal del formulario (botón "Buscar" o Enter)
    form.addEventListener("submit", function (e) {
        e.preventDefault();
        enviarFiltro();
    });

    // Búsqueda "en vivo" al escribir el nombre, con pequeño retardo (debounce)
    var nombreInput = form.querySelector("input[name='nombre']");
    var debounceTimer = null;
    if (nombreInput) {
        nombreInput.addEventListener("input", function () {
            clearTimeout(debounceTimer);
            debounceTimer = setTimeout(enviarFiltro, 400);
        });
    }

    // Filtrar al cambiar categoría/estado, sin esperar al botón
    form.querySelectorAll("select").forEach(function (select) {
        select.addEventListener("change", enviarFiltro);
    });

    // "Limpiar" también se resuelve por AJAX, sin recargar la página
    if (limpiarLink) {
        limpiarLink.addEventListener("click", function (e) {
            e.preventDefault();
            form.reset();
            cargarResultados(limpiarLink.href, true);
        });
    }

    // Atrás/adelante del navegador vuelve a pedir el listado correspondiente
    window.addEventListener("popstate", function () {
        cargarResultados(window.location.href, false);
    });

    // Borrado inline: delegado en un contenedor estable porque la tabla
    // se reemplaza cada vez que se filtra.
    resultados.addEventListener("click", function (e) {
        var link = e.target.closest(".js-borrar-material");
        if (!link) return;

        e.preventDefault();

        var nombre = link.dataset.nombre || "este material";
        if (!window.confirm('¿Seguro que quieres borrar "' + nombre + '"?')) {
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
                    var card = link.closest(".col");
                    if (card) {
                        card.parentNode.removeChild(card);
                    }
                    var grid = document.getElementById("grid-materiales");
                    if (grid && !grid.querySelector(".col")) {
                        cargarResultados(form.action + "?" + new URLSearchParams(new FormData(form)).toString(), false);
                    }
                } else {
                    window.alert(data.message || "No se pudo borrar el material.");
                    link.removeAttribute("aria-disabled");
                }
            })
            .catch(function () {
                window.alert("Error de red al borrar el material.");
                link.removeAttribute("aria-disabled");
            });
    });
})();

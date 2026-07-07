const form = document.getElementById("form-filtro-usuarios");

form.addEventListener("input", actualizar);

async function actualizar() {

    const datos = new URLSearchParams(new FormData(form));

    const respuesta = await fetch(
        form.action + "?" + datos,
        {
            headers: {
                "X-Requested-With": "XMLHttpRequest"
            }
        });

    document.getElementById("usuarios-resultados").innerHTML =
        await respuesta.text();
}
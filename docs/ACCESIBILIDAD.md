# Accesibilidad (objetivo: WCAG 2.2, nivel AAA donde es razonable)

AAA es, por definición del propio W3C, un nivel que **no siempre es
alcanzable para todo el contenido** (por ejemplo, el contraste 7:1 en
imágenes de marca, o el lenguaje de nivel de lectura simplificado). El
objetivo aquí es: cumplir AA en toda la aplicación y subir a AAA en todo lo
que no implica un rediseño completo del contenido o funcionalidades que no
existen en este proyecto (vídeo, audio, etc.).

## Qué se ha aplicado y dónde

Aplicado en profundidad en el flujo de autenticación (`Login`, `Register`,
`Perfil`) y en los layouts compartidos (`_Layout`, `_LayoutLogin`), que es lo
que pediste. El resto de vistas CRUD (`Material`, `Prestamo`,
`LineaPrestamo`, `Usuario/Index|Edit|Delete|Details`) siguen el mismo patrón
de Bootstrap + `asp-for`/`asp-validation-for` y **se benefician ya** de los
cambios globales (site.css, site.js, layout), pero no se han reescrito
campo a campo; el patrón de la sección "Cómo replicarlo" de más abajo sirve
para extenderlo si quieres que lo haga en el resto de formularios.

| Criterio WCAG 2.2 | Nivel | Qué se hizo |
|---|---|---|
| 2.4.1 Bypass Blocks | A | Enlace "Saltar al contenido principal" (`.skip-link`) en ambos layouts. |
| 2.4.13 Focus Appearance | AAA | Contorno de foco de 3px, alto contraste, `outline-offset`, en vez del `box-shadow` sutil original. |
| 1.4.1 Use of Color | A | Los enlaces dentro de texto corrido llevan subrayado, no solo color. |
| 1.4.6 Contrast (Enhanced) | AAA | Se oscureció `.text-muted`/`.form-text` de `#6c757d` (~4.6:1) a `#495057` (~8.3:1 sobre blanco). |
| 1.4.8 Visual Presentation | AAA | `line-height: 1.6` en el contenido, ancho de párrafo limitado a `70ch`. |
| 2.5.5 Target Size | AAA | Botones, enlaces de navegación y checkboxes con alto mínimo de 44px. |
| 2.3.3 Animation from Interactions | AAA | `prefers-reduced-motion: reduce` desactiva transiciones/animaciones. |
| 3.3.1 / 3.3.2 Error Identification & Labels | A | Todas las etiquetas usan `asp-for` (asociadas por `for`/`id`); errores enlazados con `aria-describedby`. |
| 4.1.3 Status Messages | AA | Resumen de errores con `role="alert"`, foco automático al enviarse (`site.js`); mensajes de éxito con `role="status" aria-live="polite"`. |
| 1.3.1 Info and Relationships | A | `<fieldset>`/`<legend>` agrupando campos relacionados (credenciales, cambio de contraseña); landmarks `<header>`/`<main>`/`<footer>` con `aria-label`. |
| 3.1.1 Language of Page | A | `lang="es"` corregido en `_LayoutLogin` (antes decía `en`). |

## Cómo replicarlo en el resto de formularios (Material, Préstamo, etc.)

Para cada formulario `Create`/`Edit`:

1. Envolver los campos relacionados en `<fieldset><legend class="visually-hidden">...</legend>`.
2. Añadir `id="Campo-error"` al `<span asp-validation-for="Campo">` y
   `aria-describedby="Campo-error"` al `<input asp-for="Campo">` correspondiente.
3. Cambiar `class="control-label"` por `class="form-label"` (ya es lo que
   usa Bootstrap 5, que es el que trae este proyecto).
4. Añadir `autocomplete` donde aplique (`autocomplete="off"` no hace falta
   salvo casos especiales).
5. Cambiar el `<div asp-validation-summary="ModelOnly" class="text-danger">`
   por `class="alert alert-danger" role="alert" tabindex="-1"` — `site.js`
   ya le pone el foco automáticamente si tiene contenido.
6. Revisar que los `<select>`/`<input type="file">` tengan su `<label
   asp-for>` correspondiente (ya lo tienen en `Material/Create.cshtml`, por
   ejemplo).

No hace falta tocar `site.js` ni `site.css` de nuevo: el foco en errores, el
contraste y el tamaño de los objetivos ya se aplican de forma global.

## Verificación recomendada

Ninguna herramienta automática certifica AAA por sí sola. Recomendamos:

- Axe DevTools / Lighthouse (detectan la mayoría de fallos de nivel A/AA).
- Navegación completa solo con teclado (Tab / Shift+Tab / Enter / Espacio),
  comprobando que el foco es siempre visible y sigue un orden lógico.
- Probar con un lector de pantalla (NVDA en Windows, o VoiceOver en macOS)
  al menos el flujo de login, registro y el envío de un formulario con
  errores.
- Comprobar contraste real con la paleta final si se cambia el CSS de
  Bootstrap (por ejemplo, si se personaliza `$primary`).

// "Copyright (c) YOUR_COMPANY. All rights reserved."

using Microsoft.Extensions.Localization;

namespace WebMarkerSpace.Extensions {

    /// <summary>
    /// Los enums de dominio (EstadoMaterial, CategoriaMaterial, RolUsuario,
    /// EstadoPrestamo...) se muestran en varias vistas con <c>@Model.Estado</c>
    /// o <c>@Html.DisplayFor</c>, que por defecto imprimen <c>ToString()</c>
    /// del valor (siempre en español, p. ej. "EnMantenimiento"). Este helper
    /// busca una traducción en SharedResource usando la clave
    /// <c>Enum.{TipoEnum}.{Valor}</c> y, si no existe, devuelve el nombre del
    /// valor tal cual (nunca lanza ni deja la celda vacía).
    /// </summary>
    public static class EnumLocalizationExtensions {

        public static string Localize(this IStringLocalizer localizer, Enum? valor) {
            if (valor is null) {
                return string.Empty;
            }

            string clave = $"Enum.{valor.GetType().Name}.{valor}";
            var resultado = localizer[clave];
            return resultado.ResourceNotFound ? valor.ToString()! : resultado.Value;
        }
    }
}

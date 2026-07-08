
using Microsoft.Extensions.Localization;

namespace WebMarkerSpace.Extensions {

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

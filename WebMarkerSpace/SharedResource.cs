// "Copyright (c) YOUR_COMPANY. All rights reserved."

namespace WebMarkerSpace {
    /// <summary>
    /// Clase marcadora (sin código), usada solo como "ancla" de tipo para
    /// <see cref="Microsoft.Extensions.Localization.IStringLocalizer{T}"/>.
    /// Los textos reales viven en /Resources, junto a Program.cs:
    ///
    ///   - Resources/SharedResource.resx     -> cultura neutra / español (es-ES)
    ///   - Resources/SharedResource.en.resx  -> inglés (en-US)
    ///
    /// IMPORTANTE: esta clase vive a propósito en el namespace raíz
    /// "WebMarkerSpace" (NO en "WebMarkerSpace.Resources"), aunque sus
    /// ficheros .resx sí estén dentro de la carpeta Resources/. Es un
    /// requisito de cómo ASP.NET Core calcula el nombre del recurso a
    /// partir de <c>ResourcesPath</c> (configurado en Program.cs) y del
    /// namespace del tipo: si la clase estuviera también en el namespace
    /// "...Resources", el nombre buscado duplicaría ese segmento
    /// ("WebMarkerSpace.Resources.Resources.SharedResource") y el
    /// localizador nunca encontraría los textos — se limitaría a imprimir
    /// la clave tal cual (p. ej. "Layout.Home" en vez de "Inicio").
    /// </summary>
    public class SharedResource { }
}

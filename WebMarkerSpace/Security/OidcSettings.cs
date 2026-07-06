// "Copyright (c) YOUR_COMPANY. All rights reserved."

namespace WebMarkerSpace.Security;

/// <summary>
/// Información no sensible sobre la configuración de login OAuth2/OIDC, para que
/// controladores y vistas puedan decidir si mostrar el botón de "iniciar sesión
/// con proveedor externo" sin tener acceso a Authority/ClientSecret.
/// </summary>
public record OidcSettings(bool Habilitado, string NombreProveedor);

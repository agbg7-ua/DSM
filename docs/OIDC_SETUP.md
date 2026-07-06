# Login con OAuth2 / OpenID Connect

WebMarkerSpace sigue soportando el login local (email + contraseña), y además
puede delegar la autenticación en **cualquier proveedor compatible con OpenID
Connect** (Google, Microsoft Entra ID, Auth0, Keycloak, Okta, etc.). Las dos
formas de entrar terminan en la misma cookie de sesión y en los mismos roles
de aplicación (`Administrador` / `Usuario`).

## Cómo funciona

1. El usuario pulsa "Entrar con `<proveedor>`" en `/Usuario/Login`.
2. `UsuarioController.ExternalLogin` hace `Challenge(...)` contra el esquema
   `OpenIdConnect`, que redirige al `Authority` configurado (Authorization
   Code + PKCE).
3. Tras autenticarse en el proveedor, éste redirige de vuelta a
   `CallbackPath` (por defecto `/signin-oidc`) con un código de autorización.
4. El middleware intercambia el código por tokens y valida el `id_token`.
5. En el evento `OnTokenValidated` (`WebMarkerSpace/Security/OidcAccountProvisioning.cs`)
   se busca o crea el usuario local:
   - Si ya existe un `Usuario` vinculado a ese proveedor (`IdExterno` = claim `sub`), se reconoce.
   - Si no, pero existe una cuenta local con el mismo email, se vincula (no se duplica).
   - Si no existe ninguna, se crea de alta automáticamente con rol `Usuario`
     (nunca `Administrador`: ese rol solo lo asigna un administrador desde `/Usuario`).
6. Se sustituye el principal externo por uno propio (Id/Nombre/Email/Rol de
   nuestra base de datos) y se firma la cookie de sesión habitual.

El proveedor externo **nunca** decide roles ni permisos dentro de
WebMarkerSpace; solo confirma "quién es" la persona.

## Configuración

En `appsettings.json` (o mejor, en variables de entorno / `dotnet user-secrets`
para el `ClientSecret`):

```json
"Authentication": {
  "Oidc": {
    "Enabled": true,
    "DisplayName": "Google",
    "Authority": "https://accounts.google.com",
    "ClientId": "TU_CLIENT_ID",
    "ClientSecret": "TU_CLIENT_SECRET",
    "CallbackPath": "/signin-oidc"
  }
}
```

Con `Enabled=false` (o sin `Authority`/`ClientId`), el login externo queda
completamente desactivado: no se registra el middleware de OIDC y el botón
no aparece en la vista de login. Esto permite seguir desarrollando y
ejecutando `InitializeDb`/tests sin depender de un proveedor externo.

Para desarrollo local recomendamos guardar el secreto así, en vez de en el
`.json` versionado:

```bash
dotnet user-secrets set "Authentication:Oidc:ClientSecret" "..." --project WebMarkerSpace
```

### Ejemplo con Keycloak (self-hosted, útil para pruebas)

```json
"Authority": "https://mi-keycloak/realms/makerspace",
"ClientId": "webmarkerspace",
"ClientSecret": "..."
```

### Ejemplo con Auth0

```json
"Authority": "https://TU_DOMINIO.auth0.com",
"ClientId": "...",
"ClientSecret": "..."
```

## Registrar el "Redirect URI" en el proveedor

Debes dar de alta, en la configuración de tu aplicación cliente del
proveedor, la URL completa de callback:

```
https://tu-dominio/signin-oidc
```

y, si usas cierre de sesión federado, también:

```
https://tu-dominio/signout-callback-oidc
```

## Cambios en el modelo de datos

Se añadieron dos columnas nuevas y opcionales a `Usuarios`:
`ProveedorExterno` e `IdExterno`. Se generan automáticamente al ejecutar
`InitializeDb` (NHibernate exporta el esquema a partir de
`Usuario.hbm.xml`); no hace falta ningún script manual.

## Limitaciones conocidas / próximos pasos razonables

- El cierre de sesión (`Logout`) solo cierra la cookie local; no revoca la
  sesión en el proveedor externo (single logout). Añadirlo implicaría llamar
  también a `SignOutAsync(OpenIdConnectDefaults.AuthenticationScheme)`.
- Si el proveedor no expone `email` (algunos permiten deshabilitar ese
  scope), el aprovisionamiento falla explícitamente en vez de crear una
  cuenta sin email: revisa que el scope `email` esté habilitado en el
  cliente OIDC.

using ApplicationCore.Domain.CEN;
using ApplicationCore.Domain.CP;
using ApplicationCore.Domain.Repositories;
using Infrastructure.NHibernate;
using Infrastructure.NHibernate.Repositories;
using Infrastructure.NHibernate.UnitOfWork;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using NHibernate;
using System.Globalization;
using WebMarkerSpace;
using WebMarkerSpace.Security;

var builder = WebApplication.CreateBuilder(args);

const string connectionString = "Server=.\\SQLEXPRESS;Database=GestionMakerspace;Trusted_Connection=True;TrustServerCertificate=True;";

// NHibernate
var sessionFactory = NHibernateHelper.BuildSessionFactory(connectionString);
builder.Services.AddSingleton<ISessionFactory>(sessionFactory);
builder.Services.AddScoped<NHibernate.ISession>(provider =>
    provider.GetRequiredService<ISessionFactory>().OpenSession());

// Repositorios
builder.Services.AddScoped<IMaterialRepository, MaterialRepository>();
builder.Services.AddScoped<IUsuarioRepository, UsuarioRepository>();
builder.Services.AddScoped<IPrestamoRepository, PrestamoRepository>();
builder.Services.AddScoped<ILineaPrestamoRepository, LineaPrestamoRepository>();

// CENs
builder.Services.AddScoped<MaterialCEN>();
builder.Services.AddScoped<UsuarioCEN>();
builder.Services.AddScoped<PrestamoCEN>();
builder.Services.AddScoped<LineaPrestamoCEN>();

// Unidad de trabajo y caso de proceso (usado por PrestamoController.Devolver
// para marcar un préstamo como devuelto y liberar sus materiales a la vez).
builder.Services.AddScoped<IUnitOfWork, NHibernateUnitOfWork>();
builder.Services.AddScoped<CasosProceso>();

// Autenticación por cookies + autorización basada en roles (Administrador / Usuario).
// El esquema de cookies sigue siendo el único que decide si hay sesión iniciada;
// tanto el login local (email/contraseña) como el login federado (OAuth2/OIDC)
// terminan emitiendo la misma cookie con las mismas claims (ver
// WebMarkerSpace.Security.OidcAccountProvisioning).
var authBuilder = builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Usuario/Login";
        options.AccessDeniedPath = "/Home/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = SameSiteMode.Lax;
    });

// Login con OAuth2 / OpenID Connect: opcional y desactivado por defecto. Se activa
// configurando Authentication:Oidc:Enabled=true junto con Authority/ClientId/ClientSecret
// (ver appsettings.json y docs/OIDC_SETUP.md). Sirve para cualquier proveedor compatible
// con OIDC: Google, Microsoft Entra ID, Auth0, Keycloak, etc.
var oidcConfig = builder.Configuration.GetSection("Authentication:Oidc");
bool oidcHabilitado = oidcConfig.GetValue<bool>("Enabled")
    && !string.IsNullOrWhiteSpace(oidcConfig["Authority"])
    && !string.IsNullOrWhiteSpace(oidcConfig["ClientId"]);

builder.Services.AddSingleton(new OidcSettings(oidcHabilitado, oidcConfig["DisplayName"] ?? "proveedor externo"));

if (oidcHabilitado)
{
    authBuilder.AddOpenIdConnect(OpenIdConnectDefaults.AuthenticationScheme, options =>
    {
        options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;

        options.Authority = oidcConfig["Authority"];
        options.ClientId = oidcConfig["ClientId"];
        options.ClientSecret = oidcConfig["ClientSecret"];
        options.CallbackPath = oidcConfig["CallbackPath"] ?? "/signin-oidc";
        options.SignedOutCallbackPath = oidcConfig["SignedOutCallbackPath"] ?? "/signout-callback-oidc";

        // Authorization Code + PKCE: el flujo recomendado para apps web con backend
        // confidencial (evita exponer tokens en el navegador).
        options.ResponseType = OpenIdConnectResponseType.Code;
        options.UsePkce = true;
        options.SaveTokens = false; // no necesitamos guardar tokens de acceso/id.

        options.Scope.Clear();
        options.Scope.Add("openid");
        options.Scope.Add("profile");
        options.Scope.Add("email");

        options.GetClaimsFromUserInfoEndpoint = true;
        options.MapInboundClaims = false;

        // Nunca nos fiamos de un rol que pudiera venir del proveedor externo: los
        // roles de WebMarkerSpace los asigna siempre nuestra base de datos.
        options.Events.OnTokenValidated = OidcAccountProvisioning.ProvisionarYSustituirPrincipalAsync;

        options.Events.OnRemoteFailure = context =>
        {
            // El mensaje que ve el usuario es siempre genérico (no queremos filtrar
            // detalles internos), pero registramos la excepción real tanto en el log
            // del servidor como en un fichero de texto en disco, para poder
            // diagnosticar el fallo con calma (la redirección al login es inmediata
            // y no da tiempo a leer la consola).
            var logger = context.HttpContext.RequestServices
                .GetRequiredService<Microsoft.Extensions.Logging.ILoggerFactory>()
                .CreateLogger("Oidc");
            logger.LogError(context.Failure, "Fallo en el flujo OIDC (remote failure).");

            try
            {
                string logPath = Path.Combine(AppContext.BaseDirectory, "oidc-errors.log");
                string entry = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} ---{Environment.NewLine}{context.Failure}{Environment.NewLine}{Environment.NewLine}";
                File.AppendAllText(logPath, entry);
            }
            catch
            {
                // Si no se puede escribir el fichero (permisos, disco, etc.) seguimos
                // adelante: ya queda registrado en el logger de arriba.
            }

            context.Response.Redirect("/Usuario/Login?error=oidc");
            context.HandleResponse();
            return Task.CompletedTask;
        };
    });
}

builder.Services.AddAuthorization();

// ---------------------------------------------------------------------
// Internacionalización (i18n) — PASO 2: toggle manual ES/EN.
// El idioma se resuelve, por petición, en este orden:
//   1) Cookie de cultura (".AspNetCore.Culture"), la que fija el toggle
//      ES/EN de Views/Shared/_LanguageToggle.cshtml a través de
//      CultureController.SetLanguage. Si la persona ya eligió idioma a
//      mano, esta cookie manda siempre por encima de lo que diga el
//      navegador.
//   2) Cabecera Accept-Language del navegador (detección automática, la
//      primera vez que se visita el sitio, antes de elegir nada a mano).
//   3) Si nada de lo anterior aplica: español (DefaultRequestCulture).
// ---------------------------------------------------------------------
// Los textos localizados viven en /Resources:
//   - SharedResource.resx     -> español (cultura neutra, es-ES)
//   - SharedResource.en.resx  -> inglés (en-US)
builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");

var culturasSoportadas = new[]
{
    new CultureInfo("es-ES"),
    new CultureInfo("en-US"),
};

builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    options.DefaultRequestCulture = new RequestCulture("es-ES");
    options.SupportedCultures = culturasSoportadas;
    options.SupportedUICultures = culturasSoportadas;

    options.RequestCultureProviders = new List<IRequestCultureProvider>
    {
        new CookieRequestCultureProvider(),
        new AcceptLanguageHeaderRequestCultureProvider()
    };
});

// MVC
// IMPORTANTE (i18n): AddViewLocalization + AddDataAnnotationsLocalization son
// las piezas que faltaban para que las DataAnnotations (Display, Required,
// EmailAddress, StringLength, Compare...) de los ViewModels se traduzcan igual
// que los textos de las vistas con @Localizer[...]. Sin esto, el
// "Name"/"ErrorMessage" que se escriba en los atributos se imprime tal cual,
// siempre en el idioma en que esté escrito en el código (español), sin
// importar el toggle ES/EN. Con esto, MVC usa el propio texto del atributo
// como CLAVE de SharedResource (p. ej. [Required(ErrorMessage = "Login.Email.Required")]
// hace internamente Localizer["Login.Email.Required"]).
builder.Services.AddControllersWithViews()
    .AddViewLocalization()
    .AddDataAnnotationsLocalization(options =>
    {
        options.DataAnnotationLocalizerProvider = (type, factory) =>
            factory.Create(typeof(SharedResource));
    });

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

// UseRequestLocalization debe ir pronto en el pipeline (antes de enrutado
// y de que se ejecuten controladores/vistas) para que la cultura ya esté
// resuelta cuando se renderiza la página.
app.UseRequestLocalization(app.Services.GetRequiredService<IOptions<RequestLocalizationOptions>>().Value);

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

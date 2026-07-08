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

var sessionFactory = NHibernateHelper.BuildSessionFactory(connectionString);
builder.Services.AddSingleton<ISessionFactory>(sessionFactory);
builder.Services.AddScoped<NHibernate.ISession>(provider =>
    provider.GetRequiredService<ISessionFactory>().OpenSession());

builder.Services.AddScoped<IMaterialRepository, MaterialRepository>();
builder.Services.AddScoped<IUsuarioRepository, UsuarioRepository>();
builder.Services.AddScoped<IPrestamoRepository, PrestamoRepository>();
builder.Services.AddScoped<ILineaPrestamoRepository, LineaPrestamoRepository>();

builder.Services.AddScoped<MaterialCEN>();
builder.Services.AddScoped<UsuarioCEN>();
builder.Services.AddScoped<PrestamoCEN>();
builder.Services.AddScoped<LineaPrestamoCEN>();

builder.Services.AddScoped<IUnitOfWork, NHibernateUnitOfWork>();
builder.Services.AddScoped<CasosProceso>();

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

        options.ResponseType = OpenIdConnectResponseType.Code;
        options.UsePkce = true;
        options.SaveTokens = false;

        options.Scope.Clear();
        options.Scope.Add("openid");
        options.Scope.Add("profile");
        options.Scope.Add("email");

        options.GetClaimsFromUserInfoEndpoint = true;
        options.MapInboundClaims = false;

        options.Events.OnTokenValidated = OidcAccountProvisioning.ProvisionarYSustituirPrincipalAsync;

        options.Events.OnRemoteFailure = context =>
        {

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

            }

            context.Response.Redirect("/Usuario/Login?error=oidc");
            context.HandleResponse();
            return Task.CompletedTask;
        };
    });
}

builder.Services.AddAuthorization();

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

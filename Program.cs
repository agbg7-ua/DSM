using ApplicationCore.Domain.CEN;
using ApplicationCore.Domain.Repositories;
using Infrastructure.NHibernate;
using Infrastructure.NHibernate.Repositories;
using Microsoft.AspNetCore.Authentication.Cookies;
using NHibernate;

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

// Autenticación por cookies + autorización basada en roles (Administrador / Usuario)
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Usuario/Login";
        options.AccessDeniedPath = "/Home/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;
        options.Cookie.HttpOnly = true;
    });
builder.Services.AddAuthorization();

// MVC
builder.Services.AddControllersWithViews();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

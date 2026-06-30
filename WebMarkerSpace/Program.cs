using ApplicationCore.Domain.CEN;
using ApplicationCore.Domain.Repositories;
using Infrastructure.NHibernate;
using Infrastructure.NHibernate.Repositories;
using Microsoft.AspNetCore.Mvc;
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

// Sesión
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// MVC
builder.Services.AddControllersWithViews();
builder.Services.AddSession(options => {

    options.IdleTimeout = TimeSpan.FromSeconds(1000);

    options.Cookie.HttpOnly = true;

    options.Cookie.IsEssential = true;

});

var app = builder.Build();

if (!app.Environment.IsDevelopment()) {
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseSession();
app.UseAuthorization();
app.UseStaticFiles();

builder.Services.AddDistributedMemoryCache();


app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Usuario}/{action=Login}/{id?}");

app.Run();

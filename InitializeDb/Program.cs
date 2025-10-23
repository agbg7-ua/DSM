using System;
using System.IO;
using Microsoft.Extensions.DependencyInjection;
using Infrastructure;
using Infrastructure.NHibernate;
using ApplicationCore.Domain.CEN;
using ApplicationCore.Domain.Repositories;
using ApplicationCore.Domain.EN;
using ApplicationCore.Domain.Enums;

namespace InitializeDb
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("InitializeDb: creando esquema de base de datos usando NHibernate...");

            // Ensure Data directory exists
            var dataDir = Path.Combine(AppContext.BaseDirectory, "Data");
            Directory.CreateDirectory(dataDir);
            AppDomain.CurrentDomain.SetData("DataDirectory", dataDir);

            try
            {
                // Setup DI container
                var services = new ServiceCollection();
                services.AddInfrastructureServices();
                
                // Add CENs and CPs
                services.AddScoped<UsuarioCEN>();
                services.AddScoped<ProductoCEN>();
                
                var serviceProvider = services.BuildServiceProvider();

                // Create schema
                Console.WriteLine("Creando esquema...");
                NHibernateHelper.CreateSchema();
                
                // Initialize basic data using CENs (ejemplo)
                using (var scope = serviceProvider.CreateScope())
                {
                    var usuarioCEN = scope.ServiceProvider.GetRequiredService<UsuarioCEN>();
                    var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

                    try
                    {
                        Console.WriteLine("Iniciando seed de datos básicos...");
                        
                        // Crear admin inicial
                        usuarioCEN.Crear(
                            nombre: "Administrador",
                            correo: "admin@dsm.com",
                            contrasena: "admin123",
                            direccion: "Calle Admin 123"
                        );

                        // Crear producto de ejemplo
                        var productoCEN = scope.ServiceProvider.GetRequiredService<ProductoCEN>();
                        productoCEN.Crear(
                            nombre: "Producto Demo",
                            precio: 99.99m,
                            stock: 10,
                            descripcion: "Producto de ejemplo para testing",
                            categoria: "Demo"
                        );

                        Console.WriteLine("Seed completado exitosamente.");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error durante el seed: {ex.Message}");
                        uow.Rollback();
                        throw;
                    }
                }

                Console.WriteLine("InitializeDb completado exitosamente.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error inicializando la base de datos: {ex.Message}");
                Console.WriteLine(ex.StackTrace);
                throw;
            }
        }
    }
}

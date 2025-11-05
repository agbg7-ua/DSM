using System;
using System.IO;
using Microsoft.Extensions.DependencyInjection;
using Infrastructure;
using Infrastructure.NHibernate;
using ApplicationCore.Domain.CEN;

namespace InitializeDb
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                Console.WriteLine("InitializeDb: iniciando proceso de creación de base de datos...");
                Console.WriteLine($"Directorio base: {AppContext.BaseDirectory}");

                // Ensure Data directory exists
                var dataDir = Path.Combine(AppContext.BaseDirectory, "Data");
                Directory.CreateDirectory(dataDir);
                AppDomain.CurrentDomain.SetData("DataDirectory", dataDir);
                
                Console.WriteLine($"Directorio de datos: {dataDir}");
                Console.WriteLine("Creando esquema de base de datos usando NHibernate...");

                // Create schema BEFORE building the full DI container / opening sessions.
                // This avoids attempting to open NHibernate sessions before the DB exists/ is attached.
                Console.WriteLine("Creando esquema...");
                try 
                {
                    NHibernateHelper.CreateSchema();
                    Console.WriteLine("Esquema creado correctamente.");

                    // Setup DI container after schema exists
                    var services = new ServiceCollection();
                    services.AddInfrastructureServices();
                    // Add CENs and CPs
                    services.AddScoped<UsuarioCEN>();
                    services.AddScoped<ProductoCEN>();
                    var serviceProvider = services.BuildServiceProvider();

                    // Optionally initialize demo data. Use command-line flag --seed to enable seeding.
                    if (args != null && args.Length > 0 && Array.Exists(args, a => a == "--seed"))
                    {
                        var initializer = new DataInitializer(serviceProvider);
                        initializer.InitializeData();
                        Console.WriteLine("Datos inicializados correctamente (flag --seed). ");
                    }
                    else
                    {
                        Console.WriteLine("No se ejecutó la semilla de datos. Para inicializar datos de ejemplo, vuelva a ejecutar con el parámetro --seed.");
                    }

                    Console.WriteLine("InitializeDb completado exitosamente.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error durante la creación del esquema: {ex.Message}");
                    if (ex.InnerException != null)
                        Console.WriteLine($"Error interno: {ex.InnerException.Message}");
                    Console.WriteLine("Stack trace:");
                    Console.WriteLine(ex.StackTrace);
                    throw;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error general: {ex.Message}");
                if (ex.InnerException != null)
                    Console.WriteLine($"Error interno: {ex.InnerException.Message}");
                Console.WriteLine("Stack trace:");
                Console.WriteLine(ex.StackTrace);
                Environment.Exit(1);
                throw;
            }
        }
    }
}

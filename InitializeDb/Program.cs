using System;
using Infrastructure.NHibernate;
using System.IO;

namespace InitializeDb
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("InitializeDb: creando esquema de base de datos usando NHibernate (scaffold).\nEste proceso requiere LocalDB y dotnet en el entorno local.");

            // Ensure Data directory
            var dataDir = Path.Combine(AppContext.BaseDirectory, "Data");
            Directory.CreateDirectory(dataDir);

            // Copy nhibernate.cfg.xml to working dir if it exists in Infrastructure output
            var cfgPath = Path.Combine(AppContext.BaseDirectory, "nhibernate.cfg.xml");
            if (!File.Exists(cfgPath))
            {
                // attempt to locate in infrastructure output path
                var candidate = Path.Combine(AppContext.BaseDirectory, "..", "Infrastructure", "NHibernate", "nhibernate.cfg.xml");
                if (File.Exists(candidate)) File.Copy(candidate, cfgPath, true);
            }

            try
            {
                NHibernateHelper.CreateSchema(cfgPath);
                Console.WriteLine("SchemaExport completado.");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error ejecutando SchemaExport: " + ex.Message);
            }

            Console.WriteLine("InitializeDb finalizado.");
        }
    }
}

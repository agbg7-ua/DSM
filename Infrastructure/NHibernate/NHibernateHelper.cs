using System;
using NHibernate.Cfg;
using NHibernate.Tool.hbm2ddl;
using NHibernate;
using System.IO;
using System.Data.SqlClient;
using System.Text.RegularExpressions;
using System.ServiceProcess;
using System.Threading;
using System.Security.Principal;

namespace Infrastructure.NHibernate
{
    public static class NHibernateHelper
    {
        public static ISessionFactory BuildSessionFactory(string? cfgPath = default)
        {
            var cfg = new Configuration();
            if (string.IsNullOrEmpty(cfgPath))
            {
                var infraDir = Path.GetDirectoryName(typeof(NHibernateHelper).Assembly.Location);
                if (infraDir == null)
                    throw new InvalidOperationException("Cannot determine Infrastructure assembly location");
                
                cfgPath = Path.Combine(infraDir, "NHibernate", "nhibernate.cfg.xml");
                cfgPath = Path.GetFullPath(cfgPath);
            }

            if (!File.Exists(cfgPath))
                throw new FileNotFoundException($"Configuration file not found at: {cfgPath}");

            // Change current directory to where the config file is
            var configDir = Path.GetDirectoryName(cfgPath);
            if (configDir != null)
                Directory.SetCurrentDirectory(configDir);

            cfg.Configure(cfgPath);

            // Ensure mappings resolved: the nhibernate.cfg.xml contains mapping files with relative paths
            // Try building the session factory using the configured connection string. If that fails (for
            // example the configured SQL Server instance is not available), attempt a LocalDB AttachDbFilename
            // fallback so the app can still run locally.
            // Before building, if the configured connection targets SQL Express, ensure the instance is running.
            var configuredConn = cfg.GetProperty("connection.connection_string") ?? string.Empty;
            if (configuredConn.IndexOf("SQLEXPRESS", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                EnsureSqlExpressServiceRunning();
            }

            // Log the final connection string NHibernate will attempt to use
            Console.WriteLine("NHibernate connection string (final): " + (string.IsNullOrEmpty(configuredConn) ? "(empty)" : configuredConn));
            try
            {
                var user = WindowsIdentity.GetCurrent()?.Name ?? "(unknown)";
                Console.WriteLine("Proceso ejecutándose como usuario: " + user);
            }
            catch { }

            // Retry loop: attempt to create a session factory multiple times with exponential backoff to mitigate timing/locking races
            const int maxAttempts = 8;
            var attempt = 0;

            // Try a few alternate DataSource variants if direct connection fails repeatedly.
            // This helps on developer machines where named-instance resolution may vary.
            string TryAlternateConnectionStrings(string orig)
            {
                if (string.IsNullOrEmpty(orig))
                    return orig;

                try
                {
                    var sbOrig = new SqlConnectionStringBuilder(orig);
                    var dataSource = sbOrig.DataSource ?? string.Empty;
                    // If no instance in dataSource, we won't attempt instance variants.
                    string instancePart = null;
                    var idx = dataSource.IndexOf('\\');
                    if (idx >= 0 && idx + 1 < dataSource.Length)
                        instancePart = dataSource.Substring(idx + 1);

                    var hosts = new[] { ".", "localhost", "127.0.0.1", "(local)" };

                    foreach (var host in hosts)
                    {
                        try
                        {
                            var sb = new SqlConnectionStringBuilder(orig);
                            sb.DataSource = instancePart == null ? host : host + "\\" + instancePart;
                            sb.ConnectTimeout = Math.Max(sb.ConnectTimeout, 30);
                            sb.ApplicationName = sb.ApplicationName ?? "InitializeDb";

                            using (var conn = new SqlConnection(sb.ConnectionString))
                            {
                                conn.Open();
                                Console.WriteLine($"Fallback connection successful using DataSource={sb.DataSource}");
                                return sb.ConnectionString;
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Fallback attempt for host '{host}' failed: {ex.Message}");
                            // try next
                        }
                    }
                    // If host variants didn't work, try protocol-prefixed variants for the original data source
                    try
                    {
                        var sbOrig2 = new SqlConnectionStringBuilder(orig);
                        var origDataSource = sbOrig2.DataSource ?? string.Empty;
                        var protoPrefixes = new[] { "np:", "lpc:" }; // named pipes, shared memory
                        foreach (var p in protoPrefixes)
                        {
                            try
                            {
                                var sb = new SqlConnectionStringBuilder(orig);
                                sb.DataSource = p + origDataSource;
                                sb.ConnectTimeout = Math.Max(sb.ConnectTimeout, 30);
                                sb.ApplicationName = sb.ApplicationName ?? "InitializeDb";

                                using (var conn = new SqlConnection(sb.ConnectionString))
                                {
                                    conn.Open();
                                    Console.WriteLine($"Fallback protocol connection successful using DataSource={sb.DataSource}");
                                    return sb.ConnectionString;
                                }
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"Fallback protocol attempt '{p}' failed: {ex.Message}");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Error testing protocol-prefixed connection strings: " + ex.Message);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error while testing alternate connection strings: " + ex.Message);
                }

                return null;
            }
            
            // Diagnostic helper: enumerate SQL Server instances visible on the network / local machine
            void PrintSqlInstances()
            {
                // SqlDataSourceEnumerator is not always available in all runtime packages; keep diagnostics minimal.
                try
                {
                    Console.WriteLine("Nota: para resolver instancias nombradas el servicio SQL Server Browser debe estar en ejecución.");
                    try
                    {
                        using (var sc = new ServiceController("SQLBrowser"))
                        {
                            Console.WriteLine($"Servicio SQL Browser (SQLBrowser) estado: {sc.Status}");
                            if (sc.Status != ServiceControllerStatus.Running)
                            {
                                try
                                {
                                    Console.WriteLine("Intentando iniciar SQL Browser automáticamente (puede requerir privilegios de administrador)...");
                                    sc.Start();
                                    sc.WaitForStatus(ServiceControllerStatus.Running, TimeSpan.FromSeconds(10));
                                    Console.WriteLine("SQL Browser iniciado correctamente.");
                                }
                                catch (Exception exStart)
                                {
                                    Console.WriteLine("No se pudo iniciar SQL Browser automáticamente: " + exStart.Message);
                                    Console.WriteLine("Por favor, inicie el servicio 'SQL Server Browser' manualmente (services.msc) o ejecute: Start-Service SQLBrowser con privilegios elevados.");
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("No se pudo verificar el servicio SQL Browser: " + ex.Message);
                    }

                    Console.WriteLine("Si la resolución de instancia falla, verifique SQL Server Configuration Manager -> Network Configuration (habilitar TCP/IP/Named Pipes) y que SQL Browser esté en ejecución.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error en diagnóstico de instancias SQL: " + ex.Message);
                }
            }
            while (true)
            {
                attempt++;
                try
                {
                    // Before first attempt, if the configured connection fails, try automated fallbacks
                    if (attempt == 1 && !string.IsNullOrEmpty(configuredConn))
                    {
                        // Print diagnostic enumerations to help diagnose instance resolution
                        PrintSqlInstances();

                        var alt = TryAlternateConnectionStrings(configuredConn);
                        if (!string.IsNullOrEmpty(alt) && alt != configuredConn)
                        {
                            configuredConn = alt;
                            cfg.SetProperty("connection.connection_string", configuredConn);
                            Console.WriteLine("Se actualizó la cadena de conexión a una variante que funcionó: " + configuredConn);
                        }
                    }

                    // If there is a configured connection, perform a quick SqlConnection test to get clearer errors early
                    if (!string.IsNullOrEmpty(configuredConn))
                    {
                        // Use SqlConnectionStringBuilder to normalize and allow tuning (timeout, app name)
                        try
                        {
                            var sb = new SqlConnectionStringBuilder(configuredConn);
                            Console.WriteLine($"Probar conexión a DataSource={sb.DataSource}, InitialCatalog={sb.InitialCatalog}, IntegratedSecurity={sb.IntegratedSecurity}");
                            // Increase connect timeout to be more tolerant during attach/recovery
                            sb.ConnectTimeout = sb.ConnectTimeout <= 0 ? 30 : Math.Max(sb.ConnectTimeout, 30);
                            sb.ApplicationName = sb.ApplicationName ?? "InitializeDb";

                            using (var testConn = new SqlConnection(sb.ConnectionString))
                            {
                                try
                                {
                                    testConn.Open();
                                    Console.WriteLine("Conexión de prueba a la base de datos configurada correcta.");
                                }
                            catch (SqlException sqlex)
                            {
                                // Print richer SqlException details to help diagnose SNI/instance/login issues
                                Console.WriteLine("SqlException durante la conexión de prueba:");
                                Console.WriteLine($"Message: {sqlex.Message}");
                                Console.WriteLine($"Number: {sqlex.Number}, State: {sqlex.State}");
                                Console.WriteLine("Errors:");
                                foreach (SqlError err in sqlex.Errors)
                                {
                                    Console.WriteLine($" - Number={err.Number}, Message={err.Message}, State={err.State}, Class={err.Class}");
                                }
                                throw;
                            }
                            catch (Exception exConn)
                            {
                                // If this is a SqlException or contains one as inner, log richer details
                                if (exConn is SqlException sex)
                                {
                                    Console.WriteLine("SqlException (unexpected) durante la conexión de prueba:");
                                    Console.WriteLine(sex.ToString());
                                }
                                else if (exConn.InnerException is SqlException innerSql)
                                {
                                    Console.WriteLine("Inner SqlException durante la conexión de prueba:");
                                    Console.WriteLine(innerSql.ToString());
                                }
                                else
                                {
                                    Console.WriteLine("Error no esperado durante la conexión de prueba: " + exConn.ToString());
                                }

                                throw;
                            }
                            }
                        }
                        catch (Exception outerEx)
                        {
                            Console.WriteLine("Error al preparar la cadena de conexión para la comprobación: " + outerEx.ToString());
                            throw;
                        }
                    }

                    var sf = cfg.BuildSessionFactory();
                    Console.WriteLine($"ISessionFactory creado usando la cadena configurada (intento {attempt}).");
                    return sf;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Intento {attempt} para crear ISessionFactory falló: {ex.Message}");
                    Console.WriteLine("Detalle de la excepción:");
                    Console.WriteLine(ex.ToString());
                    if (attempt >= maxAttempts)
                    {
                        Console.WriteLine("Número máximo de reintentos alcanzado. Intentando fallback a LocalDB para permitir inicialización local...\n(esto creará/adjuntará un MDF en la carpeta Data si LocalDB está disponible)");

                        try
                        {
                            // Prepare a local MDF path under the app's Data directory
                            var dataDir = AppDomain.CurrentDomain.GetData("DataDirectory") as string;
                            if (string.IsNullOrEmpty(dataDir))
                            {
                                dataDir = Path.Combine(AppContext.BaseDirectory, "Data");
                                Directory.CreateDirectory(dataDir);
                                AppDomain.CurrentDomain.SetData("DataDirectory", dataDir);
                            }

                            var mdfPath = Path.Combine(dataDir, "DSM_EShop.mdf");
                            Directory.CreateDirectory(Path.GetDirectoryName(mdfPath) ?? dataDir);

                            var masterConn = "Server=(LocalDB)\\MSSQLLocalDB;Database=master;Integrated Security=True;TrustServerCertificate=True";
                            using (var conn = new SqlConnection(masterConn))
                            {
                                conn.Open();
                                using (var cmd = conn.CreateCommand())
                                {
                                    var safePath = mdfPath.Replace("'", "''");
                                    cmd.CommandText = $"IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'DSM_EShop') CREATE DATABASE [DSM_EShop] ON (NAME = N'DSM_EShop', FILENAME = '{safePath}')";
                                    Console.WriteLine("Creando/asegurando base de datos MDF en LocalDB: " + mdfPath);
                                    cmd.ExecuteNonQuery();
                                }
                            }

                            var localConn = $"Server=(LocalDB)\\MSSQLLocalDB;AttachDbFilename={mdfPath};Integrated Security=True;TrustServerCertificate=True;MultipleActiveResultSets=True";
                            Console.WriteLine("Usando cadena de conexión LocalDB: " + localConn);
                            configuredConn = localConn;
                            cfg.SetProperty("connection.connection_string", configuredConn);

                            // One more attempt to build the session factory using LocalDB
                            try
                            {
                                var sf2 = cfg.BuildSessionFactory();
                                Console.WriteLine("ISessionFactory creado usando LocalDB fallback.");
                                return sf2;
                            }
                            catch (Exception ex2)
                            {
                                Console.WriteLine("El fallback a LocalDB falló: " + ex2.ToString());
                                // fall through to throw original
                            }
                        }
                        catch (Exception fallbackEx)
                        {
                            Console.WriteLine("Error durante el intento de fallback a LocalDB: " + fallbackEx.ToString());
                        }

                        Console.WriteLine("Número máximo de reintentos alcanzado y fallback a LocalDB falló (o no disponible). Revise el estado de la instancia SQL Server y la cadena de conexión.");
                        throw;
                    }

                    // Exponential backoff: base 500ms * 2^(attempt-1)
                    var waitMs = (int)(500 * Math.Pow(2, attempt - 1));
                    Console.WriteLine($"Esperando {waitMs}ms antes de reintentar...");
                    Thread.Sleep(waitMs);
                    Console.WriteLine("Reintentando crear ISessionFactory...");
                }
            }
        }

        private static void EnsureSqlExpressServiceRunning()
        {
            try
            {
                // Common named instance service name
                var serviceName = "MSSQL$SQLEXPRESS";
                using (var sc = new ServiceController(serviceName))
                {
                    if (sc.Status != ServiceControllerStatus.Running)
                    {
                        throw new InvalidOperationException($"El servicio '{serviceName}' no está en ejecución (Estado={sc.Status}). Por favor inicie SQL Server Express o ajuste la cadena de conexión.");
                    }
                }
            }
            catch (InvalidOperationException)
            {
                // Re-throw known service issues
                throw;
            }
            catch (Exception ex)
            {
                // If we couldn't access ServiceController (permissions), still surface a helpful message
                throw new InvalidOperationException("No se pudo comprobar el servicio de SQL Server Express (MSSQL$SQLEXPRESS). Asegúrese de que SQL Server Express esté instalado y en ejecución. Detalle: " + ex.Message, ex);
            }
        }

        public static void CreateSchema(string? cfgPath = default)
        {
            var cfg = new Configuration();
            if (string.IsNullOrEmpty(cfgPath))
            {
                var infraDir = Path.GetDirectoryName(typeof(NHibernateHelper).Assembly.Location);
                if (infraDir == null)
                    throw new InvalidOperationException("Cannot determine Infrastructure assembly location");
                
                cfgPath = Path.Combine(infraDir, "NHibernate", "nhibernate.cfg.xml");
                cfgPath = Path.GetFullPath(cfgPath);
            }

            if (!File.Exists(cfgPath))
                throw new FileNotFoundException($"Configuration file not found at: {cfgPath}");

            var path = Path.GetDirectoryName(cfgPath);
            if (path != null)
                Directory.SetCurrentDirectory(path);

            cfg.Configure(cfgPath);
            // Get configured connection string (from nhibernate.cfg.xml)
            var configuredConn = cfg.GetProperty("connection.connection_string");

            // Resolve |DataDirectory| token if present
            var dataDirectory = AppDomain.CurrentDomain.GetData("DataDirectory") as string;
            if (string.IsNullOrEmpty(dataDirectory))
            {
                // fallback to a Data folder next to the executable
                dataDirectory = Path.Combine(AppContext.BaseDirectory, "Data");
                Directory.CreateDirectory(dataDirectory);
                AppDomain.CurrentDomain.SetData("DataDirectory", dataDirectory);
            }

            if (!string.IsNullOrEmpty(configuredConn) && configuredConn.IndexOf("AttachDbFilename", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                // Replace token and build full path for MDF
                var resolved = configuredConn.Replace("|DataDirectory|", dataDirectory);

                // Extract the AttachDbFilename value (rough parsing)
                var m = Regex.Match(resolved, @"AttachDbFileName\s*=\s*([^;]+)", RegexOptions.IgnoreCase);
                string mdfPath;
                if (m.Success)
                {
                    mdfPath = m.Groups[1].Value.Trim().Trim('\'','\"');
                    // If it was relative, make absolute relative to dataDirectory
                    if (!Path.IsPathRooted(mdfPath))
                        mdfPath = Path.GetFullPath(Path.Combine(dataDirectory, mdfPath));
                }
                else
                {
                    // fallback: create a ProjectDatabase.mdf in dataDirectory
                    mdfPath = Path.Combine(dataDirectory, "ProjectDatabase.mdf");
                }

                // Ensure Data directory exists
                Directory.CreateDirectory(Path.GetDirectoryName(mdfPath) ?? dataDirectory);

                // Use LocalDB master to create database file if it does not exist or DB not attached
                var masterConn = "Server=(LocalDB)\\MSSQLLocalDB;Database=master;Integrated Security=True;TrustServerCertificate=True";
                try
                {
                    using (var conn = new SqlConnection(masterConn))
                    {
                        conn.Open();
                        Console.WriteLine("Conexión establecida con master DB (LocalDB)...");
                        using (var cmd = conn.CreateCommand())
                        {
                            // Create DB with the specified filename if a database with that logical name doesn't exist
                            // Use DSM_EShop as logical DB name (idempotent)
                            var safePath = mdfPath.Replace("'", "''");
                            cmd.CommandText = $"IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'DSM_EShop') CREATE DATABASE [DSM_EShop] ON (NAME = N'DSM_EShop', FILENAME = '{safePath}')";
                            Console.WriteLine("Creando base de datos (si no existe) asociada al archivo MDF...");
                            cmd.ExecuteNonQuery();
                            Console.WriteLine("Operación de creación/verificación completada.");
                        }
                    }

                    // Now set the connection string to point to the attach file explicitly
                    var finalConn = resolved; // contains AttachDbFilename replaced with absolute path
                    cfg.SetProperty("connection.connection_string", finalConn);
                    Console.WriteLine("Conexión configurada para usar el archivo MDF: " + mdfPath);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Advertencia: no se pudo crear/adjuntar la base de datos MDF via LocalDB. Se intentará usar la cadena de conexión original.");
                    Console.WriteLine(ex.Message);
                    // fallback: keep configuredConn (may point to server-based DB)
                    cfg.SetProperty("connection.connection_string", configuredConn);
                }
            }
            else
            {
                // No AttachDbFilename in config; attempt to ensure DB exists when using a server-based connection
                if (!string.IsNullOrEmpty(configuredConn) && configuredConn.IndexOf("SQLEXPRESS", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    try
                    {
                        using (var conn = new SqlConnection("Server=.\\SQLEXPRESS;Database=master;Trusted_Connection=True;TrustServerCertificate=True"))
                        {
                            conn.Open();
                            Console.WriteLine("Conexión establecida con master DB (SQLEXPRESS)...");
                            using (var cmd = conn.CreateCommand())
                            {
                                cmd.CommandText = "IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'DSM_EShop') CREATE DATABASE DSM_EShop";
                                Console.WriteLine("Creando base de datos si no existe...");
                                cmd.ExecuteNonQuery();
                                Console.WriteLine("Operación de creación/verificación completada.");
                            }
                        }

                        cfg.SetProperty("connection.connection_string", configuredConn.Contains("Database=", StringComparison.OrdinalIgnoreCase)
                            ? configuredConn
                            : "Server=.\\SQLEXPRESS;Database=DSM_EShop;Trusted_Connection=True;TrustServerCertificate=True");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Advertencia: no se pudo crear la base de datos en SQLEXPRESS: " + ex.Message);
                        cfg.SetProperty("connection.connection_string", configuredConn);
                    }
                }
                else
                {
                    // No special handling; use the configured connection string
                    cfg.SetProperty("connection.connection_string", configuredConn);
                }
            }

            // Apply schema updates idempotently using SchemaUpdate (won't drop existing tables)
            Console.WriteLine("Iniciando actualización del esquema (SchemaUpdate)...");
            var updater = new SchemaUpdate(cfg);
            Console.WriteLine("Aplicando cambios con SchemaUpdate (no se eliminarán tablas existentes)...");
            // Execute updates: first parameter - script to console, second - apply to database
            updater.Execute(true, true);
            Console.WriteLine("Actualización del esquema completada.");

            // Small stabilization delay: wait a short time to allow SQL Server to finish background recovery/attach operations
            try
            {
                const int stabilizationMs = 5000;
                Console.WriteLine($"Esperando {stabilizationMs}ms para estabilizar la base antes de abrir conexiones...");
                System.Threading.Thread.Sleep(stabilizationMs);
            }
            catch { }
        }
    }
}

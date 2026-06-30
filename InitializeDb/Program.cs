using ApplicationCore.Domain.CEN;
using ApplicationCore.Domain.CP;
using ApplicationCore.Domain.EN;
using ApplicationCore.Domain.Repositories;
using Infrastructure.NHibernate;
using Infrastructure.NHibernate.Repositories;
using Infrastructure.NHibernate.UnitOfWork;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NHibernate;

const string DatabaseName = "GestionMakerspace";
const string SqlExpressConnectionString =
    $"Server=.\\SQLEXPRESS;Database={DatabaseName};Trusted_Connection=True;TrustServerCertificate=True;";

var dataDir = Path.Combine(AppContext.BaseDirectory, "Data");
Directory.CreateDirectory(dataDir);

var mdfPath = Path.Combine(dataDir, $"{DatabaseName}.mdf");
var localDbConnectionString =
    $"Server=(localdb)\\MSSQLLocalDB;Database={DatabaseName};AttachDbFilename={mdfPath};Trusted_Connection=True;MultipleActiveResultSets=True;";

using var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
var logger = loggerFactory.CreateLogger("InitializeDb");

try {
    var config = NHibernateHelper.LoadConfiguration(SqlExpressConnectionString);

    string connectionStringUtilizada = ExportSchemaWithFallback(config, SqlExpressConnectionString, localDbConnectionString, logger);

    var services = BuildServiceProvider(connectionStringUtilizada);
    logger.LogInformation("Contenedor DI registrado. Ejecutando seed...");

    InsertarDatosPrueba(services, logger);

    logger.LogInformation("InitializeDb completado.");
}
catch (Exception ex) {
    logger.LogError(ex, "Error durante InitializeDb.");
    Environment.ExitCode = 1;
}

static string ExportSchemaWithFallback(
    NHibernate.Cfg.Configuration config,
    string primaryConnectionString,
    string fallbackConnectionString,
    ILogger logger) {
    try {
        config.SetProperty(NHibernate.Cfg.Environment.ConnectionString, primaryConnectionString);
        EnsureDatabaseExists(primaryConnectionString, logger);
        TestConnection(primaryConnectionString);
        logger.LogInformation("Usando conexión SQL Express: {Connection}", primaryConnectionString);
        NHibernateHelper.ExportSchema(config);

        return primaryConnectionString;
    }
    catch (Exception ex) when (IsConnectionFailure(ex)) {
        logger.LogWarning(ex, "Fallo conexión SQL Express. Reintentando con LocalDB...");
        config.SetProperty(NHibernate.Cfg.Environment.ConnectionString, fallbackConnectionString);
        RecreateLocalDbIfNeeded(fallbackConnectionString, logger);
        NHibernateHelper.ExportSchema(config);
        logger.LogInformation("Esquema creado en LocalDB: {Connection}", fallbackConnectionString);

        return fallbackConnectionString;
    }
}

static void EnsureDatabaseExists(string connectionString, ILogger logger) {
    var builder = new SqlConnectionStringBuilder(connectionString);
    var databaseName = builder.InitialCatalog;
    var masterConnStr = new SqlConnectionStringBuilder(connectionString) {
        InitialCatalog = "master"
    }.ConnectionString;

    using var connection = new SqlConnection(masterConnStr);
    connection.Open();

    using var cmd = connection.CreateCommand();
    cmd.CommandText = $"IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = N'{databaseName}') CREATE DATABASE [{databaseName}]";
    cmd.ExecuteNonQuery();
    logger.LogInformation("Base de datos {Database} verificada/creada.", databaseName);
}

static void TestConnection(string connectionString) {
    using var connection = new SqlConnection(connectionString);
    connection.Open();
}

static bool IsConnectionFailure(Exception ex) =>
    ex is SqlException or NHibernate.Exceptions.GenericADOException or InvalidOperationException;

static void RecreateLocalDbIfNeeded(string connectionString, ILogger logger) {
    try {
        TestConnection(connectionString);
    }
    catch {
        var builder = new SqlConnectionStringBuilder(connectionString);
        var databaseName = builder.InitialCatalog;
        var mdfPath = builder.AttachDBFilename;

        var masterConnection = new SqlConnectionStringBuilder(connectionString) {
            InitialCatalog = "master",
            AttachDBFilename = string.Empty
        }.ConnectionString;

        using var connection = new SqlConnection(masterConnection);
        connection.Open();

        using (var cmd = connection.CreateCommand()) {
            cmd.CommandText = $"""
                IF DB_ID(N'{databaseName}') IS NOT NULL
                BEGIN
                    ALTER DATABASE [{databaseName}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
                    DROP DATABASE [{databaseName}];
                END
                """;
            cmd.ExecuteNonQuery();
        }

        if (!string.IsNullOrEmpty(mdfPath) && File.Exists(mdfPath)) {
            try {
                File.Delete(mdfPath);
                var ldfPath = mdfPath.Replace(".mdf", "_log.ldf", StringComparison.OrdinalIgnoreCase);
                if (File.Exists(ldfPath)) {
                    File.Delete(ldfPath);
                }  
            }
            catch (Exception ex) {
                logger.LogWarning("No se pudieron eliminar los archivos físicos .mdf/.ldf: {Message}", ex.Message);
            }
        }

        logger.LogInformation("Base de datos LocalDB {Database} y sus archivos físicos recreados.", databaseName);
    }
}

static void InsertarDatosPrueba(ServiceProvider services, ILogger logger) {
    using var scope = services.CreateScope();
    var materialCEN = scope.ServiceProvider.GetRequiredService<MaterialCEN>();
    var usuarioCEN = scope.ServiceProvider.GetRequiredService<UsuarioCEN>();
    var prestamoCEN = scope.ServiceProvider.GetRequiredService<PrestamoCEN>();
    var lineaPrestamoCEN = scope.ServiceProvider.GetRequiredService<LineaPrestamoCEN>();
    var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

    try {
        //Usuarios
        long adminId = usuarioCEN.Crear("Juan García", "juan@makerspace.com", "1234", ApplicationCore.Domain.Enums.RolUsuario.Administrador);
        long usuarioId = usuarioCEN.Crear("María López", "maria@makerspace.com", "1234", ApplicationCore.Domain.Enums.RolUsuario.Usuario);

        //Materiales
        long taladroId = materialCEN.Crear("Taladro eléctrico", "Taladro percutor 800W", ApplicationCore.Domain.Enums.EstadoMaterial.Disponible, true, "/Images/taladro.jpg");
        long sierraId = materialCEN.Crear("Sierra circular", "Sierra circular 1200W con guía", ApplicationCore.Domain.Enums.EstadoMaterial.Disponible, true, "/Images/sierra.jpg");
        long impresoraId = materialCEN.Crear("Impresora 3D", "Impresora FDM con cama caliente", ApplicationCore.Domain.Enums.EstadoMaterial.Disponible, true, "/Images/impresora.jpg");
        long soldadorId = materialCEN.Crear("Soldador", "Soldador de estaño 60W", ApplicationCore.Domain.Enums.EstadoMaterial.EnMantenimiento, false, "/Images/soldador.jpg");

        //Préstamos
        long prestamo1Id = prestamoCEN.Crear(usuarioId, DateTime.Now.AddDays(-5), ApplicationCore.Domain.Enums.EstadoPrestamo.Activo, 7);
        lineaPrestamoCEN.Crear(prestamo1Id, taladroId,3);
        lineaPrestamoCEN.Crear(prestamo1Id, impresoraId,5);

        long prestamo2Id = prestamoCEN.Crear(adminId, DateTime.Now.AddDays(-20), ApplicationCore.Domain.Enums.EstadoPrestamo.Devuelto, 3);
        lineaPrestamoCEN.Crear(prestamo2Id, sierraId,2);

        long prestamo3Id = prestamoCEN.Crear(usuarioId, DateTime.Now, ApplicationCore.Domain.Enums.EstadoPrestamo.Pendiente, 5);
        lineaPrestamoCEN.Crear(prestamo3Id, taladroId,7);

        unitOfWork.SaveChanges();
        logger.LogInformation("Datos de prueba insertados correctamente.");
    }
    catch (Exception ex) {
        logger.LogError(ex, "Error insertando datos de prueba.");
        throw;
    }
}

static ServiceProvider BuildServiceProvider(string connectionString) {
    var services = new ServiceCollection();

    var sessionFactory = NHibernateHelper.BuildSessionFactory(connectionString);

    services.AddSingleton(sessionFactory);
    services.AddScoped<ISession>(provider => provider.GetRequiredService<ISessionFactory>().OpenSession());

    services.AddScoped<IUsuarioRepository, UsuarioRepository>();
    services.AddScoped<IMaterialRepository, MaterialRepository>();
    services.AddScoped<IPrestamoRepository, PrestamoRepository>();
    services.AddScoped<ILineaPrestamoRepository, LineaPrestamoRepository>();
    services.AddScoped<IUnitOfWork, NHibernateUnitOfWork>();

    services.AddScoped<UsuarioCEN>();
    services.AddScoped<MaterialCEN>();
    services.AddScoped<PrestamoCEN>();
    services.AddScoped<LineaPrestamoCEN>();
    services.AddScoped<CasosProceso>();

    return services.BuildServiceProvider();
}

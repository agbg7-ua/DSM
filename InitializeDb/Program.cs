using ApplicationCore.Domain.CEN;
using ApplicationCore.Domain.CP;
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
    $"Server=localhost\\SQLEXPRESS;Database={DatabaseName};Trusted_Connection=True;TrustServerCertificate=True;";

var dataDir = Path.Combine(AppContext.BaseDirectory, "Data");
Directory.CreateDirectory(dataDir);

var mdfPath = Path.Combine(dataDir, $"{DatabaseName}.mdf");
var localDbConnectionString =
    $"Server=(localdb)\\MSSQLLocalDB;Database={DatabaseName};AttachDbFilename={mdfPath};Trusted_Connection=True;MultipleActiveResultSets=True;";

using var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
var logger = loggerFactory.CreateLogger("InitializeDb");

try
{
    var config = NHibernateHelper.LoadConfiguration(SqlExpressConnectionString);
    ExportSchemaWithFallback(config, SqlExpressConnectionString, localDbConnectionString, logger);

    var services = BuildServiceProvider(localDbConnectionString);
    logger.LogInformation("Contenedor DI registrado. Seed no ejecutado (hook disponible en Program.cs).");

    logger.LogInformation("InitializeDb completado.");
}
catch (Exception ex)
{
    logger.LogError(ex, "Error durante InitializeDb.");
    Environment.ExitCode = 1;
}

static void ExportSchemaWithFallback(
    NHibernate.Cfg.Configuration config,
    string primaryConnectionString,
    string fallbackConnectionString,
    ILogger logger)
{
    try
    {
        config.SetProperty(NHibernate.Cfg.Environment.ConnectionString, primaryConnectionString);
        TestConnection(primaryConnectionString);
        logger.LogInformation("Usando conexión SQL Express: {Connection}", primaryConnectionString);
        NHibernateHelper.ExportSchema(config);
    }
    catch (Exception ex) when (IsConnectionFailure(ex))
    {
        logger.LogWarning(ex, "Fallo conexión SQL Express. Reintentando con LocalDB...");
        config.SetProperty(NHibernate.Cfg.Environment.ConnectionString, fallbackConnectionString);
        RecreateLocalDbIfNeeded(fallbackConnectionString, logger);
        NHibernateHelper.ExportSchema(config);
        logger.LogInformation("Esquema creado en LocalDB: {Connection}", fallbackConnectionString);
    }
}

static void TestConnection(string connectionString)
{
    using var connection = new SqlConnection(connectionString);
    connection.Open();
}

static bool IsConnectionFailure(Exception ex) =>
    ex is SqlException or NHibernate.Exceptions.GenericADOException or InvalidOperationException;

static void RecreateLocalDbIfNeeded(string connectionString, ILogger logger)
{
    try
    {
        TestConnection(connectionString);
    }
    catch
    {
        var builder = new SqlConnectionStringBuilder(connectionString);
        var databaseName = builder.InitialCatalog;
        var masterConnection = new SqlConnectionStringBuilder(connectionString)
        {
            InitialCatalog = "master",
            AttachDBFilename = string.Empty
        }.ConnectionString;

        using var connection = new SqlConnection(masterConnection);
        connection.Open();

        using (var cmd = connection.CreateCommand())
        {
            cmd.CommandText = $"""
                IF DB_ID(N'{databaseName}') IS NOT NULL
                BEGIN
                    ALTER DATABASE [{databaseName}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
                    DROP DATABASE [{databaseName}];
                END
                """;
            cmd.ExecuteNonQuery();
        }

        logger.LogInformation("Base de datos LocalDB {Database} recreada.", databaseName);
    }
}

static ServiceProvider BuildServiceProvider(string connectionString) {
    var services = new ServiceCollection();

    var sessionFactory = NHibernateHelper.BuildSessionFactory(connectionString);

    // El Factory sí es Singleton (uno solo para toda la app)
    services.AddSingleton(sessionFactory);

    // CORRECCIÓN: La sesión DEBE ser Scoped para que se abra y cierre en cada petición
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

// Hook para seed futuro (idempotente):
// var provider = BuildServiceProvider(localDbConnectionString);
// using var scope = provider.CreateScope();
// var usuarioCEN = scope.ServiceProvider.GetRequiredService<UsuarioCEN>();
// var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
// if (!usuarioCEN.ObtenerTodos().Any())
// {
//     usuarioCEN.Crear("Admin", "admin@makerspace.local", "admin123", RolUsuario.Administrador);
//     unitOfWork.SaveChanges();
// }

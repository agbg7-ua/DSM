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

    var Estado = ApplicationCore.Domain.Enums.EstadoMaterial.Disponible;
    var EnMantenimiento = ApplicationCore.Domain.Enums.EstadoMaterial.EnMantenimiento;
    var Roto = ApplicationCore.Domain.Enums.EstadoMaterial.Roto;
    var Prestado = ApplicationCore.Domain.Enums.EstadoMaterial.Prestado;

    try {
        //Usuarios
        long adminId = usuarioCEN.Crear("Juan García", "juan@makerspace.com", "1234", ApplicationCore.Domain.Enums.RolUsuario.Administrador);
        usuarioCEN.Crear("Lucía Fernández", "lucia@makerspace.com", "1234", ApplicationCore.Domain.Enums.RolUsuario.Administrador);
        long mariaId = usuarioCEN.Crear("María López", "maria@makerspace.com", "1234", ApplicationCore.Domain.Enums.RolUsuario.Usuario);
        long anaId = usuarioCEN.Crear("Ana Torres", "ana@makerspace.com", "1234", ApplicationCore.Domain.Enums.RolUsuario.Usuario);
        long carlosId = usuarioCEN.Crear("Carlos Ruiz", "carlos@makerspace.com", "1234", ApplicationCore.Domain.Enums.RolUsuario.Usuario);
        long pabloId = usuarioCEN.Crear("Pablo Díaz", "pablo@makerspace.com", "1234", ApplicationCore.Domain.Enums.RolUsuario.Usuario);

        //Materiales: bastantes por cada categoría, para poder probar bien el filtro.
        var C = ApplicationCore.Domain.Enums.CategoriaMaterial.Herramientas;
        long taladroId = materialCEN.Crear("Taladro eléctrico", "Taladro percutor 800W, incluye maletín y brocas variadas.", Estado, C, "/Images/taladro.jpg");
        long sierraId = materialCEN.Crear("Sierra circular", "Sierra circular 1200W con guía de corte y disco de repuesto.", Estado, C, "/Images/sierra.jpg");
        long atornilladorId = materialCEN.Crear("Atornillador eléctrico", "Atornillador inalámbrico 12V con dos baterías.", Estado, C, "");
        long lijadoraId = materialCEN.Crear("Lijadora orbital", "Lijadora orbital de mano, incluye lijas de varios grosores.", EnMantenimiento, C, "");
        long pistolaCalorId = materialCEN.Crear("Pistola de calor", "Pistola de aire caliente regulable, útil para termorretráctil.", Estado, C, "");

        C = ApplicationCore.Domain.Enums.CategoriaMaterial.Electronica;
        long multimetroId = materialCEN.Crear("Multímetro digital", "Multímetro de mano con medición de voltaje, corriente y continuidad.", Estado, C, "");
        long osciloscopioId = materialCEN.Crear("Osciloscopio", "Osciloscopio digital de 2 canales, 100MHz.", Estado, C, "");
        long estacionSoldaduraId = materialCEN.Crear("Estación de soldadura", "Estación de soldadura con temperatura regulable y soporte.", Estado, C, "");
        long fuenteAlimentacionId = materialCEN.Crear("Fuente de alimentación", "Fuente de alimentación de laboratorio, 0-30V regulable.", Estado, C, "");
        long arduinoKitId = materialCEN.Crear("Kit de Arduino", "Kit de iniciación con placa Arduino Uno, cables y sensores básicos.", Estado, C, "");

        C = ApplicationCore.Domain.Enums.CategoriaMaterial.ImpresionYFabricacion;
        long impresora3dId = materialCEN.Crear("Impresora 3D FDM", "Impresora FDM con cama caliente, admite PLA y PETG.", Estado, C, "/Images/impresora.jpg");
        long impresoraResinaId = materialCEN.Crear("Impresora 3D de resina", "Impresora de resina para piezas de alto detalle.", Estado, C, "");
        long cortadoraLaserId = materialCEN.Crear("Cortadora láser", "Cortadora/grabadora láser de sobremesa, área de trabajo A3.", Roto, C, "");
        long cncId = materialCEN.Crear("CNC de sobremesa", "Fresadora CNC de 3 ejes para madera y plásticos blandos.", Estado, C, "");
        long escaner3dId = materialCEN.Crear("Escáner 3D", "Escáner 3D de mano para digitalizar objetos pequeños.", Estado, C, "");

        C = ApplicationCore.Domain.Enums.CategoriaMaterial.Costura;
        long maquinaCoserId = materialCEN.Crear("Máquina de coser", "Máquina de coser doméstica con varios tipos de puntada.", Estado, C, "");
        long overlockId = materialCEN.Crear("Máquina overlock", "Máquina overlock para rematar y coser telas elásticas.", Estado, C, "");
        long planchaId = materialCEN.Crear("Plancha industrial", "Plancha de vapor de uso intensivo para taller de costura.", Estado, C, "");
        long patronajeId = materialCEN.Crear("Kit de patronaje", "Reglas, escuadras y papel de patronaje para confección.", Estado, C, "");

        C = ApplicationCore.Domain.Enums.CategoriaMaterial.Carpinteria;
        long sierraCalarId = materialCEN.Crear("Sierra de calar", "Sierra de calar eléctrica con hojas de varios tipos.", Estado, C, "");
        long routerId = materialCEN.Crear("Router de carpintero", "Fresadora de mano para trabajos de carpintería.", Estado, C, "");
        long cepilloId = materialCEN.Crear("Cepillo eléctrico", "Cepillo eléctrico para desbastar madera.", EnMantenimiento, C, "");
        long prensaBancoId = materialCEN.Crear("Prensa de banco", "Prensa de banco robusta para sujeción de piezas.", Estado, C, "");

        C = ApplicationCore.Domain.Enums.CategoriaMaterial.Informatica;
        long portatilId = materialCEN.Crear("Portátil de préstamo", "Portátil configurado con software de diseño y CAD.", Estado, C, "");
        long tabletGraficaId = materialCEN.Crear("Tablet gráfica", "Tableta gráfica con lápiz para diseño digital.", Estado, C, "");
        long camaraId = materialCEN.Crear("Cámara réflex", "Cámara réflex digital con objetivo estándar 18-55mm.", Estado, C, "");
        long tripodeId = materialCEN.Crear("Trípode profesional", "Trípode de aluminio, altura regulable hasta 1.6m.", Estado, C, "");
        long proyectorId = materialCEN.Crear("Proyector portátil", "Proyector portátil HD con entrada HDMI y USB.", Estado, C, "");

        C = ApplicationCore.Domain.Enums.CategoriaMaterial.Otros;
        long generadorId = materialCEN.Crear("Generador eléctrico", "Generador eléctrico portátil a gasolina, 2000W.", Estado, C, "");
        long carpaId = materialCEN.Crear("Carpa plegable", "Carpa plegable 3x3m para eventos y ferias.", Estado, C, "");
        long altavozId = materialCEN.Crear("Altavoz portátil", "Altavoz Bluetooth portátil con batería de larga duración.", Estado, C, "");
        long kitManoId = materialCEN.Crear("Kit de herramientas de mano", "Maletín con destornilladores, alicates y llaves variadas.", Estado, C, "");

        //Préstamos: variados en estado y usuario, para poder probar filtros y
        //la restricción de "solo veo los míos".
        long prestamo1Id = prestamoCEN.Crear(mariaId, DateTime.Now.AddDays(-5), ApplicationCore.Domain.Enums.EstadoPrestamo.Activo, 7);
        lineaPrestamoCEN.Crear(prestamo1Id, taladroId, 3);
        lineaPrestamoCEN.Crear(prestamo1Id, impresora3dId, 5);
        materialCEN.Modificar(taladroId, "Taladro eléctrico", "Taladro percutor 800W, incluye maletín y brocas variadas.", Prestado, ApplicationCore.Domain.Enums.CategoriaMaterial.Herramientas, "/Images/taladro.jpg", mariaId);
        materialCEN.Modificar(impresora3dId, "Impresora 3D FDM", "Impresora FDM con cama caliente, admite PLA y PETG.", Prestado, ApplicationCore.Domain.Enums.CategoriaMaterial.ImpresionYFabricacion, "/Images/impresora.jpg", mariaId);

        long prestamo2Id = prestamoCEN.Crear(adminId, DateTime.Now.AddDays(-20), ApplicationCore.Domain.Enums.EstadoPrestamo.Devuelto, 3);
        lineaPrestamoCEN.Crear(prestamo2Id, sierraId, 2);

        long prestamo3Id = prestamoCEN.Crear(mariaId, DateTime.Now, ApplicationCore.Domain.Enums.EstadoPrestamo.Pendiente, 5);
        lineaPrestamoCEN.Crear(prestamo3Id, arduinoKitId, 7);
        materialCEN.Modificar(arduinoKitId, "Kit de Arduino", "Kit de iniciación con placa Arduino Uno, cables y sensores básicos.", Prestado, ApplicationCore.Domain.Enums.CategoriaMaterial.Electronica, "", mariaId);

        long prestamo4Id = prestamoCEN.Crear(anaId, DateTime.Now.AddDays(-2), ApplicationCore.Domain.Enums.EstadoPrestamo.Activo, 10);
        lineaPrestamoCEN.Crear(prestamo4Id, maquinaCoserId, 10);
        lineaPrestamoCEN.Crear(prestamo4Id, planchaId, 10);
        materialCEN.Modificar(maquinaCoserId, "Máquina de coser", "Máquina de coser doméstica con varios tipos de puntada.", Prestado, ApplicationCore.Domain.Enums.CategoriaMaterial.Costura, "", anaId);
        materialCEN.Modificar(planchaId, "Plancha industrial", "Plancha de vapor de uso intensivo para taller de costura.", Prestado, ApplicationCore.Domain.Enums.CategoriaMaterial.Costura, "", anaId);

        long prestamo5Id = prestamoCEN.Crear(carlosId, DateTime.Now.AddDays(-15), ApplicationCore.Domain.Enums.EstadoPrestamo.Retrasado, 5);
        lineaPrestamoCEN.Crear(prestamo5Id, camaraId, 5);
        lineaPrestamoCEN.Crear(prestamo5Id, tripodeId, 5);
        materialCEN.Modificar(camaraId, "Cámara réflex", "Cámara réflex digital con objetivo estándar 18-55mm.", Prestado, ApplicationCore.Domain.Enums.CategoriaMaterial.Informatica, "", carlosId);
        materialCEN.Modificar(tripodeId, "Trípode profesional", "Trípode de aluminio, altura regulable hasta 1.6m.", Prestado, ApplicationCore.Domain.Enums.CategoriaMaterial.Informatica, "", carlosId);

        long prestamo6Id = prestamoCEN.Crear(pabloId, DateTime.Now.AddDays(-1), ApplicationCore.Domain.Enums.EstadoPrestamo.Pendiente, 3);
        lineaPrestamoCEN.Crear(prestamo6Id, multimetroId, 3);
        materialCEN.Modificar(multimetroId, "Multímetro digital", "Multímetro de mano con medición de voltaje, corriente y continuidad.", Prestado, ApplicationCore.Domain.Enums.CategoriaMaterial.Electronica, "", pabloId);

        long prestamo7Id = prestamoCEN.Crear(anaId, DateTime.Now.AddDays(-30), ApplicationCore.Domain.Enums.EstadoPrestamo.Devuelto, 4);
        lineaPrestamoCEN.Crear(prestamo7Id, generadorId, 4);

        long prestamo8Id = prestamoCEN.Crear(mariaId, DateTime.Now.AddDays(-3), ApplicationCore.Domain.Enums.EstadoPrestamo.Activo, 6);
        lineaPrestamoCEN.Crear(prestamo8Id, portatilId, 6);
        materialCEN.Modificar(portatilId, "Portátil de préstamo", "Portátil configurado con software de diseño y CAD.", Prestado, ApplicationCore.Domain.Enums.CategoriaMaterial.Informatica, "", mariaId);

        long prestamo9Id = prestamoCEN.Crear(carlosId, DateTime.Now, ApplicationCore.Domain.Enums.EstadoPrestamo.Pendiente, 2);
        lineaPrestamoCEN.Crear(prestamo9Id, altavozId, 2);
        materialCEN.Modificar(altavozId, "Altavoz portátil", "Altavoz Bluetooth portátil con batería de larga duración.", Prestado, ApplicationCore.Domain.Enums.CategoriaMaterial.Otros, "", carlosId);

        long prestamo10Id = prestamoCEN.Crear(pabloId, DateTime.Now.AddDays(-40), ApplicationCore.Domain.Enums.EstadoPrestamo.Devuelto, 5);
        lineaPrestamoCEN.Crear(prestamo10Id, cepilloId, 5);

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

# Plan de solución — Clean DDD (Monolito)

Este documento describe un árbol de proyecto propuesto para un monolito usando principios Clean Architecture + DDD adaptado a `domain.model.json`.

Objetivos relevantes para esta iteración:
- EN: Entidades de negocio (clases con propiedades y relaciones). Se ubican en `ApplicationCore/Domain/EN`.
- CEN: "Componente Entidad de Negocio" — componente sin estado que agrupa operaciones simples que afectan únicamente a una entidad y expone/consume una interfaz `I{Entity}Repository` (separando CRUD de operaciones custom de negocio).
- CP: Casos de Proceso / Application Services (use cases) que orquestan varias repos, UoW y reglas de negocio.
- Infrastructure: Implementaciones de Repositorios y UnitOfWork (uno por entidad), con adaptadores concretos para EF Core o NHibernate. En esta entrega NO se referencian EF/NH en las entidades ni en los CEN.

Nota importante: en esta iteración todas las interfaces de repositorio y el `IUnitOfWork` son síncronas (métodos `damePorOID`, `dameTodos`, `New`, `Modify`, `Destroy`, `ModifyAll`) y los CEN/CP deben consumir estas interfaces síncronas. Esto simplifica el flujo de inicialización y los CPs que hacen seed desde `InitializeDb`.

He generado las clases de entidad y enums en `ApplicationCore/Domain/EN` y `ApplicationCore/Domain/Enums` a partir de `domain.model.json` (sin referencias a EF/NHibernate). Los archivos están pensados como POCOs/Entidades de negocio puras.

## Convenciones aplicadas
- Tipos: uuid -> System.Guid, date-time -> System.DateTime, number/decimal -> System.Decimal, boolean -> bool, integer -> int.
- Relaciones: si existe una FK (p.ej. CustomerId en Order) se añade la propiedad FK y una navegación simple (p.ej. `Customer Customer` o `ICollection<Order> Orders`).
- Nullables: respetadas según `nullable` en `domain.model.json`.

### Convención para métodos crear/modificar en CENs

- Los métodos `crear` y `modificar` expuestos por los CEN deben recibir únicamente los atributos obligatorios (los definidos como `nullable: false` en `domain.model.json`).
- Los parámetros opcionales (nullable) pueden pasarse como argumentos opcionales (`T?`) o omitirse.
- Dentro del CEN se debe construir la entidad de dominio (EN) con los valores recibidos y pasar la EN resultante al repositorio (`New`/`Modify`).
- Esta convención hace explícito el contrato de creación/modificación, facilita validación previa y evita que el código de infraestructura rellene valores obligatorios de negocio.

Ejemplo: para `Pedido` (campos obligatorios: `clienteId`, `fechaCreacion`, `estado`, `total`) el CEN expondrá `crear(long clienteId, DateTime fechaCreacion, EstadoPedido estado, decimal total)` que internamente construye `new Pedido { ... }` y llama a `IPedidoRepository.New(...)`.


## Árbol propuesto (resumido y adaptado)

Solution.sln
├─ ApplicationCore
│  └─ Domain
│     ├─ EN/                # Entidades de dominio (POCO)
│     ├─ Enums/             # Enums del dominio
│     ├─ Repositories/      # Interfaces de repositorio y IUnitOfWork
│     ├─ CEN/               # Componentes Entidad de Negocio (stateless)
│     └─ CP/                # Casos de uso (application services - operaciones transaccionales)
├─ Infrastructure
│  ├─ EF/                  # Implementación NHibernate (XML mappings, repositories)
│  └─ UnitOfWork/          # Implementación de UoW (si aplica)
│  └─ Utils/			   # Adaptadores que permiten que la lógica acceda a librerías externas mediante DI
└─  InitializeDb            # Proyecto ejecutable de inicialización / seeding / demos (usa CEN/CP)

Nota: los proyectos scaffold generados se dirigen a .NET 8 (TargetFramework = `net8.0`).


## Roles en la arquitectura (resumen)
- EN (Entities): clases puras del dominio con propiedades y relaciones. Se ubican en `ApplicationCore/Domain/EN`.
- CEN (Entity Components): adaptadores de negocio por entidad; exponen operaciones CRUD y operaciones simples que afectan sólo a la entidad (sin orquestar múltiples repositorios). Se ubican en `ApplicationCore/Domain/CEN`.
- CP (Use Cases): orquestadores de alta nivel que combinan varios CEN/repos/UoW y aplican reglas de negocio. Se ubican en `ApplicationCore/Domain/CP`.
- Repositorios & UnitOfWork: interfaces en `ApplicationCore` y adaptadores concretos en `Infrastructure`. Se ubican en `ApplicationCore/Domain/Repositories`.

## Inicialización (LocalDB MDF) — esquema vacío, seed desactivado

Para esta entrega hemos optado por un flujo reproducible y local: `InitializeDb` crea y adjunta un archivo LocalDB (MDF) dentro del repositorio en `InitializeDb/Data/ProjectDatabase.mdf` (sustituir `ProjectDatabase` por el nombre de la base que desees) y ejecuta únicamente la creación del esquema (tablas y relaciones) usando NHibernate SchemaExport. **No** se ejecuta ningún seed automático en esta fase; el proyecto deja un hook/documentación para que el seed se implemente posteriormente usando los servicios CEN/CP.

Flujo aplicado por `InitializeDb` ahora:

1. Crear la carpeta `InitializeDb/Data` si no existe.
2. Generar (adjuntar) el archivo LocalDB MDF `ProjectDatabase.mdf` usando `AttachDBFilename` en la cadena de conexión a `(localdb)\\MSSQLLocalDB`.
3. Cargar los mappings `.hbm.xml` desde `Infrastructure/NHibernate/Mappings` (o desde la carpeta de salida del build) en la configuración NHibernate.
4. Ejecutar `SchemaExport` para crear las tablas y relaciones en la base de datos adjunta.
5. No ejecutar seed: queda vacío. El punto de extensión para el seed está documentado y debe implementarse usando los CEN y CP del `ApplicationCore`.

Comandos para ejecutar el initializer (PowerShell):

```powershell
Push-Location .\InitializeDb
dotnet run --project .\InitializeDb.csproj
Pop-Location
```

Dónde buscar los artefactos:
- El MDF creado (si la ejecución tiene permisos y LocalDB está instalado) estará en `InitializeDb/Data/ProjectDatabase.mdf`.
- Los scripts SQL generados por NHibernate se mostrarán en consola si `show_sql` está habilitado en la configuración.

Hook para implementar seed (sugerencia):

- Registrar NHibernate-based repositories e `IUnitOfWork` en un contenedor DI (IServiceCollection).
- Registrar los CENs (`ApplicationCore.Domain.CEN.*`) y los CPs necesarios.
- Implementar el seed llamando a los métodos de los CEN (que usan las interfaces de repositorio síncronas) y al final invocar `IUnitOfWork.SaveChanges()`.
- Asegurarse de que el seed sea idempotente (comprobar existencia antes de crear registros duplicados).

Razonamiento: usar LocalDB MDT en el repositorio facilita pruebas locales reproductibles y evita la necesidad de acceder a instancias nombradas de SQL Server (SQLEXPRESS). El seed se mantiene separado y se recomienda implementarlo siguiendo las reglas de la arquitectura (CEN/CP, transacciones y UoW).



## Integración DI mínima para inicialización
 - El `InitializeDb` inicializador debe construir un contenedor DI mínimo para registrar:
  - `NHibernate.cfg.xml` con la cadena adecuada ( una cadena a un servidor SQL Server Express).
  - Repositorios concretos (Infrastructure NHibernate implementations) implementando las interfaces síncronas.
  - `IUnitOfWork` (implementación NHibernate) síncrono.
  - Todos los CENs necesarios para el seed (síncronos).
  - CPs que se quieran probar después del seed (síncronos y transaccionales).

## Política de implementaciones de Infrastructure

- Prohibido: Repositorios `InMemory` como implementación por defecto en `Infrastructure` para inicialización/producción. Las implementaciones en memoria solo están permitidas como adaptadores de prueba en proyectos de test separados.
- Requerido: Implementaciones NHibernate deben usar mappings XML (`.hbm.xml`) ubicados en `Infrastructure/NHibernate/Mappings/` o mappings por código explícitamente decididos en la fase de diseño.
  - Registro de mappings: Los archivos `.hbm.xml` deben declararse en `NHibernate.cfg.xml` (o registrarse programáticamente) para que `Configuration.BuildSessionFactory()` los incluya. En esta entrega `NHibernateHelper` ha sido reforzado para reescribir rutas relativas a rutas absolutas en tiempo de ejecución si es necesario (busca en `AppContext.BaseDirectory` y en carpetas comunes `NHibernate/Mappings`).

Ejemplo de entrada en `NHibernate.cfg.xml`:

```xml
<mapping resource="Infrastructure/NHibernate/Mappings/Cliente.hbm.xml" />
<mapping resource="Infrastructure/NHibernate/Mappings/Pedido.hbm.xml" />
<mapping resource="Infrastructure/NHibernate/Mappings/LineaPedido.hbm.xml" />
<mapping resource="Infrastructure/NHibernate/Mappings/Mascota.hbm.xml" />
```

Razonamiento: el uso de mapeos XML garantiza trazabilidad explícita entre entidades de dominio y esquema físico, facilita auditoría de cambios y evita que implementaciones temporales en memoria se filtren al árbol `Infrastructure` del monolito.

## Cambios recientes y notas técnicas (esta iteración)

- Identidad: las entidades usaban Guid para Ids; se cambió a long con generador HiLo de NHibernate. Ventajas: claves compactas (bigint), generación eficiente y agrupada (hilo), y mayor compatibilidad con bases de datos relacionales. Los mapeos `.hbm.xml` ahora usan:

```xml
<id name="Id" column="Id" type="long">
  <generator class="hilo">
    <param name="table">NHibernateUniqueKey</param>
    <param name="column">NextHigh</param>
    <param name="max_lo">100</param>
  </generator>
</id>
```

-- Entidades: `ApplicationCore/Domain/EN/*` ahora usan `long` para `Id`. Las propiedades FK siguen la convención `{EntityName}Id` (p.ej. `OrderId`, `CustomerId`).

- Mappings: se eliminaron mapeos duplicados de columnas FK (antes se mapeaban tanto la propiedad FK como la asociación many-to-one, lo que producía errores) — ahora las FKs se representan únicamente mediante las asociaciones many-to-one en los `.hbm.xml`.

- NHibernateHelper: ahora carga `NHibernate.cfg.xml` desde `AppContext.BaseDirectory` si existe y reescribe entradas `<mapping file="..."/>` relativas a rutas absolutas dentro del `bin/` para evitar `DirectoryNotFoundException` cuando se ejecuta desde `InitializeDb`.

- InitializeDb: se añadió un fallback robusto para entornos de desarrollo donde `localhost\SQLEXPRESS` no sea accesible por TCP desde el proceso:
  - Si ocurre `Instance failure` al abrir la conexión, el inicializador cambia temporalmente la cadena NHibernate a LocalDB `(localdb)\\MSSQLLocalDB` y reintenta.
  -- Si la base de datos LocalDB ya tenía tablas incompatibles, el inicializador elimina y recrea la BD LocalDB `ProjectDatabase` antes de ejecutar `SchemaExport` para garantizar un esquema limpio.
  - El seed usa CENs y CPs y no asigna manualmente Ids; NHibernate genera los Ids con HiLo.

- Entidades y repositorios: se actualizó la API de repositorios para usar `long` como clave (p.ej. `IRepository<T,long>`, `IPedidoRepository.GetPendientesByCliente(long clienteId)`).

## Cómo ejecutar el initializer (resumen)

1. Asegúrate de que `SQL Server Browser` esté en ejecución si quieres que la instancia nombrada `localhost\\SQLEXPRESS` sea resuelta por el proceso. Si no, el initializer fallará contra esa instancia y aplicará la ruta LocalDB fallback.
2. Ejecutar (desde la raíz del repo):

```powershell
Push-Location .\InitializeDb
dotnet run --project .
Pop-Location
```

3. Salidas relevantes:
  - Logs de NHibernate mostrados en consola (SQL generado). 
  - Mensaje final: `InitializeDb completado.` en caso de éxito.

## Archivos modificados clave en esta iteración
- `ApplicationCore/Domain/EN/*.cs` — cambios: Ids a `long`, propiedades virtuales para NHibernate.
- `Infrastructure/NHibernate/Mappings/*.hbm.xml` — cambios: generator -> hilo, eliminación de mapeos FK duplicados, tipos de columna `long`.
- `Infrastructure/NHibernate/NHibernateHelper.cs` — carga robusta de `NHibernate.cfg.xml` y reescritura de rutas de mapping.
- `InitializeDb/Program.cs` — creación automática de BD y login SQL, fallback a LocalDB, drop/recreate LocalDB si detecta esquema incompatible y uso de CEN/CP para el seed.
- `ApplicationCore/Domain/Repositories/*` — interfaces adaptadas a `long` como clave.


## Buenas prácticas para CPs y CENs
- CPs deben ser transaccionales: orquestar cambios y persistir una sola vez por operación de negocio.
- CENs exponen comportamiento limitado y reutilizable 
- Evitar lógica de orquestación en los CENs; mantenerlos simples y testeables.

## Siguientes pasos recomendados (priorizados y genéricos)
1. Seguridad y dependencias
	- Revisar dependencias NuGet y planificar actualizaciones para mitigar vulnerabilidades.



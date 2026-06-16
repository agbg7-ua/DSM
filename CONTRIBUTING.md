# Contributing

## Guidelines
- Sigue la estructura del proyecto Clean DDD descrita en `solution.plan`.
- Target framework: `.NET 8` (TFM `net8.0`).
- Usa `long` para las propiedades `Id` de las entidades de dominio. No uses Guid para Ids en las entidades principales.
- Todos los repositorios y `IUnitOfWork` expuestos en `ApplicationCore` son síncronos.
- Los CENs (Componentes Entidad de Negocio) deben ser stateless y recibir únicamente los campos obligatorios en sus métodos `crear` y `modificar`.
- CPs (Casos de Proceso) deben orquestar repositorios y UoW y ser transaccionales.
- Evita dependencias de infraestructura en `ApplicationCore` (no referenciar EF/NHibernate desde las entidades o CENs).

## Coding Standards
- Respeta `.editorconfig` del repositorio. Identación: 4 espacios en C#.
- Nombres de clases en PascalCase, interfaces con prefijo `I` en PascalCase (p. ej. `IClienteRepository`).
- Carpetas: `ApplicationCore/Domain/EN`, `ApplicationCore/Domain/Enums`, `ApplicationCore/Domain/Repositories`, `ApplicationCore/Domain/CEN`, `ApplicationCore/Domain/CP`, `Infrastructure/NHibernate/Mappings`, `InitializeDb`.
- Los mappings NHibernate deben ser `.hbm.xml` y situarse en `Infrastructure/NHibernate/Mappings/`.

## Repositorios y Mappings
- Las implementaciones de `Infrastructure` deben usar NHibernate con mappings XML `.hbm.xml`.
- Evitar implementaciones InMemory por defecto en `Infrastructure`.
- `NHibernateHelper` debe buscar `NHibernate.cfg.xml` en `AppContext.BaseDirectory` y reescribir rutas relativas hacia rutas absolutas si es necesario.

## InitializeDb
- `InitializeDb` es responsable de crear el MDF LocalDB y ejecutar `SchemaExport` para crear el esquema.
- No ejecutar seed automático; proveer hooks claros para seed mediante CEN y CP.

## Pull Requests
- Un PR debe incluir descripción clara, issue relacionado y pasos para probar.
- Ejecuta `dotnet build` y `dotnet test` antes de crear PR.

## Commits
- Mensajes en inglés y estilo imperativo. Ejemplo: "Add Cliente entity and repository interface".

## Tests
- Añadir tests en un proyecto separado `Tests` con cobertura razonable para CENs y CPs.

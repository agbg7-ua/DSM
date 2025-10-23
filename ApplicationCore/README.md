# ApplicationCore (scaffold)

Este proyecto contiene las entidades de dominio (POCO), enums y las interfaces de repositorio para la arquitectura Clean + DDD.

Estructura inicial:
- Domain/EN: Entidades (POCO)
- Domain/Enums: Enums del dominio
- Domain/Repositories: Interfaces de repositorio e IUnitOfWork

Siguientes pasos:
- Implementar el proyecto .csproj para `ApplicationCore` (TargetFramework net8.0)
- Implementar Infrastructure (NHibernate mappings y repositorios concretos)
- Implementar InitializeDb (console app) para crear la BD y ejecutar SchemaExport

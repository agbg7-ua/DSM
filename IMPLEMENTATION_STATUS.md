# DSM - Implementación Clean Architecture

## Componentes implementados
1. ApplicationCore
   - Entidades POCO con relaciones virtuales para NHibernate
   - Enums del dominio
   - Interfaces de repositorio
   - CENs básicos (UsuarioCEN, ProductoCEN)
   - CP ejemplo (PedidoCP)

2. Infrastructure
   - Mappings NHibernate (.hbm.xml) para todas las entidades
   - Repositorios NHibernate concretos
   - UnitOfWork con manejo de transacciones
   - Configuración NHibernate (LocalDB)
   - Registro de servicios para DI

3. InitializeDb
   - Creación de base de datos y schema
   - Seed básico (usuario admin y producto demo)
   - Uso de DI y CENs

## Cómo ejecutar
1. Asegúrate de tener instalado:
   - .NET 8.0 SDK
   - SQL Server LocalDB

2. Ejecuta estos comandos:
```powershell
dotnet restore
dotnet build
cd InitializeDb
dotnet run
```

## Siguientes pasos recomendados
1. Implementar más CENs siguiendo el patrón de UsuarioCEN y ProductoCEN
2. Añadir validaciones de negocio en los CENs
3. Implementar más CPs para operaciones complejas
4. Agregar tests unitarios y de integración
5. Configurar logging y manejo de errores
6. Implementar capa de presentación (API/MVC)

## Notas importantes
- La base de datos se crea en LocalDB
- Los mappings usan generador HiLo para IDs
- Las relaciones son lazy loading (virtual) para mejor rendimiento
- Se usa UoW para transacciones explícitas
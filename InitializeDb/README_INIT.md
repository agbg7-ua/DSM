InitializeDb scaffold

Implementación pendiente:
- Añadir NHibernate package y configurar `NHibernate.cfg.xml`
- Registrar repositorios concretos y UnitOfWork
- Implementar SchemaExport para crear tablas en LocalDB

Comandos sugeridos (PowerShell):

```powershell
Push-Location .\InitializeDb; dotnet run --project .; Pop-Location
```

# Entrega inicial — DSM Clean Architecture Scaffold

Generé automáticamente un scaffold inicial de arquitectura Clean + DDD a partir de `modelo_dominio.puml` y las instrucciones en `solution.plan.md`.

## Archivos generados clave
- `domain.model.json` — JSON canonical del modelo de dominio (entidades, enums, relaciones).
- `ApplicationCore/` — Proyecto con POCOs de dominio en `Domain/EN`, `Domain/Enums` y las interfaces de repositorio en `Domain/Repositories`. Incluye CEN/CP scaffolds.
- `Infrastructure/` — Proyecto con mappings NHibernate (`Infrastructure/NHibernate/Mappings/*.hbm.xml`), `nhibernate.cfg.xml`, helper de NHibernate y repositorios básicos.
- `InitializeDb/` — Aplicación consola scaffold que intenta ejecutar `SchemaExport` usando NHibernate.
- `DSM.sln` — Solución que referencia los proyectos `ApplicationCore` e `InitializeDb`.

## Qué está completo ahora
- domain.model.json: representación fiel del `modelo_dominio.puml`.
- Estructura de proyectos y archivos principales generados.
- Clases POCO generadas y enums.
- Interfaces de repositorio y UoW definidas.
- Mappings NHibernate básicos (.hbm.xml) creados.
- Repositorios NHibernate simples implementados.
- InitializeDb scaffold que llama a SchemaExport.

## Limitaciones actuales y notas importantes
- En este entorno `dotnet` no está disponible (ver intento de `dotnet build` fallido). Debes ejecutar `dotnet build` y `dotnet run` localmente en tu máquina con .NET SDK instalado.
- NHibernate está referenciado en `Infrastructure.csproj` pero es necesario ajustar versiones NuGet y restaurar paquetes en tu entorno local.
- La cadena de conexión en `Infrastructure/NHibernate/nhibernate.cfg.xml` está configurada para LocalDB. Asegúrate de que LocalDB esté instalado o cambia la cadena a tu instancia de SQL Server.
- Los mapeos y nombres de Ids conservan los nombres del PUML (IdUsuario, IdProducto...); puedes normalizarlos a `Id` si prefieres.

## Pasos sugeridos para ejecutar localmente (PowerShell en Windows)

1. Asegúrate de tener instalado:
   - .NET 8 SDK (dotnet)
   - SQL Server LocalDB (opcional si usas la cadena LocalDB en nhibernate.cfg.xml)

2. Restaurar paquetes y compilar:

```powershell
Push-Location .\DSM
dotnet restore
dotnet build DSM.sln
Pop-Location
```

3. Ejecutar el Initializer para crear la base de datos y esquema:

```powershell
Push-Location .\DSM\InitializeDb
dotnet run --project .\InitializeDb.csproj
Pop-Location
```

Si tienes problemas relacionados con la ruta de los mappings, copia `Infrastructure\NHibernate\nhibernate.cfg.xml` y la carpeta `Infrastructure\NHibernate\Mappings` al directorio de salida del ejecutable (o ajusta las rutas en el archivo `nhibernate.cfg.xml`).

## Siguientes pasos recomendados (priorizados)
1. Ajustar mapeos y nombres de Id a `Id` si deseas consistencia.
2. Implementar `IUnitOfWork` concreto (NHibernate) que maneje transacciones y `SaveChanges()`.
3. Implementar CENs y CPs adicionales, con tests unitarios/integración.
4. Añadir proyecto de tests y adaptadores InMemory para desarrollo.
5. Revisar seguridad (hash de contraseñas), validaciones y DTOs para la capa de presentación.

---

Si quieres, puedo continuar y:
- Implementar `IUnitOfWork` NHibernate y completar el `InitializeDb` para crear el MDF en `InitializeDb/Data`.
- Normalizar nombres de Id a `Id` y actualizar mappings.
- Generar tests unitarios para CENs y CPs.

Dime qué prefieres que haga a continuación y lo implemento.

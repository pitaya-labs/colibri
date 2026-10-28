# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

- No incluyas `Co-Authored-By: Claude ...` ni la línea `Claude-Session: ...`
  en los mensajes de commit de este repo.

## Contexto
Backend .NET 10 (ASP.NET Core minimal APIs) del expediente clínico Colibri. Forma parte del monorepo: los requisitos y restricciones de dominio (inmutabilidad de documentos firmados, rol Administrativo sin acceso clínico, multi-tenencia por médico, etc.) están en `../CLAUDE.md` y `../specs/REQUIREMENTS.md` y aplican también aquí.

El proyecto salió de la plantilla `dotnet new vsa-api` (Vertical Slice Architecture). El `README.md` es el de esa plantilla: sus rutas dicen `colibri_api` pero los directorios reales son `colibri-api` (el namespace raíz sigue siendo `colibri_api`). `Features/` hoy contiene solo la capa `Domain/` (entidad + `IEntityTypeConfiguration`) de la Ola 1 (identidad y acceso: `Tenants`, `Users`, `Doctors`, `Clinics`, `Assignments`); aún no hay casos de uso ni `Endpoints`. Dominio, tablas y migración están en inglés. Los tests de esa ola están en `tests/colibri-api.Tests/Features/IdentityAndAccess/`.

## Comandos (desde `colibri-api/`)
```bash
dotnet run --project src/colibri-api        # API local (SQLite colibri_api.db)
dotnet test                                  # arquitectura + integración
dotnet test --filter "FullyQualifiedName~ArchitectureTests"   # una clase / un test
dotnet tool restore                          # instala dotnet-ef (versión fijada en dotnet-tools.json)
dotnet ef migrations add <Nombre> --project src/colibri-api --output-dir Common/Persistence/Migrations
```
- Las migraciones pendientes se aplican solas al arrancar, **solo en Development** (`ApplyPendingMigrations`).
- En Development/Staging la documentación está en `/openapi/v1.json` y Scalar.
- Versiones de paquetes centralizadas en `Directory.Packages.props`; `Directory.Build.props` define TFM, nullable e `IncludeSampleFeature`/`IncludeComments` (restos de la plantilla).

## Arquitectura (Vertical Slice)
- `Program.cs` solo compone: cada preocupación transversal vive en `Common/<Tema>/` con su `Add...()`/`Use...()`/`Map...()` (errores RFC 9457, CORS, health checks, rate limiting, OpenAPI, JWT, Serilog + correlation id, EF Core).
- **Todo se descubre por convención** (reflexión/Scrutor), sin registro central:
  - Endpoints: clase `*Endpoints` en `Features/<Feature>/` que implementa `IFeatureEndpoints` (`static Map`) → `MapFeatureEndpoints()`.
  - Policies de auth por slice: clase que implementa `ISlicePolicies` → `ConfigureSlicePolicies`.
  - Casos de uso: clases públicas cuyo nombre termina en `Handler` se registran scoped (`Composition/UseCaseRegistration.cs`). Validators FluentValidation por escaneo de assembly; filtro `ValidationFilter` los ejecuta antes del handler.
  - Configuración EF: `ApplyConfigurationsFromAssembly`, así que los `IEntityTypeConfiguration` de cada slice se toman solos.
- Un caso de uso = un archivo en `Features/<Feature>/` con `Request`, `Response`, `Validator` y `Handler` juntos; `Endpoints.cs` solo enruta.
- Comunicación entre slices: eventos in-process (`IEventDispatcher` / `IEventHandler<>`), nunca llamando casos de uso de otro slice. Los handlers de eventos viven en `Features/`, no en `Common/`.
- Persistencia: `AppDbContext.TrySaveChangesAsync` devuelve `SaveResult` (`ConcurrencyConflict`, `DuplicateKey`) en vez de lanzar; las entidades con `IHasRowVersion` reciben un nuevo `RowVersion` en cada UPDATE. Hoy el proveedor es SQLite (la detección de unique violation es específica de SQLite).
- Paginación: `IPagedRequest` + `PagedResponse<T>` + `PaginationExtensions`.

## Reglas impuestas por tests (`tests/colibri-api.Tests/Architecture/`)
`ArchitectureTests` (NetArchTest) rompe el build si:
- un slice usa casos de uso de otro slice (solo `Domain` y `Contracts` son públicos entre slices);
- `Common` depende de casos de uso de un slice (sí puede conocer el `Domain`);
- una clase `*Endpoints` en Features no implementa `IFeatureEndpoints`;
- un `IEventHandler<>` está fuera de `Features`;
- un tipo público anidado en un caso de uso no se llama exactamente `Request`, `Response` o `Validator` (esos nombres definen el contrato OpenAPI/TypeScript; `OpenApiRegistration` compone el schema id como `<Owner><Tipo>`);
- un `*Handler` público no queda registrado por `AddUseCaseHandlers`.

## Tests
- Integración: `ApiFactory : WebApplicationFactory<Program>` reemplaza el `AppDbContext` por SQLite en memoria (`TestDb`); `TestJwt` genera tokens para endpoints autenticados. `Program` es `public partial` para esto.

# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Build & Run Commands

```bash
# Build
dotnet build SellerInventory.sln          # Full solution
dotnet build src/Api/Api.csproj           # API only
dotnet build src/Client/BlazorWeb/BlazorWeb.csproj  # Web only

# Run with Docker (recommended)
docker compose up                          # Dev: Postgres:5432, API:5001, Web:5174

# Run locally (watch mode)
dotnet watch --project src/Api/Api.csproj                    # API → http://localhost:5000
dotnet watch --project src/Client/BlazorWeb/BlazorWeb.csproj # Web → http://localhost:5173

# Deploy to GCP
./deploy-gcloud.sh deploy-all              # Full deployment to Cloud Run
```

## Architecture

**.NET 9 Clean Architecture** with multi-tenant support (stores as tenant root).

```
Domain → Application → Infrastructure → Api
                                      → Client/BlazorWeb
```

### Layer Dependencies
- **Domain** (`src/Domain`): Entities, enums, interfaces. NO external dependencies.
- **Application** (`src/Application`): Services, DTOs, FluentValidation validators. Depends on Domain only.
- **Infrastructure** (`src/Infrastructure`): EF Core (PostgreSQL), repositories, JWT auth, storage. Implements Application interfaces.
- **Api** (`src/Api`): REST controllers. Injects Application services only.
- **BlazorWeb** (`src/Client/BlazorWeb`): Blazor WebAssembly with MudBlazor. Calls API via HttpClient.

### Multi-Tenancy
- `Store` is the tenant root - most entities implement `ITenantEntity` with `StoreId`
- `TenantMiddleware` extracts `StoreId` from JWT claims per request
- `ITenantContext` provides current tenant info to services
- Roles: `SystemAdmin` (no store), `Manager`, `Staff`

### Key Patterns

**Repository + UnitOfWork**:
```csharp
var users = await _unitOfWork.Users.FindAsync(u => u.Username == username, ct);
await _unitOfWork.SaveChangesAsync(ct);
```

**Service Registration**:
- `services.AddApplication()` in `src/Application/DependencyInjection.cs`
- `services.AddInfrastructure(config)` in `src/Infrastructure/DependencyInjection.cs`

**Validation**: FluentValidation validators auto-discovered from `src/Application/Validators/`

**Storage**: `IStorageService` with `LocalStorageService` (dev) or `GoogleCloudStorageService` (prod)

## Adding a Feature

1. Entity in `src/Domain/Entities/` (inherit `BaseEntity`, implement `ITenantEntity` if tenant-scoped)
2. DTOs in `src/Application/DTOs/{Feature}/` (Create/Update/Response pattern)
3. Validator in `src/Application/Validators/`
4. Service interface in `src/Application/Interfaces/`, implementation in `src/Application/Services/`
5. Register in `src/Application/DependencyInjection.cs`
6. Controller in `src/Api/Controllers/` (route: `api/v1/[controller]`)
7. Blazor page in `src/Client/BlazorWeb/Pages/`

## Critical Files

| Purpose | Path |
|---------|------|
| DB Context & Migrations | `src/Infrastructure/Data/ApplicationDbContext.cs` |
| Repository Pattern | `src/Infrastructure/Repositories/UnitOfWork.cs` |
| JWT & Auth Setup | `src/Infrastructure/DependencyInjection.cs` |
| Tenant Extraction | `src/Api/Middleware/TenantMiddleware.cs` |
| API Config | `src/Api/appsettings.json`, `appsettings.Development.json` |
| Client API URL | `src/Client/BlazorWeb/wwwroot/appsettings.json` |

## Conventions

- All async methods take `CancellationToken` as final parameter
- Services throw `InvalidOperationException` (400), `UnauthorizedAccessException` (401), `KeyNotFoundException` (404)
- API routes: `api/v1/[controller]`
- Entities: Guid `Id`, `CreatedAt`, `UpdatedAt` from `BaseEntity`
- No EF DbContext in controllers - always use services

## Environment

- **Database**: PostgreSQL (NEON for prod, Docker for dev)
- **Storage**: Local (`wwwroot/uploads/`) or Google Cloud Storage
- **Auth**: JWT HS256, 24-hour expiry
- **UI**: MudBlazor 7.6.0

## Default Credentials (Dev)

- `sysadmin` / `SysAdmin@123` (SystemAdmin)
- `manager` / `Manager@123` (Manager)
- `staff` / `Staff@123` (Staff)

# SellerInventory - AI Coding Agent Instructions

## Architecture Overview

This is a **.NET 8+ Clean Architecture** project with separate API and Blazor WebAssembly layers:

- **Domain** ([src/Domain](src/Domain)): Core business entities (`User`, `Product`, `Category`, `Order`, `OrderItem`) with base timestamps
- **Application** ([src/Application](src/Application)): Business logic via Services + FluentValidation validators + DTOs
- **Infrastructure** ([src/Infrastructure](src/Infrastructure)): Database (SQLite), UnitOfWork pattern, Repositories, JWT authentication
- **Api** ([src/Api](src/Api)): ASP.NET Core REST controllers (v1 routes) with Swagger docs
- **BlazorWeb** ([src/Client/BlazorWeb](src/Client/BlazorWeb)): WebAssembly UI with DevExpress components

Key principle: **Dependency flows inward** - controllers inject services, services use repositories via UnitOfWork.

## Data Flow & Key Patterns

### Service Registration (Dependency Injection)
- Add services in [src/Application/DependencyInjection.cs](src/Application/DependencyInjection.cs) with `services.AddScoped<IInterface, Implementation>()`
- Infrastructure setup in [src/Infrastructure/DependencyInjection.cs](src/Infrastructure/DependencyInjection.cs) handles DB + JWT + Auth
- Client-side equivalents in [src/Client/BlazorWeb/Program.cs](src/Client/BlazorWeb/Program.cs)

### UnitOfWork & Repository Pattern
Access repositories through `IUnitOfWork` (see [src/Infrastructure/Repositories/UnitOfWork.cs](src/Infrastructure/Repositories/UnitOfWork.cs)):
```csharp
var users = await _unitOfWork.Users.FindAsync(u => u.Username == request.Username, ct);
await _unitOfWork.SaveChangesAsync(ct);
```
Repositories include: `Users`, `Categories`, `Products`, `Orders`, `OrderItems`

### Validation
Use FluentValidation validators (auto-discovered in [src/Application/DependencyInjection.cs](src/Application/DependencyInjection.cs)):
- Create validators in [src/Application/Validators](src/Application/Validators) inheriting `AbstractValidator<T>`
- Example: [src/Application/Validators/CreateProductValidator.cs](src/Application/Validators/CreateProductValidator.cs)
- Validators automatically run on incoming DTOs

### Authentication & Authorization
- JWT tokens generated in `ITokenService` during login (24-hour expiry)
- Roles stored in `User.Role` enum → used for `[Authorize(Roles = "...")]` attributes
- Client-side: `CustomAuthStateProvider` stores JWT in local storage
- API CORS allows `http://localhost:5173` and `https://localhost:5173` (Blazor dev/production)

## Development Workflows

### Building
- **All projects**: `dotnet build SellerInventory.sln`
- **API only**: `dotnet build src/Api/Api.csproj`
- **Web only**: `dotnet build src/Client/BlazorWeb/BlazorWeb.csproj`

Or use VS Code tasks:
- `build-all` (full solution)
- `build-api` (backend)
- `build-web` (frontend)

### Running (Watch Mode)
Use background VS Code tasks:
- `watch-api`: Runs API with hot reload → http://localhost:5000
- `watch-web`: Runs WebAssembly → http://localhost:5173

### Docker Deployment
Defined in [docker-compose.yml](docker-compose.yml):
- API container: `:5000`
- Web container: `:5173`
- SQLite volume mount: `api-data`
- Set `JWT_SECRET_KEY` environment variable for production

## Project-Specific Conventions

### Entity IDs
All entities inherit from [src/Domain/Entities/BaseEntity.cs](src/Domain/Entities/BaseEntity.cs):
- `Id`: Guid (auto-generated)
- `CreatedAt`, `UpdatedAt`: DateTime UTC

### DTOs & Folders
DTOs organized by feature in [src/Application/DTOs](src/Application/DTOs):
- `Auth/`: Login, Register, Token responses
- `User/`, `Product/`, `Category/`, `Order/`, `Report/`: Feature-specific DTOs
- Use separate Create/Update/Response DTOs

### API Routes
Controllers use route prefix `api/v1/[controller]`:
```csharp
[Route("api/v1/[controller]")]
[HttpPost("login")] // → POST /api/v1/auth/login
```

### Error Handling
Controllers catch business logic exceptions and return appropriate HTTP responses:
- `UnauthorizedAccessException` → 401 Unauthorized
- `InvalidOperationException` → 400 Bad Request

### Cancellation Tokens
All async methods accept `CancellationToken cancellationToken = default` as final parameter for graceful shutdown.

## Critical Files Reference

| Purpose | File |
|---------|------|
| Solution entry point | [SellerInventory.sln](SellerInventory.sln) |
| API bootstrap & CORS | [src/Api/Program.cs](src/Api/Program.cs) |
| Service registration | [src/Application/DependencyInjection.cs](src/Application/DependencyInjection.cs) |
| Database & Auth setup | [src/Infrastructure/DependencyInjection.cs](src/Infrastructure/DependencyInjection.cs) |
| Data access pattern | [src/Infrastructure/Repositories/UnitOfWork.cs](src/Infrastructure/Repositories/UnitOfWork.cs) |
| Blazor bootstrap | [src/Client/BlazorWeb/Program.cs](src/Client/BlazorWeb/Program.cs) |
| Auth provider (client) | [src/Client/BlazorWeb/Services/](src/Client/BlazorWeb/Services) (CustomAuthStateProvider) |
| UI Pages | [src/Client/BlazorWeb/Pages](src/Client/BlazorWeb/Pages) (Categories, Products, Orders, Reports, Users, Login, POS) |

## Adding New Features

1. **Create Domain Entity**: Add to [src/Domain/Entities](src/Domain/Entities) inheriting `BaseEntity`
2. **Create DTOs**: Add to [src/Application/DTOs](src/Application/DTOs)
3. **Create Validator**: Add to [src/Application/Validators](src/Application/Validators)
4. **Create Service**: Add to [src/Application/Services](src/Application/Services), implement interface in `IApplicationMarker` namespace
5. **Register Service**: Add to [src/Application/DependencyInjection.cs](src/Application/DependencyInjection.cs) with `.AddScoped<IService, Service>()`
6. **Create Controller**: Add to [src/Api/Controllers](src/Api/Controllers), inject service via constructor
7. **Create Blazor Page**: Add to [src/Client/BlazorWeb/Pages](src/Client/BlazorWeb/Pages), inject service via `@inject`

## Quick Diagnostics

- **Port conflicts**: API defaults to `:5000`, Web to `:5173` → check `docker-compose.yml` or launch settings
- **JWT failures**: Verify `JwtSettings` in `appsettings.json` or environment variables
- **CORS issues**: Check allowed origins in [src/Api/Program.cs](src/Api/Program.cs) line ~40
- **DB path**: SQLite uses `sellerinventory.db` in current directory or Docker volume

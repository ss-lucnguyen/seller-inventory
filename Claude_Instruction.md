# PROJECT INSTRUCTION – .NET 8 Blazor POS (KIOS-like)

## 1. Role & Responsibility

You are a **Senior .NET Architect & Full-stack Engineer**.

Your task is to:
- Design
- Implement
- Refactor
- Review

a **KIOS-like Sales Management Application** (Web + Mobile-ready) using **.NET 8 + Blazor**, focusing on **clean architecture, maintainability, and free/open-source tooling**.

You must always:
- Write **production-quality code**
- Follow **Clean Architecture & SOLID**
- Avoid paid / proprietary libraries
- Explain decisions briefly when needed
- Generate code that compiles and runs locally

---

## 2. Tech Stack (STRICT)

### Backend
- **.NET 8**
- **ASP.NET Core Web API**
- **Entity Framework Core**
- Database (choose ONE, prioritize simplicity):
  - SQLite (preferred – fully free, easy)
  - OR SQL Server Express / LocalDB
- Authentication: **JWT**
- No cloud dependency (local-first)

### Frontend
- **Blazor WebAssembly**
- UI Library:
  - **DevExtreme Blazor (Community / Free components only)**
  - If DevExtreme feature is paid → FALL BACK to **pure Blazor + CSS**
- No JavaScript frameworks (React / Vue forbidden)

### Mobile (Future-ready)
- Code must be reusable for **Blazor Hybrid (.NET MAUI)** later
- No platform-specific logic inside core business code

---

## 3. Development Environment

- IDE: **Visual Studio Code**
- OS: macOS / Windows / Linux compatible
- Containerization: **Docker & Docker Compose**
- All services must run via:
  ```bash
  docker compose up
  ```

---

## 4. Project Structure (MANDATORY)

Use **Clean Architecture**:

```
src/
 ├─ Api/
 │   ├─ Controllers/
 │   ├─ Program.cs
 │   └─ appsettings.json
 │
 ├─ Application/
 │   ├─ Interfaces/
 │   ├─ DTOs/
 │   ├─ Services/
 │   └─ Validators/
 │
 ├─ Domain/
 │   ├─ Entities/
 │   ├─ Enums/
 │   └─ ValueObjects/
 │
 ├─ Infrastructure/
 │   ├─ Data/
 │   ├─ Repositories/
 │   ├─ Auth/
 │   └─ Migrations/
 │
 ├─ Client/
 │   └─ BlazorWeb/
 │       ├─ Pages/
 │       ├─ Components/
 │       ├─ Services/
 │       └─ wwwroot/
 │
 └─ Shared/
     └─ Contracts/
```

Rules:
- **Domain has NO dependency** on any other layer
- UI NEVER talks directly to Infrastructure
- API only talks to Application layer

---

## 5. Functional Scope (MVP)

### Authentication & Authorization
- Login / Logout
- JWT Token
- Roles:
  - Admin
  - Staff

### Product Management
- CRUD Product
- Category
- Price
- Stock quantity

### Order / Sales
- Create order
- Add / remove items
- Calculate total
- Persist order

### Reporting
- Daily sales
- Total revenue
- Orders count

---

## 6. Database Rules

- Use **EF Core Code First**
- All entities must have:
  - Id (Guid)
  - CreatedAt
  - UpdatedAt
- Use **Repository pattern**
- No EF DbContext usage in Controllers

---

## 7. Coding Rules

- Language: **C# 12**
- Use `record` for DTOs
- Use `async / await` everywhere
- No static services
- No Service Locator
- No God classes
- Validate input using **FluentValidation**

---

## 8. API Rules

- RESTful endpoints
- Versioned API: `/api/v1/...`
- HTTP status codes correctly used
- No business logic inside controllers

Example:
```csharp
[HttpPost]
public async Task<IActionResult> Create(CreateOrderDto dto)
{
    var result = await _orderService.CreateAsync(dto);
    return Ok(result);
}
```

---

## 9. Blazor Rules

- Use **Component-based design**
- No logic inside `.razor` view when possible
- Business logic goes into Services
- API calls via typed HttpClient

---

## 10. Docker Rules

- One container for API
- One container for Blazor Web
- One container for Database
- Use `docker-compose.yml`
- No secrets hardcoded

---

## 11. What You SHOULD Generate

Claude Code should be able to:
- Generate full project skeleton
- Add new feature incrementally
- Refactor to better architecture
- Write unit tests when asked
- Explain architecture decisions shortly

---

## 12. What You MUST NOT Do

- Do NOT use paid libraries
- Do NOT skip layers
- Do NOT mix UI and business logic
- Do NOT introduce unnecessary complexity
- Do NOT generate incomplete code

---

## 13. Output Expectations

When generating code:
- Always specify file path
- Always ensure code builds
- Prefer clarity over cleverness

---

## END OF INSTRUCTION

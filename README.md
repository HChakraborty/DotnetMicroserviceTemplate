# .NET Microservice Template (Incomplete)

A reusable starter template for building .NET microservices using Clean Architecture.

This project provides a structured base for creating new services with consistent layering and patterns. It is intentionally minimal so developers can understand the structure without being overwhelmed by production-level complexity. It serves as a practical starting point that can be extended depending on business requirements.

---

## Features

* Clean Architecture (API, Application, Domain, Infrastructure)
* Repository Pattern
* EF Core integration
* Global exception handling middleware
* CancellationToken support
* Dependency Injection configuration
* JWT Authentication & Role-based Authorization
* Policy-based access control
* Health Checks
* Rate Liming
* Unit Tests

---

## Architecture Overview

This template follows **Clean Architecture** to enforce separation of concerns and long-term maintainability.

### Layer Responsibilities

```
API Layer            → Controllers, Middleware, Request handling
Application Layer    → Business logic, Services, DTOs, Interfaces
Domain Layer         → Entities, Enums, Core rules
Infrastructure Layer → Database, Repositories, External integrations
```

Dependencies always point inward toward the Domain layer.
This prevents framework or database concerns from leaking into business logic.

### Why this structure?

* Makes the system easier to test
* Allows infrastructure changes without affecting business logic
* Prevents controllers from becoming complex
* Supports long-term maintainability in microservices

---

## Authentication & Authorization

This template uses a **separate authentication service** (`SampleAuthService`) that issues JWT access tokens.

The resource service validates tokens and enforces authorization policies rather than managing identity itself.

### Authentication Flow

```
Client → AuthService → JWT Token → Protected Service
```

1. User registers or logs in via **SampleAuthService**
2. AuthService generates a JWT access token
3. Client includes the token in requests:

```
Authorization: Bearer <access_token>
```

4. Resource services validate the token and enforce policies

### Why a separate Auth Service?

* Centralized identity management
* Avoids duplicating security logic
* Easier to scale authentication independently
* Cleaner separation of concerns

---

## JWT Configuration

Each service contains a matching configuration:

```json
"Jwt": {
  "Key": "MyVeryStrongDevelopmentKey_ChangeInProduction_123456",
  "Issuer": "SampleAuthService",
  "Audience": "SampleServices",
  "ExpireMinutes": 60
}
```

In production, these values should be stored in environment variables or a secure secret store instead of source code.

---

## Roles

Supported roles:

* `ReadUser`
* `WriteUser`
* `Admin`

Roles are embedded as claims inside the JWT and enforced via authorization policies.

---

## Authorization Policies

| Policy      | Allowed Roles              | Purpose                                 |
| ----------- | -------------------------- | --------------------------------------- |
| ReadPolicy  | ReadUser, WriteUser, Admin | Read-only operations                    |
| WritePolicy | WriteUser, Admin           | Create / update operations              |
| AdminPolicy | Admin                      | Destructive / administrative operations |

Example:

```csharp
[Authorize(Policy = "ReadPolicy")]
[HttpGet]
public async Task<IActionResult> GetAll()
```

Policies provide fine-grained access control beyond simple authentication.

---

## AuthService Responsibilities

The authentication service handles:

* User registration
* Token generation
* Password hashing (BCrypt)
* Password reset (simplified placeholder)
* User profile retrieval

Advanced features such as refresh tokens, email verification, external identity providers, and account recovery are intentionally excluded to keep the template focused and easy to extend.

---

## Health Checks

Health checks allow infrastructure systems (Docker, Kubernetes, load balancers) to determine whether the service is running and ready.

Endpoint:

```
GET /health
```

### Purpose

* Detect container failures
* Support automated restarts
* Enable readiness checks during deployment
* Monitor database connectivity

The endpoint does not require authentication because it is intended for infrastructure monitoring.

---

## Rate Limiting

Rate limiting protects services from abuse and excessive traffic.

### Purpose

* Prevent brute-force attacks
* Protect system resources
* Ensure fair usage
* Improve stability

Limits can be adjusted depending on business requirements and traffic patterns. Sensitive endpoints (login, token generation) should use stricter limits in production.

---

## Request Flow

A typical request to a protected endpoint:

```
Client Request
 → Rate Limiting
 → Authentication
 → Authorization (Policy)
 → Controller
 → Service
 → Repository
 → Database
```

Security checks occur before business logic execution.

---

## Sample Service — CRUD Reference Implementation

The template includes a **SampleService** that demonstrates a complete CRUD flow using Clean Architecture and the Repository Pattern.

This service is intentionally simple and serves as a reference implementation for building real features.

### Purpose

The sample shows how requests travel through layers:

```
Controller → Application Service → Repository → EF Core → Database
```

This helps developers understand how to structure new features consistently.

---

### Supported Operations

The sample implements full CRUD operations for `SampleEntity`:

| Operation  | Method            | Description                |
| ---------- | ----------------- | -------------------------- |
| Create     | `AddAsync`        | Adds a new entity          |
| Read All   | `GetAllAsync`     | Retrieves all records      |
| Read By Id | `GetByIdAsync`    | Retrieves a single record  |
| Update     | `UpdateAsync`     | Updates an existing record |
| Delete     | `DeleteByIdAsync` | Removes a record           |

---

### Repository Implementation (Infrastructure Layer)

The repository uses Entity Framework Core and demonstrates:

* Asynchronous database operations
* CancellationToken support
* SaveChanges per operation (simplified unit-of-work)
* Null-safe delete handling

Example responsibilities:

* Query database entities
* Persist changes
* Isolate EF Core from Application layer

This keeps business logic independent of database technology.

---

### Why CancellationToken is Included

Cancellation tokens are passed to EF Core operations to allow:

* Request aborts (client disconnects)
* Graceful shutdown handling
* Preventing unnecessary database work

This is especially important in microservices where scalability and resource management matter.

---

### Why the Sample is Important

The sample service demonstrates best practices:

* Thin controllers
* Business logic in services
* Data access in repositories
* DTO ↔ Entity mapping
* Role-based authorization usage

Developers can copy this structure when adding new features.

---

## How to Add a New Feature Using This Template

This template is designed to make feature development predictable and consistent across services.
Follow these steps when implementing new functionality.

The goal is to maintain Clean Architecture boundaries and keep responsibilities separated.

---

### Step 1 — Define the Domain Model

Create a new entity inside the **Domain layer**.

```
ServiceName.Domain
└─ Entities
   └─ Product.cs
```

Example:

```csharp
public class Product
{
    public Guid Id { get; set; }
    public string Name { get; set; } = default!;
    public decimal Price { get; set; }
}
```

Why this step first:

The Domain layer represents the core business model and should not depend on infrastructure or frameworks.

---

### Step 2 — Create DTOs

Add request/response models in the **Application layer**.

```
ServiceName.Application
└─ DTOs
   └─ ProductDTO.cs
```

DTOs prevent exposing internal domain models directly to clients and allow validation or shaping of API responses.

---

### Step 3 — Define Service Interface

Create an interface describing business operations.

```
ServiceName.Application
└─ Interfaces
   └─ IProductService.cs
```

Example:

```csharp
public interface IProductService
{
    Task<IReadOnlyList<ProductDTO>> GetAllAsync();
    Task<ProductDTO?> GetByIdAsync(Guid id);
    Task AddAsync(ProductDTO dto);
    Task UpdateAsync(ProductDTO dto);
    Task DeleteByIdAsync(Guid id);
}
```

Why interfaces:

They enable testing via mocks and keep controllers independent from concrete implementations.

---

### Step 4 — Implement Application Service

Add business logic in the **Application layer**.

```
ServiceName.Application
└─ Services
   └─ ProductService.cs
```

Responsibilities:

* Validation
* Mapping between DTOs and entities
* Coordinating repository calls
* Enforcing business rules

---

### Step 5 — Create Repository

Implement data access in the **Infrastructure layer**.

```
ServiceName.Infrastructure
└─ Repositories
   └─ ProductRepository.cs
```

Repositories isolate EF Core from business logic and allow swapping databases without changing services.

---

### Step 6 — Register Dependencies

Add service and repository mappings in dependency injection configuration.

Example:

```csharp
services.AddScoped<IProductService, ProductService>();
services.AddScoped<IRepository<Product>, ProductRepository>();
```

---

### Step 7 — Add Controller Endpoints

Expose the feature through the **API layer**.

```
ServiceName.Api
└─ Controllers
   └─ ProductController.cs
```

Controllers should remain thin and delegate logic to services.

Example:

```csharp
[ApiController]
[Route("api/v1/products")]
public class ProductController : ControllerBase
{
    private readonly IProductService _service;

    public ProductController(IProductService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
        => Ok(await _service.GetAllAsync());
}
```

---

### Step 8 — Add Authorization (Optional)

Apply policies if the endpoint should be protected.

Example:

```csharp
[Authorize(Policy = "WritePolicy")]
```

Use policies instead of role checks inside controllers to keep authorization centralized.

---

### Step 9 — Add Unit Tests

Add tests for:

* Controller behavior (mock service)
* Service logic (mock repository)
* Repository operations (in-memory database)

Testing each layer independently ensures reliability without requiring full integration tests.

---

## Project Structure

### Resource Service

```text
ServiceName
└─src/
  ├─ ServiceName.Api
  │ ├─ Controllers
  │ ├─ Middlewares
  │ └─ Extensions
  ├─ ServiceName.Application
  │ ├─ DTOs
  │ ├─ Interfaces
  │ └─ Services
  ├─ ServiceName.Domain
  │ ├─ Enums
  │ └─ Entities
  ├─ ServiceName.Infrastructure
  │ ├─ Persistence
  │ ├─ Repositories
  │ └─ Migrations
tests/
  ├─ ServiceName.UnitTests
  │ ├─ Controllers
  │ ├─ Repository
  │ └─ Services
deployment/
```

### Authentication Service

```text
SampleAuthService
└─src/
  ├─ AuthService.Api
  │ ├─ Controllers
  │ ├─ Middlewares
  │ └─ Extensions
  ├─ AuthService.Application
  │ ├─ DTOs
  │ ├─ Interfaces
  │ └─ Services
  ├─ AuthService.Domain
  │ ├─ Enums
  │ └─ Entities
  ├─ AuthService.Infrastructure
  │ ├─ Persistence
  │ ├─ Repositories
  │ ├─ Security
  │ └─ Migrations
tests/
  ├─ AuthService.UnitTests
  │ ├─ Controllers
  │ ├─ Repository
  │ └─ Services
deployment/
```

This structure keeps responsibilities clearly separated and makes scaling easier as services grow.

---

## Testing Strategy

The template encourages layered testing:

```
Controller Tests  → mock services
Service Tests     → mock repositories
Repository Tests  → in-memory database
```

Integration tests can be added later for end-to-end validation.

---

## Prerequisites

* .NET SDK 8 or later
* Any code editor

Check installation:

```
dotnet --version
```

---

## Run with Docker Compose

### Build and Run

```bash
docker compose up --build
```

### Stop

```bash
docker compose down
```

### Access Swagger

```
http://localhost:5000/swagger
```

---

## Environment

Default environment:

```
ASPNETCORE_ENVIRONMENT=Development
```

Can be changed in `docker-compose.yml`.

---

## Database (SQL Server)

* Server: `localhost,1433`
* Authentication: SQL Server Authentication
* Credentials: Defined in `.env`

---

## Usage

Clone this repository and use it as a base template for new microservices. Extend features depending on business needs.

---

## License

See the LICENSE file.

---

## Contributing

Pull requests and suggestions are welcome.

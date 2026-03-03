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
Client → SampleAuthService → JWT Token → Protected Service
```

1. User registers or logs in via **SampleAuthService**
2. SampleAuthService generates a JWT access token
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

## SampleAuthService Responsibilities

The authentication service handles:

* User registration
* Token generation
* Password hashing (BCrypt)
* Password reset (simplified placeholder)
* User profile retrieval

Advanced features such as refresh tokens, email verification, external identity providers, and account recovery are intentionally excluded to keep the template focused and easy to extend.


---

## Health Checks

Health checks allow infrastructure systems (Docker, Kubernetes, and load balancers) to determine whether the service is running and able to operate correctly.

### Endpoint

```
GET /health
```

### Purpose

* Detect container or application failures
* Support automated restarts by orchestration systems
* Verify that the service is operational
* Monitor database connectivity through the configured DbContext health check

The endpoint includes a database connectivity check to ensure the service can access its primary data store.

### Security

This endpoint does not require authentication because it is intended for infrastructure monitoring within a trusted environment and does not expose sensitive data.

---

## Event-Driven Messaging (RabbitMQ)

## Event-Driven Messaging (RabbitMQ)

This template includes optional support for **event-driven communication** between microservices using a message broker (RabbitMQ by default).

Event-driven messaging enables services to communicate asynchronously by publishing and consuming events rather than calling each other directly through REST APIs. This reduces tight coupling and improves system resilience.

> **Note:** RabbitMQ is used as a reference implementation in this template. Any compatible messaging platform (Kafka, Azure Service Bus, AWS SNS/SQS, Google Pub/Sub, etc.) can be substituted depending on system requirements.

---

### Why Event-Driven Messaging?

Synchronous REST communication creates runtime dependencies:

```
Service A → HTTP → Service B
```

If Service B is unavailable, Service A may fail or experience delays.

Event-driven messaging decouples services:

```
Service A → Message Broker → Service B / Service C / Service D
```

The publishing service does not need to know which services consume the event.

This enables independent scaling, deployment, and failure isolation.

---

### Publish–Subscribe Model

The template uses the **publish–subscribe pattern**.

Each service connects to a shared message broker and communicates through exchanges and queues.

In the current implementation, a service can publish and consume its own events internally (within the same service) to support background processing and decoupled workflows.

This allows asynchronous handling of operations without blocking the original request.

```
Publisher Service
      │
      ▼
   Message Broker
      │
      ▼
Internal Consumer (same service)
```

This pattern is useful for tasks such as:

* Sending notifications
* Triggering background jobs
* Performing non-blocking processing
* Separating write operations from side effects

---

The architecture is designed so that **other microservices can subscribe to the same events in the future without modifying the publishing service**.

```
Publisher Service
      │
      ▼
   Message Broker
      │
 ┌────┴──────────────┐
 │                   │
Internal Consumer    External Consumer Service
(same service)       (different service)
```

Multiple services can react to the same event independently, enabling loose coupling and extensibility as the system grows.

---

### Event-Driven Setup in This Template

Both the **SampleAuthService** and the **ServiceName** resource service include their own messaging configuration.

Each service can:

* Publish domain events
* Consume events from other services
* Operate independently of other services

#### Example — Publishing an Event

When a user is created in the authentication service:

```csharp
await _eventBus.PublishAsync(
    new UserCreatedEvent(user.Id, user.Email));
```

#### Example — Consuming an Event

A consuming service subscribes using a background worker:

```csharp
await _eventBus.Subscribe<UserCreatedEvent>(Handle);
```

Consumers process events asynchronously without blocking the publisher.

---

### REST vs Event Communication

Modern systems typically use a **hybrid approach**, combining REST and event-driven messaging based on requirements.

#### Use REST When

* Immediate response is required
* Client-facing operations
* Validation before processing
* Querying current state

```
Client → REST → Service → Response
```

REST provides strong consistency and request–response semantics.

---

#### Use Events When

* Notifying other services of changes
* Triggering background workflows
* Avoiding tight coupling
* Multiple services must react independently

```
Service → Event → Multiple Services
```

Events provide resilience and scalability but introduce eventual consistency.

---

### Typical Hybrid Flow

Most microservice systems follow this pattern:

```
Client → REST → Service → Event → Other Services
```

1. A client sends a request via REST
2. The service processes the operation
3. The service publishes an event
4. Other services react asynchronously

This combines real-time interaction with decoupled integration.

---

### Communication Within the Same Machine

When running locally (e.g., multiple services on different ports):

* All services connect to the same local broker instance
* No direct service URLs are required for event delivery

Example local configuration:

```
RabbitMQ → localhost:5672
```

Services communicate indirectly through the broker.

---

### Communication Across Networks

In distributed environments:

* Services connect to a shared broker endpoint
  (e.g., `rabbitmq.company.internal`)
* Services do not need to know each other’s locations
* The broker handles routing and delivery

External clients still interact with services via REST APIs.

---

### Advantages of Event-Driven Messaging

* Loose coupling between services
* Improved resilience and fault tolerance
* Independent deployment
* Horizontal scalability
* Support for asynchronous workflows

---

### Trade-Offs

* Eventual consistency between services
* More complex debugging and tracing
* Duplicate message handling required
* Additional infrastructure

Consumers should implement idempotent processing to handle possible duplicate deliveries.

---

### Message Broker Flexibility

RabbitMQ is included as a **reference implementation** for learning and development purposes.

Depending on system needs, it can be replaced with:

* Apache Kafka (high-throughput streaming)
* Azure Service Bus
* AWS SNS/SQS
* Google Pub/Sub
* Other AMQP-compatible brokers

The template’s abstraction allows swapping messaging platforms without changing application logic.

---

### Scope of Implementation

The messaging setup is intentionally minimal to keep the template approachable.

Advanced patterns such as:

* Retry policies
* Dead-letter queues
* Message versioning
* Outbox pattern
* Distributed transactions

can be added as the system evolves.


## Redis Caching

## Redis Integration

This template includes **Redis-based distributed caching** to improve performance and reduce database load for frequently accessed data.

Redis is commonly used in microservices to cache read-heavy operations such as entity lookups, authorization data, and reference data.

---

## Purpose

Caching is applied using the **cache-aside pattern**:

```
Request → Cache → Database → Cache store → Response
```

### Benefits

* Reduces database queries
* Improves response times
* Supports horizontal scaling
* Enables distributed cache across multiple service instances

---

## What Is Cached

The template demonstrates caching for:

* Entity retrieval by identifier (`GetById`)
* Authentication-related data (e.g., user lookup by email)

Large collections (`GetAll`) are intentionally not cached by default due to:

* Potential memory consumption
* Cache invalidation complexity
* Risk of stale data

Instead, production systems typically use pagination and filtering for large datasets.

---

## Cache Invalidation Strategy

Cache entries are invalidated on data changes to prevent stale reads:

| Operation | Cache Action                 |
| --------- | ---------------------------- |
| Create    | Invalidate list cache        |
| Update    | Invalidate item + list cache |
| Delete    | Remove item cache            |

This ensures consistency between the cache and the database.

---

## Implementation Details

Redis is accessed through an abstraction:

```
ICacheService
```

This keeps the Application layer independent from the caching technology and allows replacing Redis with another provider if needed.

---

## Configuration

### appsettings.json (Local Development)

```json
"Redis": {
  "ConnectionString": "localhost:6379"
}
```

### Docker Environment

When running with Docker Compose, the service connects using the container hostname:

```
Redis__ConnectionString=redis-auth:6379,abortConnect=false
```

The `abortConnect=false` option allows the service to retry connecting until Redis becomes available.

---

## Docker Setup

Redis runs as a container:

```yaml
redis-auth:
  image: redis:7
  ports:
    - "6379:6379"
  healthcheck:
    test: ["CMD", "redis-cli", "ping"]
    interval: 10s
    timeout: 5s
    retries: 10
```

Health checks ensure dependent services wait until Redis is ready.

---

## Why Distributed Cache?

In microservices, multiple instances of a service may run simultaneously.
Using a distributed cache ensures all instances share the same cached data.

---

## Production Considerations

In real systems, Redis may also be used for:

* Session storage
* Rate limiting
* Distributed locks
* Event-driven cache invalidation
* Pub/Sub messaging

These features are intentionally excluded from the template to keep it focused and extensible.

---

## Logging

This template includes structured logging using **Serilog** to capture application behavior, errors, and diagnostic information.

Logging is essential in microservices because services run in distributed environments where console output alone is insufficient for troubleshooting.

---

### Why Structured Logging?

Structured logging provides:

* Better debugging and incident analysis
* Correlation between requests and errors
* Visibility into production behavior
* Support for centralized log systems (ELK, Seq, Azure Monitor, etc.)

---

### Logging Configuration

Logging behavior is driven by configuration (`appsettings.json` or environment variables) rather than code.

Example configuration:

```json
"Serilog": {
  "MinimumLevel": {
    "Default": "Information",
    "Override": {
      "Microsoft": "Warning",
      "System": "Warning"
    }
  },
  "Enrich": [
    "FromLogContext",
    "WithMachineName",
    "WithThreadId",
    "WithEnvironmentName"
  ],
  "Properties": {
    "Application": "SampleAuthService"
  },
  "WriteTo": [
    { "Name": "Console" },
    {
      "Name": "File",
      "Args": {
        "path": "logs/log-.log",
        "rollingInterval": "Day",
        "retainedFileCountLimit": 7
      }
    }
  ]
}
```

---

### Where Logs Are Stored

In development:

* Console output (for local debugging)
* Rolling log files inside `/logs` directory

When running in Docker:

* Logs can be persisted using a volume mapping:

```
./logs:/app/logs
```

This ensures logs remain available even if containers restart.

---

### HTTP Error Logging

The template includes global middleware that:

* Logs unhandled exceptions
* Maps exceptions to appropriate HTTP responses
* Returns standardized `ProblemDetails`
* Includes a trace identifier for correlation

This prevents controllers and services from needing repetitive try-catch blocks.

---

### Non-HTTP Error Logging

Process-level exceptions are also captured:

* Unhandled exceptions
* Background task failures
* Crashes outside request pipeline

This ensures failures are recorded even if the application terminates unexpectedly.

---

### Production Considerations

In real production systems, logs are typically sent to:

* Centralized logging platforms (ELK, Splunk, Datadog)
* Cloud monitoring services
* Log aggregation pipelines

File logging in this template is primarily for local development and learning purposes.

## Rate Limiting

Rate limiting protects services from abuse and excessive traffic.

### Purpose

* Prevent brute-force attacks
* Protect system resources
* Ensure fair usage
* Improve stability

Limits can be adjusted depending on business requirements and traffic patterns. Sensitive endpoints (login, token generation) should use stricter limits in production.

---

## Testing

This template includes both **unit tests** and **integration tests** to validate behavior at different layers of the system.

The testing approach follows Clean Architecture principles and isolates concerns appropriately.

---

## Unit Tests

Unit tests validate individual components in isolation without requiring external infrastructure such as databases, caches, or message brokers.

The template includes unit tests for:

* Controllers
* Application services
* Repositories
* Authentication logic

Dependencies are mocked using **Moq**, and in-memory database providers are used where persistence behavior must be verified.

---

## Controller Tests

Controller tests verify HTTP behavior and interaction with application services without running the ASP.NET runtime.

Dependencies such as services and event bus implementations are mocked.

### Example — Controller Behavior

From `SampleControllerTests`:

```csharp
_mockService
    .Setup(x => x.GetAllAsync())
    .ReturnsAsync(new List<GetSampleRequestDto>
    {
        new GetSampleRequestDto(),
        new GetSampleRequestDto()
    });

var result = await _controller.GetAllAsync();

var ok = Assert.IsType<OkObjectResult>(result);
var data = Assert.IsAssignableFrom<IEnumerable<GetSampleRequestDto>>(ok.Value);

Assert.Equal(2, data.Count());
```

Controllers that publish events also verify event dispatch:

```csharp
_eventBus.Verify(
    e => e.PublishAsync(It.Is<SampleCreatedEvent>(
        ev => ev.Id == createdId)),
    Times.Once);
```

---

## Application Service Tests

Application services are tested independently from controllers and infrastructure.

Repositories and cache services are mocked to verify business logic only.

### Example — Business Logic Mapping

```csharp
_repoMock
    .Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
    .ReturnsAsync(entities);

var result = await _service.GetAllAsync();

Assert.Equal(2, result.Count);
Assert.Equal("One", result[0].Name);
```

---

## Cache Behavior Tests (Service Layer)

Caching is treated as an application concern, so cache behavior is tested at the service layer using mocked cache abstractions.

The tests verify the full cache lifecycle:

### Cache Hit

```csharp
_cacheMock
    .Setup(c => c.GetAsync<GetSampleRequestDto>($"sample:id:{id}"))
    .ReturnsAsync(cachedDto);

var result = await _service.GetByIdAsync(id);

_repoMock.Verify(
    r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
    Times.Never);
```

### Cache Miss → Database → Cache Set

```csharp
_cacheMock
    .Setup(c => c.GetAsync<GetSampleRequestDto>($"sample:id:{id}"))
    .ReturnsAsync((GetSampleRequestDto?)null);

_repoMock
    .Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
    .ReturnsAsync(entity);

await _service.GetByIdAsync(id);

_cacheMock.Verify(
    c => c.SetAsync(
        $"sample:id:{id}",
        It.IsAny<GetSampleRequestDto>(),
        It.IsAny<TimeSpan>()),
    Times.Once);
```

### Cache Invalidation on Data Changes

```csharp
_cacheMock.Verify(
    c => c.RemoveAsync($"sample:id:{id}"),
    Times.Once);
```

This validates that cache entries are removed when entities are added, updated, or deleted.

---

## Repository Tests

Repository tests validate persistence logic using in-memory database providers.

### Resource Service — SQLite In-Memory

```csharp
var connection = new SqliteConnection("Filename=:memory:");
await connection.OpenAsync();

var options = new DbContextOptionsBuilder<AppDbContext>()
    .UseSqlite(connection)
    .Options;

var context = new AppDbContext(options);
await context.Database.EnsureCreatedAsync();
```

### Authentication Service — EF Core InMemory Provider

```csharp
var options = new DbContextOptionsBuilder<AuthDbContext>()
    .UseInMemoryDatabase(Guid.NewGuid().ToString())
    .Options;
```

These tests verify create, read, update, and delete operations without requiring a real database.

---

## Authentication Logic Tests

Authentication workflows are tested at the service level.

### Example — Token Generation Flow

```csharp
_userRepoMock
    .Setup(x => x.GetUserByEmailAsync(email, It.IsAny<CancellationToken>()))
    .ReturnsAsync(user);

_jwtMock
    .Setup(x => x.GenerateToken(user))
    .Returns("fake-jwt");

var result = await _service.GenerateTokenAsync(dto);

Assert.Equal("fake-jwt", result!.AccessToken);
```

Low-level JWT creation is not tested directly because it relies on framework components.
Testing the token service ensures the authentication flow works correctly.

---

## Integration Tests

Integration tests validate the behavior of the application running as a real system, including:

* HTTP pipeline
* Routing
* Filters
* Authorization policies
* Database interactions

External dependencies are provided using **Docker containers via Testcontainers**.

---

## Infrastructure Used in Integration Tests

Integration tests start real instances of:

* SQL Server
* Redis (optional)
* RabbitMQ

using containerized environments.

This ensures tests run against realistic infrastructure without requiring manual setup.

---

## Authentication in Integration Tests

Instead of generating real JWT tokens, integration tests use a **fake policy evaluator** that simulates authentication and authorization.

Roles are injected via request headers:

```
X-Test-Role: Admin
X-Test-Role: WriteUser
X-Test-Role: ReadUser
```

This allows testing authorization policies without depending on the authentication service.

---

## Example — Role-Based Authorization Test

```csharp
var client = CreateClientWithRole("WriteUser");

var response = await client.DeleteAsync($"/api/v1/samples/{id}");

response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
```

---

## Database Isolation

Each integration test run:

1. Applies migrations once
2. Clears database state before tests

This ensures deterministic and repeatable results.

---

## Why Docker Must Be Running

Integration tests rely on containerized infrastructure.
Docker must be running because the tests dynamically start:

* SQL Server container
* Redis container
* RabbitMQ container

Without Docker, the infrastructure required by the application cannot be provisioned.

---

## Testing Philosophy

The template follows a layered testing strategy:

## Unit Tests

Validate behavior of individual components in isolation.

## Integration Tests

Validate interaction between components and infrastructure.

This approach provides fast feedback during development while ensuring confidence in system behavior.

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

## CI/CD Pipeline


## Continuous Integration & Deployment

This template includes GitHub Actions workflows to automate build validation, testing, and packaging.

### Pipeline Strategy

The repository uses a **two-stage CI approach** to balance speed and reliability:

#### Pull Request Validation (Fast Checks)

Runs on every PR targeting `master`:

* Build verification
* Unit tests
* Code quality checks

These checks provide fast feedback to developers and prevent obvious issues from reaching the main branch.

---

#### Master Branch Pipeline (Full Validation)

Runs after changes are merged into `master`:

* Build verification
* Unit tests
* Integration tests (with Docker infrastructure)
* Database migrations validation
* API publish
* Docker image build

Integration tests run only on the main branch because they require containerized infrastructure (SQL Server, Redis, RabbitMQ) and are intentionally heavier.

---

### Why This Approach?

* Keeps pull requests fast and developer-friendly
* Ensures full system validation before releases
* Prevents slow infrastructure tests from blocking development flow
* Mirrors real-world microservice CI strategies

---

### Infrastructure in CI

Integration tests spin up real dependencies using Docker:

* SQL Server
* Redis
* RabbitMQ

This ensures tests run against realistic environments without manual setup.

---

### Branch Protection

The `master` branch is protected with:

* Required pull requests
* Required status checks
* No direct pushes allowed

This enforces quality gates before changes are merged.

---

## How to Add a New Feature Using This Template

This template is designed to make feature development **predictable, consistent, and maintainable** across services.

When adding a new feature, follow the steps below to preserve Clean Architecture boundaries and ensure the feature integrates correctly with authentication, caching, messaging, and infrastructure concerns.

The workflow moves **from core business model outward toward delivery mechanisms**.

---

### Step 1 — Model the Domain

Define the core business concept as a domain entity.

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

**Why first?**

The Domain layer represents the system’s core business rules and must remain independent of frameworks, databases, and external services.

All other layers depend on this model — never the reverse.

---

### Step 2 — Define Use-Case DTOs

Create request/response models for the feature in the Application layer.

```
ServiceName.Application
└─ DTOs
   ├─ AddProductDto.cs
   ├─ UpdateProductDto.cs
   └─ GetProductDto.cs
```

Use separate DTOs for different operations even if they look similar initially.

This allows:

* Independent validation rules
* Safer API evolution
* Prevention of over-posting vulnerabilities
* Clear intent per operation

---

### Step 3 — Define the Application Contract

Create an interface describing the business use cases.

```
ServiceName.Application
└─ Interfaces
   └─ IProductService.cs
```

Example:

```csharp
public interface IProductService
{
    Task<IReadOnlyList<GetProductDto>> GetAllAsync();
    Task<GetProductDto?> GetByIdAsync(Guid id);
    Task<Guid> AddAsync(AddProductDto dto);
    Task UpdateAsync(UpdateProductDto dto);
    Task DeleteByIdAsync(Guid id);
}
```

**Why interfaces?**

* Enables unit testing via mocks
* Decouples API layer from implementations
* Supports future replacement of business logic

---

### Step 4 — Implement Application Logic

Implement the use cases in the Application layer.

```
ServiceName.Application
└─ Services
   └─ ProductService.cs
```

Responsibilities:

* Validation and orchestration
* DTO ↔ Entity mapping
* Enforcing business rules
* Publishing domain events (if needed)
* Cache interaction (read-through / invalidation)

The Application layer must remain independent of infrastructure details.

---

### Step 5 — Implement Data Access

Create repository implementations in the Infrastructure layer.

```
ServiceName.Infrastructure
└─ Repositories
   └─ ProductRepository.cs
```

Responsibilities:

* Database queries
* Persistence
* Transaction boundaries
* Mapping to domain entities

This isolates EF Core (or any database technology) from business logic.

---

### Step 6 — Wire Dependencies

Register implementations in the dependency injection container.

Example:

```csharp
services.AddScoped<IProductService, ProductService>();
services.AddScoped<IRepository<Product>, ProductRepository>();
```

If the feature uses caching or messaging, register related components as well.

---

### Step 7 — Expose API Endpoints

Add controller endpoints in the API layer.

```
ServiceName.Api
└─ Controllers
   └─ ProductController.cs
```

Controllers should:

* Remain thin
* Delegate logic to Application services
* Handle HTTP concerns only
* Not contain business rules

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

### Step 8 — Apply Security Policies (If Required)

Protect endpoints using authorization policies rather than inline role checks.

Example:

```csharp
[Authorize(Policy = "WritePolicy")]
```

This keeps security centralized and consistent across services.

---

### Step 9 — Integrate Cross-Cutting Concerns

Depending on the feature, integrate:

* Caching (Redis)
* Event publishing (RabbitMQ)
* Validation
* Logging
* Rate limiting
* Background processing

These concerns should be handled through existing abstractions to maintain consistency.

---
### Step 10 — Add Tests

Test the feature at multiple layers to ensure correctness and integration with the system.

#### Unit Tests

Validate each layer in isolation:

| Layer          | Testing Strategy                        |
| -------------- | --------------------------------------- |
| Controller     | Mock application services               |
| Application    | Mock repositories and external services |
| Infrastructure | In-memory or test database              |

Unit tests provide fast feedback and verify business logic without requiring external systems.

---

#### Integration Tests

Validate the feature as part of the running application.

Integration tests should cover:

* HTTP endpoints
* Routing and filters
* Authorization policies
* Database interaction
* Cross-cutting concerns (caching, messaging, etc.)

Use the provided integration test infrastructure:

* `WebApplicationFactory`
* Testcontainers (SQL Server, Redis, RabbitMQ)
* Fake authentication for role-based testing
* Real database migrations

Integration tests ensure the feature works correctly within the service’s runtime environment.

---

#### Why Both Are Required

Unit tests verify correctness of individual components.
Integration tests verify that components work together correctly.

Using both prevents:

* False confidence from isolated tests
* Fragile end-to-end tests
* Undetected configuration issues

This layered approach maintains reliability while keeping tests fast and maintainable.

---

#### Why This Process Matters

Following this workflow ensures:

* Consistent feature structure across services
* Minimal coupling between layers
* Easier testing and maintenance
* Clear separation of responsibilities
* Compatibility with distributed architecture patterns

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
  │   ├─ Application
  │   ├─ Builder
  │   └─ Services
  ├─ ServiceName.Application
  │ ├─ DTOs
  │ ├─ Events
  │ ├─ Interfaces
  │ └─ Services
  ├─ ServiceName.Domain
  │ ├─ Enums
  │ └─ Entities
  ├─ ServiceName.Infrastructure
  │ ├─ Persistence
  │ ├─ Repositories
  │ ├─ BackgroundServices
  │ ├─ Configuration
  │ ├─ Messaging
  │ ├─ Caching
  │ └─ Migrations
tests/
  ├─ ServiceName.UnitTests
  │ ├─ Controllers
  │ ├─ Repository
  │ └─ Services
  ├─ ServiceName.IntegrationTests
  │ ├─ Api
  │ ├─ Fixtures
  │ └─ Helpers
deployment/
```

### Authentication Service

```text
SampleAuthService
└─src/
  ├─ SampleAuthService.Api
  │ ├─ Controllers
  │ ├─ Middlewares
  │ └─ Extensions
  │   ├─ Application
  │   ├─ Builder
  │   └─ Services
  ├─ SampleAuthService.Application
  │ ├─ DTOs
  │ ├─ Interfaces
  │ └─ Services
  ├─ SampleAuthService.Domain
  │ ├─ Enums
  │ └─ Entities
  ├─ SampleAuthService.Infrastructure
  │ ├─ Persistence
  │ ├─ Repositories
  │ ├─ Security
  │ ├─ BackgroundServices
  │ ├─ Configuration
  │ ├─ Messaging
  │ ├─ Caching
  │ └─ Migrations
tests/
  ├─ SampleAuthService.UnitTests
  │ ├─ Controllers
  │ ├─ Repository
  │ └─ Services
  ├─ SampleAuthService.IntegrationTests
  │ ├─ Api
  │ ├─ Fixtures
  │ └─ Helpers
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

Integration tests will be added later for end-to-end validation.

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

---

## Environment

Default environment:

```
ASPNETCORE_ENVIRONMENT=Development
```

Can be changed in `docker-compose.yml`.

---

## Swagger

* ServiceName: `http://localhost:5000/swagger/index.html`
* SampleAuthService: `http://localhost:5001/swagger/index.html`

Can be changed in `docker-compose.yml`.

## Database (SQL Server)

* ServiceName DB Server: `localhost,1433`,
* SampleAuthService DB Server: `localhost,1434`
* Authentication: SQL Server Authentication
* Credentials: Defined in `.env`

Can be changed in `docker-compose.yml`.

---

## Usage

Clone this repository and use it as a base template for new microservices. Extend features depending on business needs.

---

## License

See the LICENSE file.

---

## Contributing

Pull requests and suggestions are welcome.
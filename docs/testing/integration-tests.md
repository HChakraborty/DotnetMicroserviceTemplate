# Integration Testing in Microservice Template

This document explains the **integration testing setup** provided in the microservice-template. It covers **how integration tests are structured, why each part exists, and how users can extend or adapt it**.

> ⚠️ This template includes **minimum setup needed for integration tests**. Event consumers, caching, and other components are not fully tested by default but can be added later.

---

## Table of Contents

1. [Purpose of Integration Tests](#purpose-of-integration-tests)
2. [Overall Architecture](#overall-architecture)
3. [Testable Components](#testable-components)
4. [Containers and Test Environment](#containers-and-test-environment)
5. [Database Initialization and Reset](#database-initialization-and-reset)
6. [Authentication and Authorization in Tests](#authentication-and-authorization-in-tests)
7. [Cache Replacement](#cache-replacement)
8. [Example Test Cases](#example-test-cases)
9. [Adding New Test Cases](#adding-new-test-cases)
10. [Code Coverage Considerations](#code-coverage-considerations)
11. [Limitations and Next Steps](#limitations-and-next-steps)

---

## Purpose of Integration Tests

Integration tests validate that the **service’s components work together as expected**. Unlike unit tests:

* They test **real HTTP endpoints**.
* They verify **database interactions**.
* They can validate **authentication/authorization flows**.
* They confirm **middleware and pipeline behavior**.

Integration tests in this template **do not cover caching or event consumers** by default. These can be added if needed.

---

## Overall Architecture

The microservice-template uses **ASP.NET Core**, and integration tests rely on:

1. **WebApplicationFactory** to host the API in-memory.
2. **Testcontainers** for isolated dependencies:

   * SQL Server
   * Redis (replaced with in-memory cache)
   * RabbitMQ

**Diagram: Integration Test Flow**

```
[Integration Test] 
       |
       v
[HttpClient] --> [In-Memory API Server] --> [Database / Cache / Auth]
```

* `ApiFactory` or `AuthApiFactory` hosts the service in a test environment.
* Containers simulate real dependencies, but caching is swapped for **in-memory** for speed and determinism.

---

## Testable Components

The template ensures tests can cover:

1. **Controllers and endpoints**

   * e.g., `/api/v1/samples` in ServiceName
   * e.g., `/api/v1/users` in SampleAuthService

2. **Authentication and roles**

   * ReadUser, WriteUser, Admin roles are simulated.
   * JWTs are generated for each test user.

3. **Database interactions**

   * Entity Framework Core migrations run before tests.
   * Database is reset between tests to avoid interference.

4. **Middleware behaviors**

   * Global exception handling
   * Rate limiting
   * Health check endpoints

---

## Containers and Test Environment

**Testcontainers** provide isolated dependency environments:

| Container  | Purpose          | Notes                                 |
| ---------- | ---------------- | ------------------------------------- |
| SQL Server | Stores test data | Each test resets DB tables            |
| Redis      | Cache            | Replaced by in-memory cache for tests |
| RabbitMQ   | Messaging        | Configured but not tested by default  |

**Snippet: Container Fixture**

```csharp
public class ContainersFixture : IAsyncLifetime
{
    public MsSqlContainer Sql { get; } = new MsSqlBuilder(ContainerImages.Sql)
        .WithPassword("Your_strong_password123").Build();
    
    public RedisContainer Redis { get; } = new RedisBuilder(ContainerImages.Redis).Build();
    
    public RabbitMqContainer RabbitMq { get; } = new RabbitMqBuilder(ContainerImages.RabbitMq)
        .WithUsername("guest").WithPassword("guest").Build();

    public async Task InitializeAsync()
    {
        await Sql.StartAsync();
        await Redis.StartAsync();
        await RabbitMq.StartAsync();
    }

    public async Task DisposeAsync()
    {
        await Sql.DisposeAsync();
        await Redis.DisposeAsync();
        await RabbitMq.DisposeAsync();
    }
}
```

---

## Database Initialization and Reset

* **InitializeDatabaseAsync** ensures the database is migrated once per test run.
* **ResetDatabaseAsync** clears tables before each test to maintain isolation.

**Snippet: Reset DB**

```csharp
await db.Database.ExecuteSqlRawAsync(
    "EXEC sp_msforeachtable 'DELETE FROM ?'");
```

* Ensures tests **do not interfere** with each other.
* Only core tables are reset; some services may skip this if external dependencies exist.

---

## Authentication and Authorization in Tests

To test **role-based endpoints**:

1. **FakePolicyEvaluator** overrides the default ASP.NET Core authorization pipeline.
2. **X-Test-Role header** simulates the role.
3. JWTs can also be generated using **TestJwtHelper** for services that require token-based auth.

**Snippet: Role-based Test**

```csharp
var client = _factory.CreateClient();
client.DefaultRequestHeaders.Add("X-Test-Role", "ReadUser");
var response = await client.GetAsync("/api/v1/samples");
response.StatusCode.Should().Be(HttpStatusCode.OK);
```

**Why this is needed:**

* Avoids creating real users in the auth service.
* Allows testing **permissions in isolation**.

---

## Cache Replacement

Redis is replaced by **InMemoryCacheService** to:

* Make tests **faster** and **deterministic**.
* Avoid dependency on Docker Redis for CI pipelines.

**Snippet: In-Memory Cache**

```csharp
public class InMemoryCacheService : ICacheService
{
    private readonly IMemoryCache _cache = new MemoryCache(new MemoryCacheOptions());
    
    public Task<T?> GetAsync<T>(string key) => Task.FromResult(_cache.Get<T>(key));
    public Task SetAsync<T>(string key, T value, TimeSpan exp) { _cache.Set(key, value, exp); return Task.CompletedTask; }
    public Task RemoveAsync(string key) { _cache.Remove(key); return Task.CompletedTask; }
}
```

---

## Example Test Cases

The template provides examples of:

### ServiceName

* `GetAll_ShouldReturnOk_ForReadUser`
* `Add_ShouldReturnOk_ForWriteUser`
* `Delete_ShouldReturnOk_ForAdmin`

### SampleAuthService

* `Admin_Should_Access_Any_User`
* `WriteUser_Should_Access_ReadUser`
* `ReadUser_Should_Not_Access_Others`
* `NoToken_Should_Return_Unauthorized`

**Key Concept:**
Tests cover **common user roles and CRUD actions**. Additional tests can be added per endpoint.

---

## Adding New Test Cases

1. Use the provided **ApiFactory** or **AuthApiFactory**.
2. Seed necessary database state using helper methods.
3. Use `CreateClientWithRole` or `TestJwtHelper.GenerateToken` to simulate authenticated requests.
4. Reset the database if test data should not persist across tests.

**Diagram: Test Creation Flow**

```
[Test Method]
     |
     v
[Seed Data] -> [Create Client] -> [Send HTTP Request] -> [Assert Response]
```

---

## Code Coverage Considerations

* Core controllers, auth, and database interactions are covered.
* Services like **cache** and **messaging** are stubbed to simplify tests.

> To improve coverage:
>
> * Add tests for **event consumers** if implemented.
> * Add **cache behavior tests** if you replace in-memory with Redis.
> * Add **exception scenarios** in middleware for error handling.

---

## Limitations and Next Steps

* Event consumers and caching logic are **not tested** by default.
* Tests assume **SQL Server as primary DB**; other providers may require adjustments.
* JWT and role helpers are **template-based**; customize if your auth rules differ.

**Next steps for users:**

* Add tests for **new endpoints**.
* Integrate **event and cache tests** if needed.
* Expand **role scenarios** for more granular coverage.
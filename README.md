# .NET Microservice Template (Incomplete)

A reusable starter template for building .NET microservices using Clean Architecture.

This project provides a structured base for creating new services with consistent layering and patterns. It is a minimalistic design intended to help better understand the structure without overwhelming you with too many features. It serves as a starting point.

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
* Rate Limiting
* Unit Tests

---

## Authentication & Authorization

This template uses a **separate authentication service** (`SampleAuthService`) that issues JWT access tokens.
SampleService validates the token and enforces authorization policies.

### Authentication Flow

1. User registers and generates a token via **AuthService**
2. AuthService returns a JWT access token
3. Client sends the token in requests to protected services:

```
Authorization: Bearer <access_token>
```

4. Resource services validate the token using the shared configuration

---

### JWT Configuration

Each service contains a matching configuration:

```json
"Jwt": {
  "Key": "MyVeryStrongDevelopmentKey_ChangeInProduction_123456",
  "Issuer": "SampleAuthService",
  "Audience": "SampleServices",
  "ExpireMinutes": 60
}
```

In production, these values should be stored in environment variables or a secure secret store.

---

### Roles

Supported roles:

* `ReadUser`
* `WriteUser`
* `Admin`

Roles are embedded as claims inside the JWT and enforced via policies.

---

### Authorization Policies

Example policies implemented in resource services:

| Policy      | Allowed Roles              | Purpose                                 |
| ----------- | -------------------------- | --------------------------------------- |
| ReadPolicy  | ReadUser, WriteUser, Admin | Read-only operations                    |
| WritePolicy | WriteUser, Admin           | Create / update operations              |
| AdminPolicy | Admin                      | Destructive / administrative operations |

Example usage in controllers:

```csharp
[Authorize(Policy = "ReadPolicy")]
[HttpGet]
public async Task<IActionResult> GetAll()
```

---

### AuthService Responsibilities

The authentication service handles:

* User registration
* Token generation
* Password hashing (BCrypt)
* Password reset (simplified placeholder)
* Profile endpoint

For simplicity, advanced features such as refresh tokens, email verification, and external identity providers are not included in this template but can be added depending on requirements.

---

## Health Checks

Health checks allow infrastructure components such as Docker, Kubernetes, and load balancers to determine whether the service is running and ready to handle requests.

The service exposes a health endpoint:

```
GET /health
```

### Purpose

* Detect container failures
* Support automated restarts
* Enable readiness checks during deployment
* Monitor database connectivity

Health checks are registered during startup and mapped to the `/health` endpoint.

The endpoint does not require authentication and is intended for infrastructure monitoring only.

---

## Rate Limiting

Rate limiting protects services from excessive traffic, abuse, and denial-of-service scenarios by restricting how many requests a client can make within a defined time window.

The template uses ASP.NET Core’s built-in rate limiting middleware.

### Purpose

* Prevent brute-force attacks on authentication endpoints
* Protect system resources
* Ensure fair usage across clients
* Improve service stability

Rate limiting can be applied globally or per endpoint depending on requirements.

Critical endpoints such as login and token generation should use stricter limits in production.

---

## Project Structure

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

---

## Prerequisites

* .NET SDK 8 or later
* Code or text editor

Check installation:

```
dotnet --version
```

---

## Run with Docker Compose

### Build and Run Image

```bash
docker compose up --build
```

### Stop Service

```bash
docker compose down
```

### Open in Browser

The container runs on port `5000:8080` by default (configurable in `docker-compose.yml`).

```
http://localhost:5000/swagger
```

---

## Environment

The container runs in Development mode by default but can be changed in `docker-compose.yml`.

```
ASPNETCORE_ENVIRONMENT=Development
```

---

## Database (SQL Server)

* Server: `localhost,1433`
* Authentication: SQL Server Authentication
* Credentials: Defined in `.env`

---

## Usage

Clone this repository and use it as a base template for new microservices.

---

## License

See the LICENSE file.

---

## Contributing

Pull requests and suggestions are welcome.

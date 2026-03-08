# .NET Microservice Template

A minimal starter template for building **.NET microservices using Clean Architecture**.

The repository provides a structured foundation for creating services with consistent layering, authentication, caching, messaging, and testing patterns.
The implementation is intentionally simplified to keep the architecture easy to understand and extend.

---

# Features

* Clean Architecture (API, Application, Domain, Infrastructure)
* Repository pattern
* EF Core integration
* JWT authentication & role-based authorization
* Redis caching support
* RabbitMQ event messaging
* Global exception handling
* Health checks
* Rate limiting
* Structured logging (Serilog)
* Unit and integration tests
* Docker-based development environment

---

# Prerequisites

Make sure the following tools are installed:

* .NET 8 SDK
* Docker

Verify installation:

```bash
dotnet --version
docker --version
```

---

# Running the Project

Start all services using Docker Compose:

```bash
docker compose up --build
```

Stop the services:

```bash
docker compose down
```

---

# Accessing the Services

### Swagger

Resource Service
http://localhost:5000/swagger

Authentication Service
http://localhost:5001/swagger

---

# Databases

| Service          | Host      | Port |
| ---------------- | --------- | ---- |
| Resource Service | localhost | 1433 |
| Auth Service     | localhost | 1434 |

Database credentials and configuration are defined in the `.env` file.

---

# CI / CD

Basic CI workflows are included using **GitHub Actions**.

Workflow files are located in:

```
.github/workflows/
```

Typical pipeline steps include:

* Build
* Run tests
* Code checks / linting
* Container image build

These workflows can be extended to support deployment pipelines depending on the target infrastructure.

---

# Documentation

Detailed architecture explanations and design decisions are available in:

→ **[docs/README.md](./docs/README.md)**

Topics covered include:

* Architecture overview
* Folder structure rationale
* Authentication and authorization
* Messaging and caching
* Testing strategy
* Deployment considerations
* Creating additional services from the template

---

# License

See the `LICENSE` file.

---

# Contributing

Pull requests and suggestions are welcome.

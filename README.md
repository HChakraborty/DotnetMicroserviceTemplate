
# .NET Microservice Template (Incomplete)

A reusable starter template for building .NET microservices using Clean Architecture.

This project provides a structured base for creating new services with consistent layering and patterns.

## Features

- Clean Architecture (API, Application, Domain, Infrastructure)
- Repository Pattern
- EF Core integration
- Global exception handling middleware
- CancellationToken support
- Dependency Injection configuration

## Project Structure

```text
src/
├─ ServiceName.Api
│ ├─ Controllers
│ ├─ Middlewares
│ └─ Extensions
├─ ServiceName.Application
│ ├─ DTOs
│ ├─ Interfaces
│ └─ Services
├─ ServiceName.Domain
│ ├─ Entities
│ └─ Interfaces
└─ ServiceName.Infrastructure
  ├─ Persistence
  └─ Repositories
```

## Prerequisites

- .NET SDK 8 or later
- Visual Studio / VS Code / Rider

Check installation:

`dotnet --version`

## Getting Started

Build the solution:

`dotnet build`

Run the API:

`dotnet run --project src/ServiceName.Api/ServiceName.Api`

Open Swagger:

`http://localhost:<port>/swagger`

## Usage

Clone this repository and use it as a base template for new microservices.

## License

See the LICENSE file.

## Contributing

Pull requests and suggestions are welcome.


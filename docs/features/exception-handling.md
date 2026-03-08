# Exception Handling

## Table of Contents

* [1. Overview](#1-overview)
* [2. Architecture](#2-architecture)
* [3. Request Flow](#3-request-flow)
* [4. HTTP Exception Handling](#4-http-exception-handling)
* [5. Process-Level Exception Handling](#5-process-level-exception-handling)
* [6. Logging Strategy](#6-logging-strategy)
* [7. When to Handle Exceptions at the Code Level](#7-when-to-handle-exceptions-at-the-code-level)

---

## 1. Overview

This template includes a **centralized exception handling strategy** to ensure that unexpected errors are handled consistently across the application.

The goal is to prevent unhandled exceptions from leaking internal implementation details while still providing enough information for debugging and observability.

Two levels of exception handling are implemented:

* **HTTP pipeline handling** using middleware
* **process-level handling** for runtime failures outside the request pipeline

The HTTP middleware converts unhandled exceptions into standardized **ProblemDetails** responses. This ensures that API clients always receive a structured and predictable error response.

Unhandled exceptions are also logged using **structured logging** so they can be traced and diagnosed in production environments.

---

## 2. Architecture

Exception handling is implemented in the **API layer** using middleware that intercepts exceptions during request processing.

The middleware sits inside the **ASP.NET Core request pipeline** and captures exceptions thrown by controllers, application services, or infrastructure components.

```mermaid
flowchart LR

Client --> Middleware
Middleware --> Controller
Controller --> Service
Service --> Repository
Repository --> Database
````

If any component throws an exception that is not handled internally, the middleware catches it and generates a standardized response.

Exceptions that occur **outside the HTTP pipeline** are handled separately through global runtime handlers.

This separation ensures that both request-level and process-level failures are captured and logged.

---

## 3. Request Flow

The exception middleware wraps the rest of the pipeline in a `try/catch` block.

If no exception occurs, the request continues normally.

If an exception is thrown, the middleware:

1. determines the appropriate HTTP status code
2. logs the exception with contextual information
3. returns a standardized `ProblemDetails` response

```mermaid
flowchart TD

A[Client Request] --> B[HTTP Middleware]

B --> C[Controller]
C --> D[Application Service]
D --> E[Repository]
E --> F[(Database)]

F --> G[Response]

C -->|Exception| H[Middleware Catch Block]
D -->|Exception| H
E -->|Exception| H

H --> I[Map Exception to HTTP Status]
I --> J[Log Exception]
J --> K[Return ProblemDetails Response]
```

This approach ensures that unexpected failures are handled consistently and that the API always returns a well-defined error response.

---

## 4. HTTP Exception Handling

Unhandled HTTP exceptions are captured by the `HttpErrorHandlingMiddleware`.

The middleware performs three responsibilities:

* mapping exceptions to HTTP status codes
* logging the exception
* returning a standardized `ProblemDetails` response

### Exception Mapping

Common exception types are mapped to appropriate HTTP status codes.

| Exception                     | Status Code               |
| ----------------------------- | ------------------------- |
| `ArgumentException`           | 400 Bad Request           |
| `KeyNotFoundException`        | 404 Not Found             |
| `UnauthorizedAccessException` | 401 Unauthorized          |
| Other exceptions              | 500 Internal Server Error |

Example mapping logic:

```csharp
private static HttpStatusCode MapStatusCode(Exception exception)
{
    return exception switch
    {
        ArgumentException => HttpStatusCode.BadRequest,
        KeyNotFoundException => HttpStatusCode.NotFound,
        UnauthorizedAccessException => HttpStatusCode.Unauthorized,
        _ => HttpStatusCode.InternalServerError
    };
}
```

This prevents each controller from needing to implement repetitive error handling logic.

### ProblemDetails Responses

Errors returned to clients follow the **RFC 7807 Problem Details** standard used by ASP.NET Core APIs.

Example response:

```json
{
  "status": 404,
  "title": "Resource Not Found",
  "detail": "Item not found",
  "instance": "/api/items/123",
  "traceId": "0HMS5FJ..."
}
```

The `traceId` allows client errors to be correlated with server logs.

In **development environments**, the response includes the exception message to assist debugging.

In **production environments**, a generic message is returned to avoid exposing internal details.

---

## 5. Process-Level Exception Handling

Some exceptions occur **outside the HTTP request pipeline**, such as:

* background tasks
* asynchronous task failures
* runtime failures in the host process

These exceptions are captured using global handlers registered during application startup.

Example:

```csharp
AppDomain.CurrentDomain.UnhandledException += (sender, args) =>
{
    var exception = (Exception)args.ExceptionObject;

    Log.Fatal(
        exception,
        "Unhandled process-level exception");
};
```

Another handler captures unobserved task exceptions:

```csharp
TaskScheduler.UnobservedTaskException += (sender, args) =>
{
    Log.Fatal(
        args.Exception,
        "Unobserved task exception");

    args.SetObserved();
};
```

These handlers ensure that critical failures are logged even when they occur outside the normal request pipeline.

---

## 6. Logging Strategy

All unhandled exceptions are logged using **structured logging**.

Logs include contextual request information such as:

* request path
* HTTP method
* trace identifier
* exception details

Example log template used by the middleware:

```csharp
"Unhandled exception. TraceId: {TraceId}, Path: {Path}, Method: {Method}"
```

Different log levels are used depending on the type of error.

| Status Code | Log Level   |
| ----------- | ----------- |
| 400 / 404   | Warning     |
| 401         | Information |
| 500         | Error       |

Structured logging also makes it easier to query and analyze logs in centralized logging systems.

---

## 7. When to Handle Exceptions at the Code Level

While global exception handling provides a **safety net**, some exceptions should be handled **directly in the application code**:

* **Business rule violations** – e.g., invalid operation in service logic
* **Recoverable errors** – e.g., retry on transient database or API errors
* **Domain-specific validation** – e.g., missing required fields
* **Controlled logging for specific events** – when you need to record structured context beyond what global logging captures

**Guidelines**:

1. **Handle where you can recover or provide meaningful feedback**.
   Global exception handling should not replace proper error handling inside services or repositories.

2. **Let unhandled exceptions bubble up** only if the service cannot resolve them or must propagate a failure to the client.

3. **Use domain-specific exception types** for fine-grained control, rather than relying solely on `ArgumentException` or `InvalidOperationException`.

4. **Log expected exceptions at the appropriate level** (Warning, Info) within the service layer, leaving Error logs for unexpected/unrecoverable failures.


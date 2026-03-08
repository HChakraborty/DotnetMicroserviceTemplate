# Logging

## Table of Contents

* [1. Overview](#1-overview)
* [2. Architecture](#2-architecture)
* [3. Logging Configuration](#3-logging-configuration)
* [4. Request Logging](#4-request-logging)
* [5. Structured Logging](#5-structured-logging)
* [6. Exception Logging](#6-exception-logging)
* [7. Log Storage](#7-log-storage)

---

# 1. Overview

This template uses **structured logging** to provide visibility into application behavior, errors, and operational events.

Logging is implemented using **Serilog**, which integrates with the ASP.NET logging pipeline and supports configurable log outputs.

Serilog was selected because it provides:

* structured logging with contextual properties
* flexible configuration through application settings
* multiple output targets (console, files, external logging systems)
* seamless integration with ASP.NET Core middleware

Logs are used to record:

* incoming HTTP requests
* application events
* infrastructure failures
* unhandled exceptions

The logging configuration is **driven by configuration files**, allowing environments to modify logging behavior without changing application code.

By default, logs are written to:

* the **console**
* a **rolling log file**

This configuration provides useful visibility during development while allowing production environments to integrate centralized logging systems.

---

# 2. Architecture

Logging is integrated at the **host level** so that it captures events from the entire application, including framework components and middleware.

```mermaid
flowchart LR

Client --> ASPNETPipeline
ASPNETPipeline --> RequestLogging
ASPNETPipeline --> ApplicationCode
ApplicationCode --> Logger
Logger --> LogSinks
LogSinks --> Console
LogSinks --> LogFiles
````

The logging pipeline captures events from multiple sources:

* ASP.NET request pipeline
* application services
* middleware
* infrastructure components

All logs are routed through **Serilog**, which formats and writes them to configured sinks.

---

# 3. Logging Configuration

Logging is configured during application startup.

```csharp
builder.Host
    .AddLoggingConfiguration(builder.Configuration)
    .AddGlobalExceptionHandling();
```

The logging extension initializes Serilog using the application configuration.

```csharp
public static IHostBuilder AddLoggingConfiguration(
    this IHostBuilder host,
    IConfiguration config)
{
    var logFile = $"logs/log-{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.log";

    Log.Logger = new LoggerConfiguration()
        .ReadFrom.Configuration(config)
        .WriteTo.File(logFile)
        .Enrich.FromLogContext()
        .CreateLogger();

    host.UseSerilog();

    return host;
}
```

The logging behavior such as **log levels, output formats, and sinks** is defined in `appsettings.json`.

Example configuration:

```json
"Serilog": {
  "MinimumLevel": {
    "Default": "Information",
    "Override": {
      "Microsoft": "Error",
      "System": "Error"
    }
  },
  "Enrich": [
    "FromLogContext",
    "WithMachineName",
    "WithThreadId",
    "WithEnvironmentName"
  ],
  "WriteTo": [
    { "Name": "Console" },
    {
      "Name": "File",
      "Args": {
        "path": "logs/log-.log",
        "rollingInterval": "Day",
        "rollOnFileSizeLimit": true,
        "fileSizeLimitBytes": 10485760,
        "retainedFileCountLimit": 7
      }
    }
  ]
}
```

This configuration ensures that logs are:

* written to both console and file
* rotated daily
* limited in size
* retained for a limited number of files

---

# 4. Request Logging

The template enables automatic request logging using Serilog middleware.

```csharp
app.UseSerilogRequestLogging();
```

This middleware records information about each incoming HTTP request, including:

* request method
* request path
* response status code
* request duration

Example log entry:

```
[2026-03-06 12:34:10 INF] HTTP GET /api/users responded 200 in 45 ms
```

Request logging helps developers and operators monitor traffic patterns and identify slow or failing requests.

---

# 5. Structured Logging

Logs use **structured logging**, which means important data is captured as named fields rather than plain text.

Example:

```csharp
_logger.LogWarning(
    exception,
    "Redis unavailable. Cache GET failed for key {CacheKey}",
    key);
```

Instead of writing a simple text message, the log includes structured properties:

* `CacheKey`
* exception details
* timestamp
* log level

Structured logs allow logging systems to:

* filter events
* perform searches
* build dashboards
* analyze failures

Infrastructure components such as the Redis cache service also log failures without interrupting application behavior.

Example:

```csharp
_logger.LogWarning(ex,
    "Redis unavailable. Cache GET failed for key {CacheKey}", key);
```

In this case, the cache failure is logged while the application continues to function normally.

---

# 6. Exception Logging

Unhandled exceptions are captured and logged by the application's **global exception handling system**.

Exceptions from the HTTP pipeline are handled by middleware that logs them with contextual information.

Example log message:

```
Unhandled exception. TraceId: 0HMP7C9J..., Path: /api/orders, Method: GET
```

The middleware records:

* the request path
* the HTTP method
* the trace identifier
* the exception stack trace

This information allows developers to correlate client responses with server logs.

The system also logs **process-level failures** that occur outside the HTTP pipeline.

```csharp
AppDomain.CurrentDomain.UnhandledException += (sender, args) =>
{
    Log.Fatal(
        (Exception)args.ExceptionObject,
        "Unhandled process-level exception");
};
```

Unobserved task exceptions are also captured.

```csharp
TaskScheduler.UnobservedTaskException += (sender, args) =>
{
    Log.Fatal(
        args.Exception,
        "Unobserved task exception");

    args.SetObserved();
};
```

These handlers ensure that unexpected failures are logged even if they occur outside normal request processing.

---

# 7. Log Storage

By default, logs are written to:

* the **console**
* a **rolling file inside the `logs/` directory**

Console logs are primarily useful during:

* local development
* debugging
* containerized environments where logs are collected from standard output

File logs provide a persistent local record of application activity.

Example log file:

```
logs/log-2026-03-06_12-30-00.log
```

The log file name includes a timestamp generated during application startup, allowing logs from different runs to be separated.

Log files rotate automatically based on configuration settings to prevent uncontrolled disk usage.

In **production environments**, applications typically do not rely on local log files. Instead, logs are forwarded to centralized logging systems such as:

* log aggregation platforms
* monitoring systems
* observability platforms

These systems allow teams to perform:

* distributed log search
* cross-service tracing
* operational monitoring

The template keeps the default logging infrastructure simple while allowing production environments to integrate external logging systems through configuration changes without modifying application code.


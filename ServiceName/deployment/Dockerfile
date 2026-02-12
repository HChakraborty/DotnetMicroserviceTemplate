# -----------------------------
# Build stage
# -----------------------------
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy solution
COPY *.sln ./

# Copy project files first (for cache)
COPY src/ServiceName.Api/*.csproj src/ServiceName.Api/
COPY src/ServiceName.Application/*.csproj src/ServiceName.Application/
COPY src/ServiceName.Domain/*.csproj src/ServiceName.Domain/
COPY src/ServiceName.Infrastructure/*.csproj src/ServiceName.Infrastructure/

# Restore dependencies
RUN dotnet restore src/ServiceName.Api/ServiceName.Api.csproj

# Copy full source
COPY src ./src/

# Publish
RUN dotnet publish src/ServiceName.Api/ServiceName.Api.csproj \
    -c Release \
    -o /app/publish \
    /p:UseAppHost=false


# -----------------------------
# Runtime stage
# -----------------------------
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Configure ASP.NET to listen on port 8080
ENV ASPNETCORE_URLS=http://+:8080

# Copy published output
COPY --from=build /app/publish .

EXPOSE 8080

ENTRYPOINT ["dotnet", "ServiceName.Api.dll"]

# -----------------------------
# Build stage
# -----------------------------
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy solution file
COPY *.sln ./

# Copy project files first (for layer caching)
COPY src/AuthService.Api/*.csproj src/AuthService.Api/
COPY src/AuthService.Application/*.csproj src/AuthService.Application/
COPY src/AuthService.Domain/*.csproj src/AuthService.Domain/
COPY src/AuthService.Infrastructure/*.csproj src/AuthService.Infrastructure/

# Restore dependencies
RUN dotnet restore src/AuthService.Api/AuthService.Api.csproj

# Copy full source
COPY src/ ./src/

# Publish application
RUN dotnet publish src/AuthService.Api/AuthService.Api.csproj \
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

ENTRYPOINT ["dotnet", "AuthService.Api.dll"]

# -----------------------------
# Build stage
# -----------------------------
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy solution file
COPY *.sln ./

# Copy project files first (for layer caching)
COPY src/SampleAuthService.Api/*.csproj src/SampleAuthService.Api/
COPY src/SampleAuthService.Application/*.csproj src/SampleAuthService.Application/
COPY src/SampleAuthService.Domain/*.csproj src/SampleAuthService.Domain/
COPY src/SampleAuthService.Infrastructure/*.csproj src/SampleAuthService.Infrastructure/

# Restore dependencies
RUN dotnet restore src/SampleAuthService.Api/SampleAuthService.Api.csproj

# Copy full source
COPY src/ ./src/

# Publish application
RUN dotnet publish src/SampleAuthService.Api/SampleAuthService.Api.csproj \
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

ENTRYPOINT ["dotnet", "SampleAuthService.Api.dll"]

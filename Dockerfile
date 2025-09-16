# .NET 8 Multi-stage build for ASP.NET Core
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy solution and project files
COPY *.sln ./
COPY GameSpace/GameSpace.csproj ./GameSpace/
COPY GameSpace.Tests/GameSpace.Tests.csproj ./GameSpace.Tests/
COPY tools/DbSmoke/DbSmoke.csproj ./tools/DbSmoke/

# Restore dependencies
RUN dotnet restore

# Copy source code
COPY . .

# Build and publish
WORKDIR /src/GameSpace
RUN dotnet publish -c Release -o /app/publish /p:UseAppHost=false

# Final stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Set environment variables for Cloud Run
ENV ASPNETCORE_URLS=http://0.0.0.0:8080
ENV ASPNETCORE_ENVIRONMENT=Production
EXPOSE 8080

# Copy published app
COPY --from=build /app/publish .

# Health check (optional)
HEALTHCHECK --interval=30s --timeout=3s --start-period=10s --retries=3 \
    CMD curl -f http://127.0.0.1:8080/healthz || exit 1

# Entry point
ENTRYPOINT ["dotnet", "GameSpace.dll"]
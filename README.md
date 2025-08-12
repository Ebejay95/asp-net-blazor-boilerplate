# Cybersecurity-Management-Cockpit

Eine moderne Blazor Server-Anwendung mit hexagonaler Architektur, C# .NET 8, und PostgreSQL.

Install:

dotnet tool install --global dotnet-ef
dotnet tool update --global dotnet-ef


## Features

- ✅ Hexagonale Architektur (Ports & Adapters)
- ✅ Blazor Server mit C#
- ✅ PostgreSQL Datenbank
- ✅ Benutzerverwaltung (Login, Register, Password Reset)
- ✅ Entity Framework Core Migrations
- ✅ Unit & Integration Tests
- ✅ CI/CD mit GitHub Actions
- ✅ Docker & Docker Compose
- ✅ Cookie-basierte Authentifizierung

## Projekt-Setup

### Voraussetzungen

- .NET 8 SDK
- PostgreSQL (oder Docker)
- Visual Studio 2022 / VS Code

### Lokale Entwicklung

1. **Repository klonen**

   ```bash
   git clone <repository-url>
   cd CMC
   ```

2. **Abhängigkeiten installieren**

   ```bash
   dotnet restore
   ```

3. **Datenbank starten (mit Docker)**

   ```bash
   docker-compose up -d postgres
   ```

4. **Migrations ausführen**

   ```bash
   cd src/CMC.Web
   dotnet ef database update --project ../CMC.Infrastructure
   ```

5. **Anwendung starten**

   ```bash
   dotnet run --project src/CMC.Web
   ```

6. **Browser öffnen**: https://localhost:5001

### Mit Docker Compose (Komplett)

```bash
docker-compose up --build
```

Anwendung: http://localhost:3000
pgAdmin: http://localhost:8080 (admin@example.com / admin)

## Tests ausführen

```bash
# Unit Tests
dotnet test tests/CMC.UnitTests

# Integration Tests
dotnet test tests/CMC.IntegrationTests

# Alle Tests
dotnet test
```

## Architektur

### Domain Layer (Core)

- Entities: User
- Value Objects: Email
- Domain Services & Rules

### Application Layer

- Use Cases / Services: UserService
- Ports (Interfaces): IUserRepository, IEmailService
- DTOs: RegisterUserRequest, LoginRequest, etc.

### Infrastructure Layer (Adapters)

- Database: PostgreSQL mit EF Core
- Repositories: UserRepository
- External Services: EmailService
- Configuration & DI

### Web Layer (UI)

- Blazor Server
- Razor Pages
- Authentication & Authorization

## Neue Migration hinzufügen

```bash
cd src/CMC.Web
dotnet ef migrations add <MigrationName> --project ../CMC.Infrastructure
dotnet ef database update --project ../CMC.Infrastructure
```

## Deployment

Die Anwendung kann deployed werden mit:

- Docker Images
- Azure App Service
- AWS ECS
- Kubernetes

Siehe `.github/workflows/ci.yml` für CD-Pipeline.

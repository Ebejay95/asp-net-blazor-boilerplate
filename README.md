CMC Project Architecture Guide
==============================

Überblick
---------

Das CMC (Compliance Management Center) Projekt folgt einer **Clean Architecture** mit klarer Trennung der Verantwortlichkeiten. Die Struktur basiert auf dem **Onion Model** mit Dependency Inversion.

Projektstruktur
---------------

### 🏗️ **CMC.Domain** - Domain Layer (Core)

**Zweck:** Geschäftslogik und Domänen-Modell ohne externe Abhängigkeiten

```
CMC.Domain/
├── Entities/           # Domain Entities (User, Customer, LibraryFramework, etc.)
├── ValueObjects/       # Value Objects (Email)
├── Interfaces/         # Domain Interfaces (ISoftDeletable, IVersionedEntity)
└── Common/            # Domain Exceptions

```

**Was gehört hierhin:**

-   **Entities**: Geschäftsobjekte mit Identität (`User`, `Customer`, `LibraryFramework`)
-   **Value Objects**: Unveränderliche Werte (`Email`)
-   **Domain Services**: Geschäftslogik die nicht zu einer Entity gehört
-   **Domain Interfaces**: Verträge für Soft-Delete, Versionierung
-   **Domain Exceptions**: Geschäftsspezifische Fehler

* * * * *

### 🎯 **CMC.Application** - Application Layer

**Zweck:** Use Cases und Anwendungslogik, orchestriert Domain und Infrastructure

```
CMC.Application/
├── Services/          # Application Services (UserService, CustomerService)
└── Ports/            # Interfaces für Infrastructure (IUserRepository, IEmailService)

```

**Was gehört hierhin:**

-   **Application Services**: Koordinieren Use Cases
-   **Repository Interfaces**: Datenzugriff-Verträge (`IUserRepository`)
-   **External Service Interfaces**: E-Mail, externe APIs (`IEmailService`)
-   **DTOs/Commands**: Anwendungs-spezifische Datenstrukturen

* * * * *

### 📋 **CMC.Contracts** - API Contracts

**Zweck:** API-Verträge zwischen Frontend und Backend

```
CMC.Contracts/
├── Users/             # User-bezogene Requests/DTOs
├── Customers/         # Customer-bezogene Requests/DTOs
├── LibraryFrameworks/ # Library-bezogene Requests/DTOs
├── RecycleBin/        # RecycleBin DTOs
└── Common/           # Shared Contracts (Attributes)

```

**Was gehört hierhin:**

-   **Request/Response DTOs**: API Ein-/Ausgabe-Modelle
-   **Validation Attributes**: API-Validierung
-   **Shared Contracts**: Wiederverwendbare API-Strukturen

* * * * *

### 🗄️ **CMC.Infrastructure** - Infrastructure Layer

**Zweck:** Externe Abhängigkeiten und technische Implementation

```
CMC.Infrastructure/
├── Persistence/           # Entity Framework Konfiguration
│   ├── Configurations/    # EF Entity Configurations
│   ├── Interceptors/      # EF Interceptors (RevisionInterceptor)
│   └── AppDbContext.cs    # Database Context
├── Repositories/          # Repository Implementierungen
├── Services/             # Infrastructure Services (EmailService, RecycleBinService)
├── Migrations/           # EF Database Migrations
└── ServiceCollectionExtensions/ # DI Registration

```

**Was gehört hierhin:**

-   **Database Configurations**: EF Mappings, Constraints
-   **Repository Implementations**: Datenzugriff-Logic
-   **External Service Implementations**: E-Mail, File Storage
-   **Database Migrations**: Schema-Änderungen
-   **Interceptors**: Cross-cutting Concerns (Auditing, Soft-Delete)

* * * * *

### 🌐 **CMC.Web** - Presentation Layer

**Zweck:** Blazor Server UI und Web-Controller

```
CMC.Web/
├── Pages/                 # Blazor Pages
│   ├── CockpitPagesSuperAdmins/ # Admin-spezifische Seiten
│   ├── CockpitPagesUsers/       # User-spezifische Seiten
│   └── Shared/                  # Wiederverwendbare Components
├── Controllers/           # MVC Controllers (AuthController)
├── Services/             # UI Services (DialogService, EFEditService)
├── Styles/               # CSS Styling
└── Auth/                 # Authentication Logic

```

**Was gehört hierhin:**

-   **Blazor Components**: UI-Logik und Darstellung
-   **Controllers**: RESTful APIs
-   **UI Services**: Client-seitige Services
-   **Authentication**: Login/Logout Logic

* * * * *

Abhängigkeitsrichtung
---------------------

```
Web ──→ Application ──→ Domain
 ↓           ↓
Infrastructure ──→ Domain
      ↓
  Contracts

```

-   **Domain** hat keine Abhängigkeiten (Clean Architecture Core)
-   **Application** verwendet nur Domain
-   **Infrastructure** implementiert Application.Ports
-   **Web** orchestriert alles über Dependency Injection

* * * * *

Key Design Patterns
-------------------

### 🔄 **Repository Pattern**

```
// Application definiert Interface
public interface IUserRepository { ... }

// Infrastructure implementiert
public class UserRepository : IUserRepository { ... }

```

### 🎭 **Dependency Inversion**

```
// High-level modules depend on abstractions
public class UserService
{
    public UserService(IUserRepository repo) { ... }
}

```

### 📝 **Audit Trail (Cross-Cutting)**

-   **RevisionInterceptor**: Automatisches Tracking aller Änderungen
-   **Soft-Delete**: Logisches Löschen mit IsDeleted-Flag
-   **RecycleBin**: Wiederherstellung gelöschter Entities

### 🎯 **Value Objects**

```
// Domain-getriebene Validierung
public record Email(string Value) { ... }

```

* * * * *

Technologie-Stack
-----------------

-   **.NET 8** - Framework
-   **Entity Framework Core** - ORM mit PostgreSQL
-   **Blazor Server** - UI Framework
-   **PostgreSQL** - Database mit JSONB Support
-   **Clean Architecture** - Architektur-Pattern

* * * * *

Entwicklungsrichtlinien
-----------------------

### ✅ **Machen Sie:**

-   Domain Logic in CMC.Domain
-   Repository Interfaces in Application.Ports
-   EF Configurations für alle Entities
-   Dependency Injection für alle Services

### ❌ **Vermeiden Sie:**

-   Domain Dependencies auf Infrastructure
-   Business Logic in Controllers
-   Direkte DbContext-Nutzung außerhalb Infrastructure
-   Unvalidierte Value Objects

* * * * *

Build & Setup
-------------

1.  **Database:** PostgreSQL Container via Docker Compose
2.  **Migrations:** `dotnet ef database update` in Infrastructure
3.  **Run:** `dotnet run` in CMC.Web

Die Architektur unterstützt testbare, wartbare und erweiterbare Entwicklung durch klare Trennung der Verantwortlichkeiten.

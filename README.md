<div align="center">

# LibraryMS — University Library Management API

  <div>
    <img src="https://img.shields.io/badge/-.NET_9-512BD4?style=for-the-badge&logo=dotnet&logoColor=white" />
    <img src="https://img.shields.io/badge/-C%23-239120?style=for-the-badge&logo=csharp&logoColor=white" />
    <img src="https://img.shields.io/badge/-ASP.NET_Core-512BD4?style=for-the-badge&logo=dotnet&logoColor=white" />
    <img src="https://img.shields.io/badge/-Entity_Framework_Core-512BD4?style=for-the-badge&logo=dotnet&logoColor=white" /> <br/>
    <img src="https://img.shields.io/badge/-PostgreSQL-4169E1?style=for-the-badge&logo=PostgreSQL&logoColor=white" />
    <img src="https://img.shields.io/badge/-JWT-000000?style=for-the-badge&logo=jsonwebtokens&logoColor=white" />
    <img src="https://img.shields.io/badge/-Cloudinary-3448C5?style=for-the-badge&logo=Cloudinary&logoColor=white" />
    <img src="https://img.shields.io/badge/-Swagger-85EA2D?style=for-the-badge&logo=swagger&logoColor=black" /> <br/>
    <img src="https://img.shields.io/badge/-xUnit-512BD4?style=for-the-badge&logo=dotnet&logoColor=white" />
    <img src="https://img.shields.io/badge/-Docker-2496ED?style=for-the-badge&logo=docker&logoColor=white" />
    <img src="https://img.shields.io/badge/-Azure_Container_Apps-0078D4?style=for-the-badge&logo=microsoftazure&logoColor=white" />
    <img src="https://img.shields.io/badge/-GitHub_Actions-2088FF?style=for-the-badge&logo=github-actions&logoColor=white" />
  </div>

</div>

<br/>

LibraryMS Backend is a robust, enterprise-grade RESTful Web API built with **.NET 9**, designed to power all library operations for the LibraryMS platform. It implements **Clean Architecture** principles to ensure a clear separation of concerns, maintainability, and testability across all layers. The API handles complex business rules including borrow limits, soft-deletion of catalog items, a multi-stage user registration and approval workflow, and automated email notifications — serving as the reliable data and logic backbone for the frontend application.

> 🔗 **Frontend Repository:** [LibraryMS Frontend](https://github.com/ErkyHanma/libraryms)

---

## 📔 Table of Contents

- [Features](#features)
- [API Documentation](#api-documentation)
- [Tech Stack](#tech-stack)
- [Architecture](#architecture)
- [Database Schema](#database-schema)
- [Testing](#testing)
- [Infrastructure & Deployment](#infrastructure--deployment)
- [Local Development Setup](#local-development-setup)
- [Disclaimer](#disclaimer)

---

## ✨ Features <a id="features"></a>

### Book & Catalog Management

- **Book CRUD:** Create, read, update, and soft-delete books from the library catalog.
- **Category Management:** Manage book classifications with unique name constraints — category names can be reused after soft-deletion.
- **Cover Images:** Book cover images are uploaded and delivered via **Cloudinary**.
- **Soft Delete Pattern:** Deleted items are never permanently removed, preserving data integrity and allowing recovery via a `DeletedAt` timestamp with global EF Core query filters.

### User Authentication & Authorization

- **ASP.NET Core Identity:** Full identity management including registration, login, and role-based access control.
- **JWT Bearer Authentication:** Stateless authentication with **Refresh Token** support for seamless session management.
- **Role-Based Access:** Separate permissions for regular library members and administrators.

### Borrowing Workflow

- **Borrow & Return:** Full borrowing lifecycle management — tracking `BorrowDate`, `DueDate`, and `ReturnDate` per record.
- **Borrow Limits:** Business rules enforce borrowing caps per user.
- **Status Tracking:** Borrow status (active, returned, overdue) is determined dynamically via date comparisons.
- **History Preservation:** Borrow records use a restrict-delete relationship with books, ensuring the full borrowing history is never lost.

### User Approval Workflow

- **Account Requests:** New user registrations go through a multi-stage admin approval process before gaining access.
- **Email Notifications:** Automated emails are sent via **MailKit/MimeKit** (SMTP) to notify users whether their account was approved or rejected, as well as other key account events.

### Admin Dashboard

- **Statistics:** Aggregated real-time data on total books, users, active loans, and pending account requests.

### API Documentation

- **Swagger/OpenAPI:** Full interactive API documentation available out of the box for all endpoints.

---

## 📖 API Documentation <a id="api-documentation"></a>
 
The full API is documented and explorable via **Swagger UI**, deployed alongside the live backend instance.
 
🔗 **Live Swagger UI:** [https://libraryms.greenwater-8b3cb852.eastus.azurecontainerapps.io/swagger/index.html](https://libraryms.greenwater-8b3cb852.eastus.azurecontainerapps.io/swagger/index.html)
 
All endpoints are documented with their expected request bodies, parameters, and responses. You can explore and test the API directly from the browser — no local setup required.
 
### Trying it out with the Demo Account
 
A read-only demo account is available to authenticate and interact with the API through Swagger without needing to register:
 
| Field | Value |
| :--- | :--- |
| **Email** | `demo@example.com` |
| **Password** | `DemoPa$$word123` |
 
> ⚠️ **This is a restricted demo account.** It is limited to **read-only actions** only and cannot perform any write operations such as creating, updating, or deleting data. It is intended solely for exploring the available endpoints and understanding the API structure.
 
**To authenticate in Swagger:**
1. Call the `/api/auth/login` endpoint with the demo credentials above to obtain a JWT token.
2. Click the **Authorize** button (🔒) at the top of the Swagger UI.
3. Enter the token in the format `Bearer <your_token>` and confirm.
4. All secured endpoints will now be accessible for the duration of the token's validity.
 
---

## 💻 Tech Stack <a id="tech-stack"></a>

| Component | Technology | Notes |
| :--- | :--- | :--- |
| **Framework** | `.NET 9`, `ASP.NET Core` | RESTful Web API |
| **Database** | `PostgreSQL` via EF Core | Production; InMemory for dev/testing |
| **Identity** | `ASP.NET Core Identity`, `JWT Bearer` | Refresh token support |
| **ORM** | `Entity Framework Core` | Code-first migrations, global query filters |
| **Storage** | `Cloudinary` | Book cover image uploads and delivery |
| **Email** | `MailKit` / `MimeKit` | SMTP email notifications |
| **Documentation** | `Swagger / OpenAPI` | Auto-generated interactive API docs |
| **Containerisation** | `Docker` | Multi-stage Dockerfile for production builds |
| **Testing** | `xUnit`, `Moq`, `FluentAssertions` | Unit & integration test suites |

---

## 🏛️ Architecture <a id="architecture"></a>

The solution is built following **Clean Architecture** principles, partitioned into four distinct layers. The core rule is **inward-only dependencies** — inner layers (Domain and Application) have zero knowledge of outer layers (Infrastructure and WebApi).

```
LibraryMS-backend/
│
├── Core.Domain/                  # Enterprise logic — Entities, Enums, Repository Interfaces
├── Core.Application/             # Business logic — Services, DTOs, AutoMapper, FluentValidation
│
├── Infrastructure.Persistence/   # EF Core DbContext, Repositories, Migrations (PostgreSQL)
├── Infrastructure.Identity/      # ASP.NET Identity, JWT, Refresh Tokens
├── Infrastructure.Shared/        # Email (MailKit), Cloudinary integrations
│
├── LibraryMS.WebApi/             # Entry point — Controllers, Middleware, Swagger, Exception Handling
│
└── Tests/
    ├── LibraryMS.Tests.UnitTests/        # Service-level unit tests
    └── LibraryMS.Tests.IntegrationTests/ # Repository-level integration tests
```

### Layer Responsibilities

| Layer | Project | Responsibility |
| :--- | :--- | :--- |
| **Domain** | `Core.Domain` | Enterprise logic: Entities, Enums, and Repository Interfaces. Zero dependencies on other projects. |
| **Application** | `Core.Application` | Business logic: Services, DTOs, AutoMapper profiles, and FluentValidation rules. |
| **Infrastructure** | `Infrastructure.*` | Implementation details: Persistence (EF Core/PostgreSQL), Identity (JWT), and Shared (Email/Cloudinary). |
| **Web API** | `LibraryMS.WebApi` | Entry point: Controllers, Middleware, Swagger, and Global Exception Handling. |

Each layer exposes an extension method to `IServiceCollection` to register its own dependencies, keeping the wiring clean and following the **Inversion of Control (IoC)** pattern.

---

## 🗄️ Database Schema <a id="database-schema"></a>

The system manages two EF Core contexts — `LibraryMSContext` for library data and `IdentityContext` for authentication — whose relationships are illustrated in the diagram below.

<div align="center">
<img width="900" alt="LibraryMS DB Diagram" src="https://github.com/user-attachments/assets/62236666-6fa2-4b8c-8d2b-a9281f8000de" />

</div>


### Key Design Decisions

- **Soft Delete:** All major entities use a `DeletedAt` timestamp. EF Core global query filters automatically exclude soft-deleted records from all queries.
- **Borrow Status:** Determined dynamically at query time by comparing `BorrowDate`, `DueDate`, and `ReturnDate` — no stored status enum that can go stale.
- **Category Uniqueness:** Enforced only on active (non-deleted) records, allowing category names to be reused after deletion.

---

## 🧪 Testing <a id="testing"></a>

The testing strategy covers two layers to ensure reliability of both business logic and data persistence.

| Project | Focus | Tools |
| :--- | :--- | :--- |
| `LibraryMS.Tests.UnitTests` | `Core.Application` service-level logic | `xUnit`, `Moq`, `FluentAssertions` |
| `LibraryMS.Tests.IntegrationTests` | `Infrastructure.Persistence` repository-level data integrity | `xUnit`, `EF Core InMemory`, `FluentAssertions` |

- **Unit Tests:** Services are tested in complete isolation using `Moq` to mock all dependencies. `FluentAssertions` provides readable, expressive validation.
- **Integration Tests:** Repository interactions are tested against an **EF Core InMemory** database, simulating real data persistence without requiring a physical PostgreSQL instance.

Run all tests with:

```bash
dotnet test
```

---

## ☁️ Infrastructure & Deployment <a id="infrastructure--deployment"></a>

- **Backend:** Containerised and deployed on **Azure Container Apps**.
- **Database:** **Neon PostgreSQL** (serverless).
- **CI/CD:** Automated build and deployment pipeline via **GitHub Actions**.
- **Containerisation:** Multi-stage `Dockerfile` provided for optimised production images.
- **Cloud Storage:** **Cloudinary** for book cover images.

---

## ⚙️ Local Development Setup <a id="local-development-setup"></a>

### Prerequisites

- [.NET 9 SDK](https://dotnet.microsoft.com/download)
- [PostgreSQL 14+](https://www.postgresql.org/download/)
- [Docker](https://www.docker.com/products/docker-desktop) *(optional, for containerised runs)*

### Configuration

The following files are git-ignored and must be created manually. Add your settings to `appsettings.Development.json` inside `LibraryMS.WebApi/`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=libraryms;Username=your_user;Password=your_password",
    "IdentityConnection": "Host=localhost;Database=libraryms_identity;Username=your_user;Password=your_password"
  },
  "JwtSettings": {
    "Secret": "your_jwt_secret_key",
    "ExpiryMinutes": 60,
    "RefreshTokenExpiryDays": 7
  },
  "Cloudinary": {
    "CloudName": "your_cloud_name",
    "ApiKey": "your_api_key",
    "ApiSecret": "your_api_secret"
  },
  "EmailSettings": {
    "Host": "smtp.your-provider.com",
    "Port": 587,
    "Username": "your_email",
    "Password": "your_password"
  }
}
```

---

### 1. Apply Database Migrations

```bash
dotnet ef database update --project LibraryMS.Infrastructure.Persistence
dotnet ef database update --project LibraryMS.Infrastructure.Identity
```

---

### 2. Run the API

```bash
dotnet run --project LibraryMS.WebApi
```

Swagger UI will be available at `http://localhost:<port>/swagger`.

---

### 3. Run with Docker *(optional)*

```bash
docker build -t libraryms-backend .
docker run -p 8080:8080 libraryms-backend
```

---

## ⚠️ Disclaimer

All books, user data, and any other content present in this application are **entirely fictional and for demonstration purposes only**. I do not own, claim ownership of, or have any affiliation with any of the books or entities referenced within the platform. LibraryMS is not a real library or registered organization of any kind — it is a **personal portfolio project** built purely for educational purposes and to demonstrate full-stack software development skills. Any resemblance to real institutions, persons, or published works is coincidental.

---

## 📃 License

This project is licensed under the [MIT License](./LICENSE).


# Products Management API

A RESTful Products Management API developed with **.NET 10, ASP.NET Core, Entity Framework Core and SQL Server**.

This project was developed as part of a .NET Software Engineer technical assessment. It demonstrates REST API development, layered architecture, database persistence, validation, optimistic concurrency, atomic stock operations and automated testing.

The sample product catalogue uses generic optical products, such as contact lenses, progressive lenses and intraocular lenses.

## 1. Technology Stack

| Component | Technology |
|---|---|
| Framework | .NET 10 |
| Web API | ASP.NET Core Controllers |
| Database | Microsoft SQL Server 2022 |
| ORM | Entity Framework Core 10 |
| Database management | EF Core Code First Migrations |
| Unit testing | NUnit + NSubstitute |
| Integration testing | NUnit + WebApplicationFactory + Testcontainers |
| Local infrastructure | Docker Compose |
| API documentation | OpenAPI |

## 2. Architecture

The solution follows a pragmatic Clean Architecture approach, separating business rules, application use cases, infrastructure and HTTP concerns.

```text
Products/
├── src/
│   ├── Products.Api/
│   ├── Products.Application/
│   ├── Products.Domain/
│   └── Products.Infrastructure/
├── tests/
│   ├── Products.UnitTests/
│   └── Products.IntegrationTests/
├── docker/
│   ├── compose.yaml
│   └── .env.example
├── scripts/
│   ├── Init.ps1
│   └── SetConnectionString.ps1
└── Products.slnx
```

### Products.Api

Responsible for HTTP endpoints, request handling, API-specific validation and exception handling.

Controllers delegate business operations to Application services.

### Products.Application

Contains application services, DTOs, mappings, interfaces and application-specific exceptions.

The Application layer does not depend on Entity Framework Core or concrete infrastructure implementations.

### Products.Domain

Contains the Product entity, business rules and domain validation messages.

The Domain project has no dependencies on the other solution projects.

### Products.Infrastructure

Implements data access using EF Core and SQL Server, including repositories, database configurations, migrations and database seeding.

### Dependency direction

- API → Application and Infrastructure
- Infrastructure → Application and Domain
- Application → Domain
- Domain → No project dependencies

The API acts as the composition root, while each relevant layer exposes its own Dependency Injection registration method.

## 3. API Endpoints

| Method | Endpoint | Description |
|---|---|---|
| GET | `/api/products` | Retrieve all products |
| GET | `/api/products/{id}` | Retrieve a product by ID |
| POST | `/api/products` | Create a product |
| PUT | `/api/products/{id}` | Update product details |
| DELETE | `/api/products/{id}` | Delete a product |
| POST | `/api/products/{id}/decrement-stock/{quantity}` | Decrease available stock |
| POST | `/api/products/{id}/add-to-stock/{quantity}` | Increase available stock |
| GET | `/api/products/search?name={name}` | Search products by partial name |
| GET | `/api/products/stock-level?min={min}&max={max}` | Filter products by stock range |

All nine required endpoints are implemented.

The update endpoint modifies product details without directly overwriting stock. Stock adjustments are handled through dedicated endpoints.

## 4. Prerequisites

To run the application locally, install:

- .NET 10 SDK
- Docker Desktop with Linux containers enabled
- PowerShell
- EF Core CLI tools
- Visual Studio 2026 or another compatible .NET development environment (optional)

Install the EF Core CLI if it is not already available:

```powershell
dotnet tool install --global dotnet-ef --version "10.0.*"
```

## 5. Local Setup

### Step 1 — Clone the repository

```powershell
git clone <repository-url>
cd <repository-folder>
```

Replace the placeholders with the actual repository URL and folder.

### Step 2 — Initialize the local environment

Run from the solution root:

```powershell
.\scripts\Init.ps1
```

On the first execution, the script creates `docker/.env` from `docker/.env.example`.

Edit `docker/.env` and replace the placeholder with a strong password:

```dotenv
SQL_SA_PASSWORD=YOUR_STRONG_LOCAL_PASSWORD
```

Do not commit this file to source control.

### Step 3 — Run the initialization again

```powershell
.\scripts\Init.ps1
```

The initialization process:

1. Checks the required local tools.
2. Starts SQL Server using Docker Compose.
3. Configures the database connection through .NET User Secrets.
4. Applies EF Core database migrations.
5. Executes the database seeding logic.

The script can be executed again without intentionally deleting existing application data.

If your PowerShell execution policy prevents locally authored scripts from running, review the system execution policy configuration. `RemoteSigned` is a common option for local development.

### Step 4 — Start the API

From the solution root:

```powershell
dotnet run --project src/Products.Api
```

Alternatively, open `Products.slnx` in Visual Studio and select `Products.Api` as the startup project.

The HTTP address is determined by the application's launch configuration or the URLs supplied when starting the application.

### Step 5 — Access the API

Use the configured application address followed by:

```text
/api/products
```

In Development, the OpenAPI document is available at:

```text
/openapi/v1.json
```

## 6. Database

The application uses SQL Server 2022, running in a Docker container for local development.

EF Core Code First migrations manage the database schema.

The database contains:

- Products
- EF Core migration history
- ProductIds sequence

### Product model

| Property | Type | Description |
|---|---|---|
| Id | int | Unique six-digit product identifier |
| Name | string | Required product name |
| Description | string? | Optional description |
| Price | decimal | Product price |
| Stock | int | Available quantity |
| CreatedAtUtc | DateTime | Creation timestamp |
| UpdatedAtUtc | DateTime? | Last update timestamp |
| RowVersion | byte[] | SQL Server concurrency token |

### Database seeding

Initial demonstration data is created using EF Core seeding.

The sample catalogue includes generic optical products.

The seeding logic checks whether products already exist before adding initial records, preventing duplicate initial population during subsequent initialization attempts.

The seed data is intended for development and assessment purposes.

## 7. Data Consistency and Concurrency

### Six-digit product identifiers

Product identifiers are generated using a SQL Server SEQUENCE:

```sql
CREATE SEQUENCE dbo.ProductIds
AS int
START WITH 100000
INCREMENT BY 1
MINVALUE 100000
MAXVALUE 999999
NO CYCLE;
```

The database centrally coordinates identifier generation, preventing duplicate sequence values across multiple application instances using the same database.

**Known limitation:** A six-digit numeric identifier offers a maximum of 900,000 distinct values.

The sequence does not recycle identifiers automatically. Additionally, gaps may occur because sequence values can be consumed even when transactions fail.

For a larger retail platform, a wider technical primary key and a separate business-facing product code would be preferable.

### Optimistic concurrency

SQL Server `rowversion` is used to detect conflicting product updates.

The update request includes the previously retrieved version, encoded as Base64.

The persistence layer verifies that the expected version still matches the database record.

Conflicting updates are rejected with HTTP `409 Conflict`, preventing silent overwriting of concurrent changes.

### Atomic stock updates

Stock operations use conditional SQL updates through EF Core `ExecuteUpdateAsync()`.

This avoids read-modify-write races between concurrent requests.

Stock decrement operations only succeed when sufficient stock is available.

Stock increment operations include protection against integer overflow.

Failed stock adjustments are translated into appropriate application results.

## 8. Validation and Error Handling

Validation is performed across the appropriate layers:

- DataAnnotations and custom DTO validation for incoming API requests.
- Application validation for use-case parameters.
- Domain validation to preserve entity invariants.
- Database constraints as an additional integrity safeguard.

Centralized exception handling uses ASP.NET Core `IExceptionHandler` and ProblemDetails.

Typical responses include:

| HTTP status | Meaning |
|---|---|
| 200 OK | Successful retrieval, update or stock adjustment |
| 201 Created | Product successfully created |
| 204 No Content | Product successfully deleted |
| 400 Bad Request | Invalid input |
| 404 Not Found | Product does not exist |
| 409 Conflict | Concurrency conflict or invalid stock adjustment |
| 500 Internal Server Error | Unexpected failure |

Unexpected internal errors are not intentionally exposed to clients through the application's global exception handler.

## 9. Automated Testing

The project includes unit and integration tests using NUnit.

### Unit Tests

Unit tests verify domain behavior and application service operations.

NSubstitute is used to isolate application services from repository implementations.

Run:

```powershell
dotnet test tests/Products.UnitTests/Products.UnitTests.csproj
```

### Integration Tests

Integration tests execute HTTP requests against the ASP.NET Core application through WebApplicationFactory.

Testcontainers provides an isolated SQL Server database using the same database provider as the application.

The suite covers:

- Product creation and retrieval
- Product updates and deletion
- Invalid requests
- Partial-name search
- Stock-range filtering
- Stock adjustments
- Concurrent product creation
- Concurrent stock decrement operations
- Stale rowversion conflicts

Run:

```powershell
dotnet test tests/Products.IntegrationTests/Products.IntegrationTests.csproj
```

Docker must be available for these tests.

A shared SQL Server Testcontainer reduces startup overhead, while product data is cleared between tests.

The tests do not require access to the developer's local database.

### Run all tests

```powershell
dotnet test Products.slnx
```

## 10. Architectural Decisions and Trade-offs

| Decision | Rationale |
|---|---|
| Clean Architecture | Separation of responsibilities and dependency inversion |
| Repository per entity | Keeps persistence concerns outside Application |
| No generic repository | Avoids unnecessary abstraction |
| Dependency Injection per layer | Centralizes service registration while keeping Program.cs small |
| SQL Server SEQUENCE | Centralized, concurrency-safe ID generation |
| SQL Server rowversion | Optimistic concurrency protection |
| Atomic stock operations | Prevents lost updates and negative stock under concurrency |
| Explicit DTO mappings | Readable mappings without an additional dependency |
| NUnit | Suitable parametrization, fixtures and integration with .NET tooling |
| Testcontainers | Repeatable integration tests against a real database |
| Docker Compose | Reproducible local environment |
| User Secrets | Keeps local connection credentials outside source control |

### Scope limitations

This assessment intentionally focuses on product management.

Authentication, authorization, distributed caching, message brokers, auditing infrastructure and production deployment automation are outside the requested scope.

The solution prioritizes maintainability and correct core behavior over additional infrastructure complexity.

## 11. Security Considerations

The Docker environment uses a local SQL Server administrator account solely for assessment and development.

Credentials are not stored in the committed source code.

Local development may use `TrustServerCertificate=True` with the isolated SQL Server container.

A production deployment should instead use least-privilege database credentials, trusted TLS certificates and a dedicated secrets-management mechanism.

## 12. Final Notes

The solution is intended to demonstrate maintainable .NET development, REST API design, database consistency and meaningful automated testing.

The architecture keeps business rules independent from infrastructure implementation details, while SQL Server provides the consistency guarantees required for concurrent product and stock operations.

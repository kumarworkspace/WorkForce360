# Clean Architecture Implementation

## Overview

This application now follows Clean Architecture principles, providing a clear separation of concerns and improved maintainability.

## Architecture Layers

### 1. Core/Domain Layer (Innermost - No Dependencies)
**Location:** `Core/Domain/`

Contains the business entities and interfaces. This layer has NO dependencies on any other layers.

#### Entities (`Core/Domain/Entities/`)
- `BaseEntity.cs` - Base class with common audit fields
- `Document.cs` - Document entity
- `DocumentDetail.cs` - Document version details
- `Policy.cs` - Policy entity
- `AuditLog.cs` - Audit logging entity
- `SSO.cs` - Single Sign-On entity
- `AcknowledgementRequest.cs` - Acknowledgement request entity
- `AcknowledgementStatusEntry.cs` - Acknowledgement status entity

#### Value Objects (`Core/Domain/ValueObjects/`)
- `AuthorInfo.cs` - Author/Approver information
- `PersonInfo.cs` - Person information
- `PeriodicRequestSettings.cs` - Periodic request settings

#### Enums (`Core/Domain/Enums/`)
- `PolicyStatus.cs` - Active, Draft, Disabled
- `DocumentStatus.cs` - Draft, PendingApproval, Approved, Rejected, Published
- `ApprovalStatus.cs` - Pending, Approved, Rejected
- `ActionType.cs` - Create, Update, Delete, View, Approve, Reject

#### Interfaces (`Core/Domain/Interfaces/`)
- `IRepository<T>` - Generic repository interface
- `IDocumentRepository` - Document-specific repository
- `IDocumentDetailRepository` - Document detail repository
- `IPolicyRepository` - Policy repository
- `IAuditLogRepository` - Audit log repository
- `IUnitOfWork` - Unit of Work pattern for transaction management

### 2. Core/Application Layer (Business Logic)
**Location:** `Core/Application/`

Contains application business logic, use cases, and service interfaces.

#### Interfaces (`Core/Application/Interfaces/`)
- `IDocumentService` - Document operations
- `IPolicyService` - Policy operations
- `IAuthenticationService` - Authentication operations
- `IAcknowledgementRequestService` - Acknowledgement request operations
- `IAcknowledgementStatusService` - Acknowledgement status operations
- `IFileStorageService` - File storage abstraction

#### Services (`Core/Application/Services/`)
- `DocumentService.cs` - Document business logic
- `PolicyService.cs` - Policy business logic
- `AuthenticationService.cs` - Authentication logic
- `AcknowledgementRequestService.cs` - Acknowledgement request logic
- `AcknowledgementStatusService.cs` - Acknowledgement status logic

#### DTOs (`Core/Application/DTOs/`)
- `AuthResult.cs` - Authentication result
- `DocumentUploadRequest.cs` - Document upload request
- `DocumentUploadResult.cs` - Document upload response

### 3. Infrastructure Layer (External Concerns)
**Location:** `Infrastructure/`

Contains implementations of interfaces defined in Domain and Application layers.

#### Data (`Infrastructure/Data/`)
- `CactusDbContext.cs` - Entity Framework DbContext

#### Configurations (`Infrastructure/Data/Configurations/`)
- Entity Type Configurations for each entity
- Handles EF Core mappings, table names, column mappings, relationships

#### Repositories (`Infrastructure/Repositories/`)
- `Repository<T>` - Generic repository implementation
- `DocumentRepository` - Document repository
- `DocumentDetailRepository` - Document detail repository
- `PolicyRepository` - Policy repository
- `AuditLogRepository` - Audit log repository
- `UnitOfWork` - Transaction management implementation

#### Services (`Infrastructure/Services/`)
- `FileStorageService.cs` - File system storage implementation

### 4. Presentation Layer (UI)
**Location:** Root - `Components/`, `Pages/`, `Shared/`

Your existing Blazor Server UI components remain unchanged.

## Dependency Flow

```
Presentation → Application → Domain
         ↓
   Infrastructure (implements interfaces from Application & Domain)
```

## Key Benefits

### 1. Separation of Concerns
- Each layer has a specific responsibility
- Business logic is independent of UI and data access
- Easy to understand and navigate

### 2. Testability
- Domain layer can be tested in isolation
- Application services can be tested with mocked repositories
- Infrastructure can be tested independently

### 3. Maintainability
- Changes to database don't affect business logic
- Changes to UI don't affect business rules
- Easy to add new features following established patterns

### 4. Repository Pattern
- Abstracts data access
- Provides clean API for data operations
- Supports unit testing with mock repositories

### 5. Unit of Work Pattern
- Manages transactions across multiple repositories
- Ensures data consistency
- Single point of commit/rollback

## Dependency Injection Setup

Located in `Program.cs`:

```csharp
// Repositories
builder.Services.AddScoped<IDocumentRepository, DocumentRepository>();
builder.Services.AddScoped<IDocumentDetailRepository, DocumentDetailRepository>();
builder.Services.AddScoped<IPolicyRepository, PolicyRepository>();
builder.Services.AddScoped<IAuditLogRepository, AuditLogRepository>();

// Unit of Work
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// Infrastructure Services
builder.Services.AddScoped<IFileStorageService, FileStorageService>();

// Application Services
builder.Services.AddScoped<IPolicyService, PolicyService>();
builder.Services.AddScoped<IDocumentService, DocumentService>();
// ... other services
```

## Migration from Old Structure

### Old Structure:
```
Models/          → Domain entities mixed with data annotations
Data/            → DbContext
Services/        → Services directly accessing DbContext
Components/      → UI components
Pages/           → UI pages
```

### New Clean Architecture:
```
Core/
  Domain/
    Entities/          → Pure domain entities (no EF attributes)
    Enums/             → Business enums
    ValueObjects/      → Value objects
    Interfaces/        → Repository interfaces
  Application/
    Interfaces/        → Service interfaces
    Services/          → Business logic
    DTOs/              → Data transfer objects
Infrastructure/
  Data/
    CactusDbContext.cs → DbContext
    Configurations/    → EF mappings
  Repositories/        → Repository implementations
  Services/            → Infrastructure services (file storage, etc.)
Presentation/          → UI (Components, Pages, Shared) - kept in root for Blazor compatibility
```

## Database Migrations

The database structure remains the same. EF Core configurations are now separated into individual configuration classes in `Infrastructure/Data/Configurations/`.

## Important Changes

### Enum Conversions
Enums are now properly typed in the domain layer and converted to strings in the database:
- `DocumentStatus` (was string, now enum)
- `ApprovalStatus` (was string, now enum)
- `ActionType` (was string, now enum)

### Preserved Functionality
- All existing UI components work without modification
- All database tables and columns remain the same
- All business logic is preserved
- File upload functionality maintained

## Future Enhancements

1. **Add CQRS Pattern** - Separate read and write operations
2. **Add MediatR** - For command/query handling
3. **Add FluentValidation** - For input validation
4. **Complete Authentication** - Implement JWT/OAuth providers
5. **Add AutoMapper** - For DTO mappings
6. **Add Specification Pattern** - For complex queries
7. **Add Event Sourcing** - For audit trail
8. **Add API Layer** - RESTful API endpoints

## Development Guidelines

### Adding a New Entity

1. Create entity in `Core/Domain/Entities/`
2. Create repository interface in `Core/Domain/Interfaces/`
3. Create EF configuration in `Infrastructure/Data/Configurations/`
4. Add DbSet to `CactusDbContext`
5. Create repository implementation in `Infrastructure/Repositories/`
6. Update `IUnitOfWork` and `UnitOfWork`
7. Register in `Program.cs`
8. Add migration: `dotnet ef migrations add YourMigrationName`

### Adding a New Service

1. Create service interface in `Core/Application/Interfaces/`
2. Create service implementation in `Core/Application/Services/`
3. Inject required repositories via `IUnitOfWork`
4. Register service in `Program.cs`
5. Inject service in your Blazor components

### Adding a New Feature

1. Define DTOs in `Core/Application/DTOs/`
2. Add methods to appropriate service interface
3. Implement business logic in service
4. Update UI components to use new service methods

## Testing Strategy

### Unit Tests
- Domain entities - business rules
- Application services - business logic with mocked repositories
- Repository implementations - with in-memory database

### Integration Tests
- Test full stack with test database
- Test UI components with test services

## Performance Considerations

- Use `AsNoTracking()` for read-only queries
- Implement caching for frequently accessed data
- Use pagination for large result sets
- Consider async/await for all I/O operations

## Security Considerations

- All database writes go through Unit of Work
- Audit logging for all critical operations
- Soft delete maintains data integrity
- Transaction support for data consistency

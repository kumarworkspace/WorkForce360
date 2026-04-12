# Cactus Application - Clean Architecture Diagram

## Layer Overview

```
┌─────────────────────────────────────────────────────────────┐
│                    PRESENTATION LAYER                        │
│  (Blazor Server - Components, Pages, Shared)                │
│                                                              │
│  • Policies.razor                                           │
│  • SignIn.razor                                             │
│  • Components (AddDocumentDialog, UploadDocumentSlide, etc.)│
└──────────────────────┬──────────────────────────────────────┘
                       │ Uses
                       ↓
┌─────────────────────────────────────────────────────────────┐
│              CORE - APPLICATION LAYER                        │
│         (Business Logic & Use Cases)                         │
│                                                              │
│  Interfaces:                  Services:                      │
│  • IDocumentService           • DocumentService             │
│  • IPolicyService             • PolicyService               │
│  • IAuthenticationService     • AuthenticationService       │
│  • IFileStorageService        • AcknowledgementService      │
│                                                              │
│  DTOs:                                                       │
│  • DocumentUploadRequest                                     │
│  • DocumentUploadResult                                      │
│  • AuthResult                                                │
└──────────────────────┬──────────────────────────────────────┘
                       │ Uses
                       ↓
┌─────────────────────────────────────────────────────────────┐
│               CORE - DOMAIN LAYER                            │
│         (Business Entities & Rules)                          │
│                                                              │
│  Entities:                    Value Objects:                 │
│  • Document                   • AuthorInfo                   │
│  • DocumentDetail             • PersonInfo                   │
│  • Policy                     • PeriodicRequestSettings      │
│  • AuditLog                                                  │
│  • SSO                        Enums:                         │
│  • AcknowledgementRequest     • PolicyStatus                 │
│  • AcknowledgementStatus      • DocumentStatus               │
│                               • ApprovalStatus               │
│  Interfaces:                  • ActionType                   │
│  • IRepository<T>                                            │
│  • IDocumentRepository                                       │
│  • IPolicyRepository                                         │
│  • IUnitOfWork                                               │
└──────────────────────┬──────────────────────────────────────┘
                       ↑ Implements
                       │
┌─────────────────────────────────────────────────────────────┐
│              INFRASTRUCTURE LAYER                            │
│    (Data Access, External Services, I/O)                     │
│                                                              │
│  Data:                        Repositories:                  │
│  • CactusDbContext            • Repository<T>               │
│  • Entity Configurations      • DocumentRepository          │
│    - PolicyConfiguration      • PolicyRepository            │
│    - DocumentConfiguration    • AuditLogRepository          │
│    - AuditLogConfiguration    • UnitOfWork                  │
│                                                              │
│  Services:                                                   │
│  • FileStorageService                                        │
│                                                              │
│  External:                                                   │
│  • SQL Server Database                                       │
│  • File System (uploads/)                                    │
└─────────────────────────────────────────────────────────────┘
```

## Dependency Flow

```
┌─────────────┐
│ Presentation│ ──depends on──> Application
└─────────────┘                      │
                                     │
                                     ↓
                            ┌──────────────┐
                            │    Domain    │
                            └──────────────┘
                                     ↑
                                     │
                            ┌────────┴────────┐
                            │ Infrastructure  │
                            │  (implements)   │
                            └─────────────────┘
```

## Request Flow Example: Document Upload

```
1. User uploads PDF in Blazor Component
   └─> UploadDocumentSlide.razor

2. Component calls Application Service
   └─> IDocumentService.UploadDocumentAsync(request)

3. DocumentService orchestrates operation
   ├─> IFileStorageService.SaveFileAsync()
   ├─> IUnitOfWork.BeginTransactionAsync()
   ├─> IUnitOfWork.Documents.AddAsync()
   ├─> IUnitOfWork.DocumentDetails.AddAsync()
   ├─> IUnitOfWork.Policies.AddAsync()
   ├─> IUnitOfWork.SaveChangesAsync()
   ├─> IUnitOfWork.CommitTransactionAsync()
   └─> IUnitOfWork.AuditLogs.AddAsync() (fire-and-forget)

4. Infrastructure executes
   ├─> FileStorageService → File System
   ├─> DocumentRepository → SQL Server
   ├─> DocumentDetailRepository → SQL Server
   ├─> PolicyRepository → SQL Server
   └─> AuditLogRepository → SQL Server

5. Response flows back
   └─> DocumentUploadResult → DocumentService → Component → User
```

## Folder Structure

```
Cactus/
│
├── Core/
│   ├── Domain/
│   │   ├── Entities/
│   │   │   ├── BaseEntity.cs
│   │   │   ├── Document.cs
│   │   │   ├── DocumentDetail.cs
│   │   │   ├── Policy.cs
│   │   │   ├── AuditLog.cs
│   │   │   ├── SSO.cs
│   │   │   ├── AcknowledgementRequest.cs
│   │   │   └── AcknowledgementStatusEntry.cs
│   │   │
│   │   ├── Enums/
│   │   │   ├── PolicyStatus.cs
│   │   │   ├── DocumentStatus.cs
│   │   │   ├── ApprovalStatus.cs
│   │   │   └── ActionType.cs
│   │   │
│   │   ├── ValueObjects/
│   │   │   ├── AuthorInfo.cs
│   │   │   ├── PersonInfo.cs
│   │   │   └── PeriodicRequestSettings.cs
│   │   │
│   │   └── Interfaces/
│   │       ├── IRepository.cs
│   │       ├── IDocumentRepository.cs
│   │       ├── IDocumentDetailRepository.cs
│   │       ├── IPolicyRepository.cs
│   │       ├── IAuditLogRepository.cs
│   │       └── IUnitOfWork.cs
│   │
│   └── Application/
│       ├── Interfaces/
│       │   ├── IDocumentService.cs
│       │   ├── IPolicyService.cs
│       │   ├── IAuthenticationService.cs
│       │   ├── IAcknowledgementRequestService.cs
│       │   ├── IAcknowledgementStatusService.cs
│       │   └── IFileStorageService.cs
│       │
│       ├── Services/
│       │   ├── DocumentService.cs
│       │   ├── PolicyService.cs
│       │   ├── AuthenticationService.cs
│       │   ├── AcknowledgementRequestService.cs
│       │   └── AcknowledgementStatusService.cs
│       │
│       └── DTOs/
│           ├── AuthResult.cs
│           ├── DocumentUploadRequest.cs
│           └── DocumentUploadResult.cs
│
├── Infrastructure/
│   ├── Data/
│   │   ├── CactusDbContext.cs
│   │   └── Configurations/
│   │       ├── PolicyConfiguration.cs
│   │       ├── DocumentConfiguration.cs
│   │       ├── DocumentDetailConfiguration.cs
│   │       ├── AuditLogConfiguration.cs
│   │       ├── SSOConfiguration.cs
│   │       ├── AcknowledgementRequestConfiguration.cs
│   │       └── AcknowledgementStatusConfiguration.cs
│   │
│   ├── Repositories/
│   │   ├── Repository.cs
│   │   ├── DocumentRepository.cs
│   │   ├── DocumentDetailRepository.cs
│   │   ├── PolicyRepository.cs
│   │   ├── AuditLogRepository.cs
│   │   └── UnitOfWork.cs
│   │
│   └── Services/
│       └── FileStorageService.cs
│
├── Components/          (Blazor UI Components)
├── Pages/              (Blazor Pages)
├── Shared/             (Layouts, Navigation)
├── wwwroot/            (Static files, uploads)
│
├── Program.cs          (DI Configuration)
├── appsettings.json    (Configuration)
│
└── Documentation/
    ├── CLEAN_ARCHITECTURE.md
    └── ARCHITECTURE_DIAGRAM.md
```

## Data Flow Patterns

### Read Operations (Query)
```
Component → Service → UnitOfWork → Repository → DbContext → Database
    ↓                                                            │
    ←──────────────────── Entity ←─────────────────────────────┘
```

### Write Operations (Command)
```
Component → Service → UnitOfWork.BeginTransaction()
                        ├→ Repository.AddAsync()
                        ├→ Repository.UpdateAsync()
                        ├→ UnitOfWork.SaveChangesAsync()
                        └→ UnitOfWork.CommitTransaction()
```

### Transaction Example
```
try {
    await UnitOfWork.BeginTransactionAsync();

    // Multiple operations
    await UnitOfWork.Documents.AddAsync(document);
    await UnitOfWork.SaveChangesAsync();

    await UnitOfWork.DocumentDetails.AddAsync(detail);
    await UnitOfWork.SaveChangesAsync();

    await UnitOfWork.Policies.AddAsync(policy);
    await UnitOfWork.SaveChangesAsync();

    // All or nothing
    await UnitOfWork.CommitTransactionAsync();
}
catch {
    await UnitOfWork.RollbackTransactionAsync();
}
```

## Benefits Visualization

```
┌──────────────────────────────────────────────────────────┐
│                   OLD ARCHITECTURE                        │
├──────────────────────────────────────────────────────────┤
│                                                           │
│  Component → Service → DbContext → Database              │
│              ↓                                            │
│         Direct EF queries                                 │
│         Mixed concerns                                    │
│         Hard to test                                      │
│         Tight coupling                                    │
│                                                           │
└──────────────────────────────────────────────────────────┘

                         VS

┌──────────────────────────────────────────────────────────┐
│                  CLEAN ARCHITECTURE                       │
├──────────────────────────────────────────────────────────┤
│                                                           │
│  Component → Service → Repository → DbContext → Database │
│              ↓           ↓                                │
│       Business Logic  Data Access                        │
│       Testable        Mockable                           │
│       Maintainable    Flexible                           │
│       Independent     Swappable                          │
│                                                           │
└──────────────────────────────────────────────────────────┘
```

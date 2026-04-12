# AG CACTUS - Policy Management System

## Application Status
✅ **Running at:** http://localhost:5000

## Authentication & Security

### SSO with Microsoft Entra ID
- Sign in with Microsoft account
- Group-based authorization
- OIDC protocol implementation
- Token validation on every request

### Authentication Guard
- **All pages require authentication** except:
  - `/signin` - Sign-in page
  - `/terms-of-service` - Terms page
  - `/privacy-policy` - Privacy page

- Unauthenticated access attempts are redirected to `/signin`
- All access attempts are logged in audit trail

### Audit Logging
**Every action is logged** including:
- Page access (successful and failed)
- User ID
- IP address
- Device information
- Timestamp
- Action details

## Configuration

### Azure AD (`appsettings.json`)
```json
{
  "AzureAd": {
    "TenantId": "aab8e9e4-c825-4e61-9019-03938582a655",
    "ClientId": "2f61ea65-c747-4c10-8734-cafc9a645948",
    "ClientSecret": "configured",
    "AllowedGroups": ["52b389ab-2078-4851-9098-544682c9f66d"]
  }
}
```

## Clean Code Structure

```
Infrastructure/
  └── Middleware/
      └── AuthenticationMiddleware.cs   # Guards all pages + audit logging

Core/
  ├── Application/                      # Business logic
  ├── Domain/                           # Entities & interfaces

Infrastructure/
  ├── Data/                             # Database context
  ├── Repositories/                     # Data access
  └── Services/                         # Infrastructure services
```

## Running

```bash
dotnet build && dotnet run
```

Visit: http://localhost:5000

---
© 2025 Aventra Group

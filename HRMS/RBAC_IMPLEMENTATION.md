# Role-Based Access Control (RBAC) Implementation

## Overview

This document describes the comprehensive RBAC system implemented for tenant-based access control with fine-grained permissions.

## Architecture

### Components

1. **Login Handler** (`Pages/LoginHandler.cshtml`)
   - Validates user existence, active status, and TenantId
   - Checks role assignment
   - Creates claims with UserId, TenantId, and Role

2. **RoleBasedAuthorizationMiddleware** (`Infrastructure/Middleware/RoleBasedAuthorizationMiddleware.cs`)
   - Runs after authentication middleware
   - Validates tenant and user permissions for each page request
   - Blocks unauthorized access with 403 status
   - Logs all access attempts (successful and denied)

3. **AuthorizationHelper** (`Core/Application/Services/AuthorizationHelper.cs`)
   - Maps page paths to module names
   - Checks user access levels for pages
   - Provides permission checking utilities

4. **RouteGuard Component** (`Components/RouteGuard.razor`)
   - Blazor component for protecting routes
   - Shows access denied message if user lacks permission
   - Can be wrapped around page content

5. **NavMenu** (`Shared/NavMenu.razor`)
   - Dynamically hides menu items based on user permissions
   - Only shows pages user has access to (access level > 0)

6. **TenantValidationService** (`Core/Application/Services/TenantValidationService.cs`)
   - Validates tenant access for users
   - Ensures tenant isolation in all operations
   - Throws UnauthorizedAccessException on tenant mismatch

## Access Levels

| Level | Name | Permissions |
|-------|------|-------------|
| 0 | No Access | Screen hidden, URL blocked |
| 1 | View Access | Read-only access |
| 2 | Modify Access | View + Create + Update |
| 3 | Full Access | View + Create + Update + Delete |

## Page to Module Mapping

The following pages are mapped to modules:

- `/hrms/employees` → "Employee Management"
- `/hrms/leave` → "Leave Management"
- `/tms/courses` → "Training Management"
- `/tms/planning` → "Training Management"
- `/admin/roles` → "Role Management"
- `/admin/access-control` → "Access Control"
- `/admin/user-roles` → "Access Control"
- `/admin/users` → "Access Control"
- `/admin/create-user` → "Access Control"
- `/dashboard` → "Dashboard" (public for authenticated users)

## User Story Implementation

### User Story 1: Role-Based Login with Tenant Validation ✅

**Implementation:**
- `LoginHandler.cshtml` validates:
  - User exists
  - User is active (`IsActive = true`)
  - TenantId is valid (`TenantId > 0`)
  - Role is assigned
- On failure, redirects with appropriate error message
- On success, creates claims and redirects to dashboard

**Validation Rules:**
- ❌ User without TenantId → login blocked
- ❌ User with invalid TenantId → login blocked
- ❌ Inactive user → login blocked
- ❌ User without role → login blocked
- ✅ Valid user + role + tenant → login success

### User Story 2: Role-Based Page Access Control ✅

**Implementation:**
- `RoleBasedAuthorizationMiddleware` checks every page request
- `NavMenu.razor` hides menu items based on permissions
- `RouteGuard.razor` component protects individual pages

**Features:**
- Menu items hidden if access = No Access (level 0)
- Direct URL access blocked with 403 Unauthorized
- Access denied page shown for unauthorized users

### User Story 3: Screen-Level Access (View / Modify / Full / No Access) ✅

**Implementation:**
- `AccessControlService` provides access level checking
- `AuthorizationHelper` maps pages to modules and checks access
- Access levels enforced at middleware and component level

**Features:**
- Menu visibility depends on AccessLevel
- API endpoints validate access before execution
- Unauthorized access returns 403 Forbidden
- No Access → module hidden entirely

### User Story 4: TenantId-Based Data Isolation ✅

**Implementation:**
- `TenantValidationService` validates tenant access
- All service methods filter by TenantId
- Cross-tenant access throws UnauthorizedAccessException

**Validation Rules:**
- ❌ Missing TenantId → reject request
- ❌ TenantId mismatch → reject request
- ✅ Valid TenantId → allow operation

**Example:**
```csharp
// In StaffService.GetByIdAsync
if (staff.TenantId != tenantId)
{
    throw new UnauthorizedAccessException("Access denied: Staff does not belong to your tenant.");
}
```

### User Story 5: Admin Role & Access Mapping Management ✅

**Implementation:**
- Role Management page (`/admin/roles`)
- Access Control page (`/admin/access-control`)
- User Role Mapping page (`/admin/user-roles`)
- All changes take effect immediately
- Audit logging for all role changes

**Features:**
- Assign roles to users
- Map screens to roles
- Configure access level per screen
- Tenant-specific role mapping
- Changes take effect immediately
- Audit log maintained for role changes
- Cannot assign cross-tenant roles

### User Story 6: Security & Audit Logging ✅

**Implementation:**
- Enhanced `AuthenticationMiddleware` logs all page access
- `RoleBasedAuthorizationMiddleware` logs access attempts
- All audit logs include:
  - UserId
  - Role
  - TenantId
  - Timestamp
  - IP Address
  - Action Type
  - Description

**Audit Logs Capture:**
- ✅ Login success / failure
- ✅ Unauthorized access attempts
- ✅ Role changes
- ✅ Tenant mismatch attempts
- ✅ CRUD actions with full context

## Usage Examples

### Protecting a Page with RouteGuard

```razor
@page "/protected-page"
@layout MainLayout

<RouteGuard RequiredAccessLevel="2"> <!-- Modify Access Required -->
    <MudContainer>
        <!-- Page content -->
    </MudContainer>
</RouteGuard>
```

### Checking Access in Code

```csharp
@inject IAuthorizationHelper AuthorizationHelper

@code {
    private async Task<bool> CanEdit()
    {
        var accessLevel = await AuthorizationHelper.GetPageAccessLevelAsync(_userId, "/hrms/employees", _tenantId);
        return accessLevel >= 2; // Modify Access or higher
    }
}
```

### Adding Tenant Validation to Service Methods

```csharp
public async Task<SomeDto> GetByIdAsync(int id, int tenantId, int userId)
{
    // Validate tenant access
    var user = await _unitOfWork.User.GetByIdAsync(userId);
    if (user == null || user.TenantId != tenantId)
    {
        throw new UnauthorizedAccessException("Access denied: Tenant mismatch.");
    }
    
    // Continue with operation...
}
```

## Security Best Practices

1. **Always validate TenantId** in service methods
2. **Never trust client-provided TenantId** - use from authenticated user claims
3. **Log all access attempts** for audit trail
4. **Use least privilege principle** - grant minimum required access
5. **Validate permissions at multiple layers** - middleware, components, and services

## Testing

To test the RBAC system:

1. **Login Validation:**
   - Try logging in with inactive user → should be blocked
   - Try logging in with user without TenantId → should be blocked

2. **Page Access:**
   - Access page without permission → should see 403 or access denied
   - Menu items should be hidden for pages without access

3. **Tenant Isolation:**
   - Try accessing data from different tenant → should be blocked
   - All queries should filter by TenantId

4. **Audit Logging:**
   - Check AuditLogs table for all access attempts
   - Verify login attempts are logged
   - Verify unauthorized access is logged

## Configuration

The RBAC system is automatically configured in `Program.cs`:

```csharp
// Register services
builder.Services.AddScoped<IAuthorizationHelper, AuthorizationHelper>();
builder.Services.AddScoped<ITenantValidationService, TenantValidationService>();

// Add middleware
app.UseMiddleware<RoleBasedAuthorizationMiddleware>();
```

## Future Enhancements

- [ ] API-level authorization attributes
- [ ] Button-level permission checks
- [ ] Dynamic permission caching
- [ ] Permission inheritance
- [ ] Time-based access control






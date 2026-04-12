# SSO Authentication with Microsoft Entra ID

## Overview
AG CACTUS uses Microsoft Entra ID (formerly Azure AD) for Single Sign-On authentication via OpenID Connect (OIDC) protocol.

## How It Works

### 1. User Access Flow
```
User visits app → Redirected to /signin → Clicks "Sign in with Microsoft"
    ↓
Microsoft login page → User enters credentials → MFA (if required)
    ↓
Token validation → Group membership check → Redirect to /policies
```

### 2. Authentication Components

#### **OIDC Configuration** ([Program.cs:27-68](Program.cs#L27-L68))
- Configured with Microsoft Identity Web
- Validates tokens from Microsoft
- Checks user group membership
- Handles authentication failures

#### **Authentication Middleware** ([Infrastructure/Middleware/AuthenticationMiddleware.cs](Infrastructure/Middleware/AuthenticationMiddleware.cs))
- Guards all protected pages
- Redirects unauthenticated users to `/signin`
- Logs all access attempts
- Captures user, IP, device info

## Configuration

### Azure AD Setup

1. **App Registration** (Azure Portal)
   - Create app registration in Entra ID
   - Note: Client ID, Tenant ID
   - Generate Client Secret

2. **API Permissions**
   - `openid` - OpenID Connect
   - `profile` - User profile
   - `email` - Email address
   - `User.Read` - Read user data
   - `GroupMember.Read.All` - Read group membership

3. **Token Configuration**
   - Add `groups` claim to tokens
   - Include group IDs in ID and Access tokens

4. **Security Groups**
   - Create security group in Azure AD
   - Add authorized users
   - Copy group Object ID

### Application Settings

**appsettings.json:**
```json
{
  "AzureAd": {
    "Instance": "https://login.microsoftonline.com/",
    "Domain": "aventragroup.com",
    "TenantId": "YOUR_TENANT_ID",
    "ClientId": "YOUR_CLIENT_ID",
    "ClientSecret": "YOUR_CLIENT_SECRET",
    "CallbackPath": "/signin-oidc",
    "SignedOutCallbackPath": "/signout-callback-oidc",
    "AllowedGroups": ["GROUP_OBJECT_ID"]
  }
}
```

## Group-Based Authorization

### How It Works
1. User signs in with Microsoft
2. Azure AD returns token with user's group memberships
3. Application validates token
4. Checks if user belongs to `AllowedGroups`
5. **Grant access** if user is in allowed group
6. **Deny access** if user is NOT in allowed group

### Implementation
```csharp
// In Program.cs OnTokenValidated event
var allowedGroups = Configuration["AzureAd:AllowedGroups"];
var userGroups = context.Principal.Claims
    .Where(c => c.Type == "groups")
    .Select(c => c.Value);

if (!userGroups.Any(g => allowedGroups.Contains(g)))
{
    context.Fail("User is not a member of the authorized group.");
}
```

## Page Protection

### Public Pages (No Auth Required)
- `/` - Redirects to signin
- `/signin` - Sign-in page
- `/terms-of-service` - Terms page
- `/privacy-policy` - Privacy page
- Static files (CSS, JS, images)

### Protected Pages (Auth Required)
- `/policies` - Main application
- **All other pages** require authentication

### Middleware Logic
```csharp
// Check if page is public
if (IsPublicPath(path))
{
    Allow access
}

// Check authentication
if (!User.IsAuthenticated)
{
    Log unauthorized attempt
    Redirect to /signin
}

// User is authenticated
Log successful access
Allow access
```

## Audit Logging

### What Gets Logged
**Every page access** is logged with:
- User ID (or "Anonymous")
- Action Type (`View`)
- Table Name (`PageAccess`)
- Record ID (page path)
- IP Address
- Device Info (User-Agent)
- Timestamp

### Logged Events
- **Unauthorized Access**: User tries to access protected page without auth
- **Successful Access**: Authenticated user accesses protected page
- **Failed Login**: Authentication fails
- **Access Denied**: User not in authorized group

### Database Table
```sql
AuditLogs (
    AuditId,
    UserId,
    ActionType,
    TableName,
    RecordId,
    ActionDetails,
    IPAddress,
    DeviceInfo,
    Timestamp
)
```

## Error Handling

### Authentication Failures
- **Scenario**: Microsoft authentication fails
- **Action**: Redirect to `/signin?error=authentication_failed`
- **Message**: "Authentication failed. Please try again."

### Access Denied
- **Scenario**: User not in authorized group
- **Action**: Redirect to `/signin?error=access_denied`
- **Message**: "Access denied. You are not authorized."

### Token Validation Fails
- **Scenario**: Invalid or expired token
- **Action**: Redirect to Microsoft login
- **Result**: User must re-authenticate

## Security Features

### 1. Token Validation
- Every request validates OIDC token
- Expired tokens trigger re-authentication
- Invalid tokens are rejected

### 2. Group Membership
- Checked during token validation
- Only users in `AllowedGroups` can access
- Group changes reflect immediately on next login

### 3. HTTPS
- Required for OIDC in production
- Localhost HTTP allowed for development

### 4. Session Management
- Cookies store authentication state
- Session timeout controlled by Azure AD
- Sign-out clears all cookies

## Testing

### Test Authorized User
1. Navigate to http://localhost:5000
2. Click "Sign in with Microsoft"
3. Sign in with account in authorized group
4. Should redirect to `/policies`

### Test Unauthorized User
1. Sign in with account NOT in group
2. Should see "Access denied" error
3. Redirected back to `/signin`

### Test Direct Page Access
1. Without signing in, visit `/policies`
2. Should redirect to `/signin`
3. Audit log records unauthorized attempt

## Troubleshooting

### "Redirect URI mismatch"
- **Fix**: Ensure redirect URI in Azure matches `/signin-oidc`

### "Groups claim not in token"
- **Fix**: Enable groups claim in Token Configuration
- Grant `GroupMember.Read.All` permission

### "Access denied" for valid user
- **Fix**: Verify user is in configured security group
- Check group Object ID in `appsettings.json`

### Pages not protected
- **Fix**: Ensure middleware is registered in `Program.cs`
- Check middleware is after `UseAuthentication()`

## Architecture Benefits

### Clean Code
- Single middleware handles all authentication
- Audit logging centralized
- Easy to add/remove protected pages

### Secure by Default
- All pages protected unless explicitly public
- Every access logged
- Group validation automatic

### Maintainable
- Configuration in `appsettings.json`
- No code changes for group updates
- Clear separation of concerns

## Summary

✅ **SSO**: Microsoft Entra ID via OIDC
✅ **Authorization**: Group-based access control
✅ **Protection**: All pages require authentication
✅ **Audit**: Every action logged
✅ **Clean**: Single middleware, centralized logic
✅ **Secure**: Token validation, HTTPS, group checks

---
© 2025 Aventra Group

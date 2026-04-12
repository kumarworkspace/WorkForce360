# Authentication Implementation

## Overview

The authentication system has been implemented following Clean Architecture principles with secure password hashing using PBKDF2 with SHA256.

## Features Implemented

### ✅ Email/Password Authentication
- Secure password hashing using PBKDF2 with SHA256
- 100,000 iterations for strong protection against brute force
- Salt-based hashing for each password
- Email validation
- "Remember me" functionality (UI ready, backend can be extended)

### 🔄 SSO Providers (UI Ready, Backend Pending)
- Google Sign-In (button available)
- Microsoft Sign-In (button available)
- Apple Sign-In (button available)

## Database Schema

### tbl_Users Table
```sql
CREATE TABLE tbl_Users
(
    UserId INT IDENTITY(1,1) PRIMARY KEY,
    Username NVARCHAR(50) NOT NULL UNIQUE,
    EmailId NVARCHAR(100) NOT NULL UNIQUE,
    PasswordHash NVARCHAR(255) NOT NULL,
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedOn DATETIME NOT NULL DEFAULT GETDATE(),
    CreatedBy NVARCHAR(50) NULL
);
```

## Architecture Components

### Domain Layer
- **Entities**: `User.cs` - User entity with properties matching database schema
- **Interfaces**: `IUserRepository.cs` - Repository interface for user operations

### Application Layer
- **Interfaces**:
  - `IAuthenticationService.cs` - Authentication service contract
  - `IPasswordHasher.cs` - Password hashing contract
- **Services**: `AuthenticationService.cs` - Implements authentication logic
- **DTOs**: `AuthResult.cs` - Authentication result response

### Infrastructure Layer
- **Repositories**: `UserRepository.cs` - User data access implementation
- **Services**:
  - `PasswordHasher.cs` - PBKDF2-based password hashing
  - Implements IPasswordHasher interface
- **Configurations**: `UserConfiguration.cs` - EF Core entity configuration

### Presentation Layer
- **Pages**: `SignIn.razor` - Login page with email/password and SSO options

## Security Features

### Password Hashing
- **Algorithm**: PBKDF2 with SHA256
- **Salt Size**: 16 bytes (128 bits)
- **Key Size**: 32 bytes (256 bits)
- **Iterations**: 100,000
- **Format**: `{Base64Salt}:{Base64Hash}`

### Security Best Practices Implemented
1. ✅ Passwords are never stored in plain text
2. ✅ Each password has a unique salt
3. ✅ High iteration count for brute-force protection
4. ✅ Constant-time comparison to prevent timing attacks
5. ✅ Generic error messages to prevent username enumeration
6. ✅ Logging of authentication attempts

## How to Use

### 1. Create Test Users

Run the SQL script `CreateTestUser.sql` to create a test user:

```sql
-- Test User Credentials:
-- Email: testuser@aventragroup.com
-- Password: Test@123
```

### 2. Testing Login

1. Navigate to http://localhost:5000/signin
2. Enter email: `testuser@aventragroup.com`
3. Enter password: `Test@123`
4. Click "Sign In"
5. On successful login, you'll be redirected to the home page (`/`)

### 3. Creating Additional Users

#### Option A: Using SQL with Pre-generated Hash

You can use the C# code below to generate password hashes:

```csharp
using System.Security.Cryptography;

public static string HashPassword(string password)
{
    const int SaltSize = 16;
    const int KeySize = 32;
    const int Iterations = 100000;

    var salt = RandomNumberGenerator.GetBytes(SaltSize);
    var hash = Rfc2898DeriveBytes.Pbkdf2(
        password,
        salt,
        Iterations,
        HashAlgorithmName.SHA256,
        KeySize
    );

    return $"{Convert.ToBase64String(salt)}:{Convert.ToBase64String(hash)}";
}
```

Then insert into database:

```sql
INSERT INTO tbl_Users (Username, EmailId, PasswordHash, IsActive, CreatedOn, CreatedBy)
VALUES ('username', 'user@company.com', 'HASH_HERE', 1, GETDATE(), 'system');
```

#### Option B: Create a User Registration Page

You can create a `/signup` page that uses the `IPasswordHasher` service to hash passwords before storing them.

## API Reference

### IAuthenticationService

```csharp
public interface IAuthenticationService
{
    // Email/Password authentication
    Task<AuthResult> SignInWithEmailAsync(string email, string password, bool rememberMe);

    // SSO authentication (to be implemented)
    Task<AuthResult> SignInWithGoogleAsync();
    Task<AuthResult> SignInWithMicrosoftAsync();
    Task<AuthResult> SignInWithAppleAsync();

    // Session management
    Task SignOutAsync();
    Task<bool> IsAuthenticatedAsync();
}
```

### AuthResult

```csharp
public class AuthResult
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public string? UserId { get; set; }
    public string? Token { get; set; }         // For JWT implementation
    public string? RefreshToken { get; set; }  // For JWT implementation
    public DateTime? ExpiresAt { get; set; }   // For JWT implementation
}
```

## Error Messages

The system returns generic error messages to prevent username enumeration:

- **Invalid credentials**: "Invalid email or password"
- **Missing input**: "Email and password are required"
- **System error**: "An error occurred during sign in"

## Logging

Authentication attempts are logged with appropriate severity:

- **Info**: Successful authentication
- **Warning**: Failed login attempts
- **Error**: System errors during authentication

Example log entries:
```
[INFO] User successfully authenticated: user@company.com
[WARNING] Login attempt with non-existent email: unknown@company.com
[WARNING] Failed login attempt for user: user@company.com
```

## Future Enhancements

### Short Term
1. **Session Management**: Implement persistent sessions
2. **JWT Tokens**: Add JWT token generation for API authentication
3. **Password Reset**: Implement "Forgot Password" functionality
4. **Account Lockout**: Lock accounts after multiple failed attempts
5. **Two-Factor Authentication**: Add 2FA support

### Long Term
1. **SSO Integration**:
   - Google OAuth 2.0
   - Microsoft Azure AD
   - Apple Sign In
2. **Audit Trail**: Track all authentication events
3. **Password Policies**: Enforce password complexity rules
4. **User Registration**: Self-service user registration with email verification

## Testing

### Manual Testing Checklist

- [ ] Login with valid credentials succeeds
- [ ] Login with invalid email fails with appropriate message
- [ ] Login with valid email but wrong password fails
- [ ] Empty email/password shows validation errors
- [ ] Invalid email format shows validation error
- [ ] Password visibility toggle works
- [ ] "Remember me" checkbox works
- [ ] Successful login redirects to home page
- [ ] Failed login shows error message
- [ ] Loading state shows during authentication

### Security Testing

- [ ] Passwords are never visible in network requests
- [ ] Password hashes are not exposed in API responses
- [ ] Failed login attempts are logged
- [ ] Error messages don't reveal if email exists
- [ ] Timing attacks are mitigated with constant-time comparison

## Troubleshooting

### Common Issues

**Issue**: "Invalid email or password" for valid credentials
- **Solution**: Verify the password hash in database was generated correctly
- **Check**: Run the test user SQL script again

**Issue**: Application crashes on login
- **Solution**: Check that all services are registered in `Program.cs`
- **Verify**: UserRepository and PasswordHasher are registered

**Issue**: Database connection error
- **Solution**: Verify connection string in `appsettings.json`
- **Check**: Database server is running and accessible

## Dependencies

- **.NET 9.0**: Core framework
- **Entity Framework Core 9.0**: Database access
- **SQL Server**: Database
- **MudBlazor**: UI components
- **System.Security.Cryptography**: Password hashing

## Configuration

No additional configuration needed. The authentication system uses:
- Database connection from `appsettings.json`
- Services registered in `Program.cs`
- No external API keys required for email/password auth

## Migration Notes

If you have existing users with different password hashing:
1. Create a migration script to rehash passwords
2. Or implement a hybrid verification system
3. Or force password reset for all users

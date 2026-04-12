using HRMS.Core.Application.DTOs;
using HRMS.Core.Application.Interfaces;
using HRMS.Core.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace HRMS.Core.Application.Services;

public class AuthenticationService : Interfaces.IAuthenticationService
{
    private readonly ILogger<AuthenticationService> _logger;
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;

    public AuthenticationService(
        ILogger<AuthenticationService> logger,
        IUserRepository userRepository,
        IPasswordHasher passwordHasher)
    {
        _logger = logger;
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
    }

    public async Task<AuthResult> SignInWithEmailAsync(string email, string password, bool rememberMe)
    {
        try
        {
            // Validate input
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                return new AuthResult
                {
                    Success = false,
                    Message = "Email and password are required"
                };
            }

            // Get user by email
            var user = await _userRepository.GetByEmailAsync(email);
            if (user == null)
            {
                _logger.LogWarning("Login attempt with non-existent email: {Email}", email);
                return new AuthResult
                {
                    Success = false,
                    Message = "Invalid email"
                };
            }

            // Verify password
            if (!_passwordHasher.VerifyPassword(password, user.PasswordHash))
            {
                _logger.LogWarning("Failed login attempt for user: {Email}", email);
                return new AuthResult
                {
                    Success = false,
                    Message = "Invalid email or password"
                };
            }

            // Validate user is active
            if (!user.IsActive)
            {
                _logger.LogWarning("Inactive user attempted login: {Email}", email);
                return new AuthResult
                {
                    Success = false,
                    Message = "Your account is inactive. Please contact your administrator."
                };
            }

            // Validate TenantId exists
            if (user.TenantId <= 0)
            {
                _logger.LogWarning("User without TenantId attempted login: {Email}", email);
                return new AuthResult
                {
                    Success = false,
                    Message = "Invalid tenant configuration. Please contact your administrator."
                };
            }

            // Validate user has a role assigned (check UserRoles table)
            // Note: This is a basic check - full role validation happens after login
            if (string.IsNullOrEmpty(user.Role))
            {
                _logger.LogWarning("User without role attempted login: {Email}", email);
                return new AuthResult
                {
                    Success = false,
                    Message = "No role assigned. Please contact your administrator."
                };
            }

            // Successful authentication
            _logger.LogInformation("User successfully authenticated: {Email}, TenantId: {TenantId}, Role: {Role}", 
                email, user.TenantId, user.Role);
            return new AuthResult
            {
                Success = true,
                Message = "Login successful",
                UserId = user.UserId.ToString(),
                // TODO: Generate JWT token if needed
                Token = null,
                RefreshToken = null,
                ExpiresAt = null
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during sign in for email: {Email}", email);
            return new AuthResult
            {
                Success = false,
                Message = "An error occurred during sign in"
            };
        }
    }

    public async Task<AuthResult> SignInWithGoogleAsync()
    {
        // TODO: Implement Google OAuth
        await Task.CompletedTask;
        return new AuthResult
        {
            Success = false,
            Message = "Google authentication not implemented yet"
        };
    }

    public async Task<AuthResult> SignInWithMicrosoftAsync()
    {
        // TODO: Implement Microsoft OAuth
        await Task.CompletedTask;
        return new AuthResult
        {
            Success = false,
            Message = "Microsoft authentication not implemented yet"
        };
    }

    public async Task<AuthResult> SignInWithAppleAsync()
    {
        // TODO: Implement Apple OAuth
        await Task.CompletedTask;
        return new AuthResult
        {
            Success = false,
            Message = "Apple authentication not implemented yet"
        };
    }

    public Task SignOutAsync()
    {
        // Signout is handled by Logout.cshtml page
        _logger.LogInformation("SignOutAsync called - redirecting to /logout");
        return Task.CompletedTask;
    }

    public async Task<bool> IsAuthenticatedAsync()
    {
        // TODO: Implement authentication check
        await Task.CompletedTask;
        return false;
    }
}

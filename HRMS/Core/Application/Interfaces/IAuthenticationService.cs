using HRMS.Core.Application.DTOs;

namespace HRMS.Core.Application.Interfaces;

public interface IAuthenticationService
{
    Task<AuthResult> SignInWithEmailAsync(string email, string password, bool rememberMe);
    Task<AuthResult> SignInWithGoogleAsync();
    Task<AuthResult> SignInWithMicrosoftAsync();
    Task<AuthResult> SignInWithAppleAsync();
    Task SignOutAsync();
    Task<bool> IsAuthenticatedAsync();
}

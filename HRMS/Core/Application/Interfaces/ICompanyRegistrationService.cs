using HRMS.Core.Application.DTOs;

namespace HRMS.Core.Application.Interfaces;

public interface ICompanyRegistrationService
{
    Task<RegistrationResult> RegisterCompanyAsync(CompanyRegistrationRequest request);
    Task<bool> VerifyEmailAsync(string token);
    Task<bool> ResendVerificationEmailAsync(string email);
}









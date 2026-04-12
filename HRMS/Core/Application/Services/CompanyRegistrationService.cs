using HRMS.Core.Application.DTOs;
using HRMS.Core.Application.Interfaces;
using HRMS.Core.Domain.Entities;
using HRMS.Core.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;

namespace HRMS.Core.Application.Services;

public class CompanyRegistrationService : ICompanyRegistrationService
{
    private readonly ILogger<CompanyRegistrationService> _logger;
    private readonly ITenantRepository _tenantRepository;
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IConfiguration _configuration;
    private readonly ISeedingService _seedingService;

    public CompanyRegistrationService(
        ILogger<CompanyRegistrationService> logger,
        ITenantRepository tenantRepository,
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        IUnitOfWork unitOfWork,
        IConfiguration configuration,
        ISeedingService seedingService)
    {
        _logger = logger;
        _tenantRepository = tenantRepository;
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _unitOfWork = unitOfWork;
        _configuration = configuration;
        _seedingService = seedingService;
    }

    public async Task<RegistrationResult> RegisterCompanyAsync(CompanyRegistrationRequest request)
    {
        try
        {
            // Validate company name is unique
            if (await _tenantRepository.TenantNameExistsAsync(request.CompanyName))
            {
                return new RegistrationResult
                {
                    Success = false,
                    Message = $"Company name '{request.CompanyName}' is already taken. Please choose another name."
                };
            }

            // Validate email is unique globally (since we check at registration)
            if (await _userRepository.EmailExistsAsync(request.Email))
            {
                return new RegistrationResult
                {
                    Success = false,
                    Message = $"Email '{request.Email}' is already registered. Please use a different email address."
                };
            }

            // Create tenant
            var tenant = new Tenant
            {
                CompanyName = request.CompanyName,
                Domain = request.Domain,
                IsActive = true,
                IsLocked = false,
                CreatedDate = DateTime.UtcNow
            };

            await _tenantRepository.AddAsync(tenant);
            await _unitOfWork.SaveChangesAsync(); // Save to get TenantId

            // Create user for company admin
            var passwordHash = _passwordHasher.HashPassword(request.Password);

            var user = new User
            {
                TenantId = tenant.TenantId,
                FullName = request.FullName,
                Email = request.Email,
                PasswordHash = passwordHash,
                LoginProvider = string.IsNullOrEmpty(passwordHash) ? "Email" : null,
                Role = request.Role,
                IsEmailVerified = false,
                FailedLoginAttempts = 0,
                IsActive = true,
                IsLocked = false,
                CreatedDate = DateTime.UtcNow
            };

            await _userRepository.AddAsync(user);
            await _unitOfWork.SaveChangesAsync();

            // Seed default roles and permissions for the tenant
            try
            {
                await _seedingService.SeedDefaultRolesAndPermissionsAsync(tenant.TenantId, user.UserId);
                _logger.LogInformation("Default roles and permissions seeded for tenant: {TenantId}", tenant.TenantId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error seeding default roles and permissions for tenant: {TenantId}. Continuing with registration.", tenant.TenantId);
                // Don't fail registration if seeding fails - can be done manually later
            }

            // Assign default "Super Admin" role to the newly created user
            try
            {
                // Get the "Super Admin" role for this tenant
                var allRoles = await _unitOfWork.Role.GetByTenantIdAsync(tenant.TenantId, includeInactive: true);
                var superAdminRole = allRoles.FirstOrDefault(r => r.RoleName.Equals("Super Admin", StringComparison.OrdinalIgnoreCase));
                
                if (superAdminRole != null)
                {
                    // Create UserRole entry
                    var userRole = new UserRole
                    {
                        UserId = user.UserId,
                        RoleId = superAdminRole.RoleId,
                        TenantId = tenant.TenantId,
                        IsActive = true,
                        CreatedBy = user.UserId,
                        CreatedDate = DateTime.UtcNow
                    };

                    await _unitOfWork.UserRole.AddAsync(userRole);
                    
                    // Update user's Role field to "Super Admin"
                    user.Role = "Super Admin";
                    await _userRepository.UpdateAsync(user);
                    
                    await _unitOfWork.SaveChangesAsync();
                    
                    _logger.LogInformation("Assigned Super Admin role to user: {Email} (UserId: {UserId}) for tenant: {TenantId}", 
                        user.Email, user.UserId, tenant.TenantId);
                }
                else
                {
                    _logger.LogWarning("Super Admin role not found for tenant: {TenantId}. User role assignment skipped.", tenant.TenantId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error assigning default role to user: {Email} (UserId: {UserId}) for tenant: {TenantId}. Registration will continue but user may need manual role assignment.", 
                    user.Email, user.UserId, tenant.TenantId);
                // Don't fail registration if role assignment fails - can be done manually later
            }

            // Generate email verification token
            var verificationToken = GenerateVerificationToken(user.UserId, user.Email);

            // TODO: Send verification email
            // await SendVerificationEmailAsync(user.Email, user.FullName, verificationToken);

            _logger.LogInformation("Company registered: {CompanyName} (TenantId: {TenantId}), User: {Email} (UserId: {UserId})", 
                tenant.CompanyName, tenant.TenantId, user.Email, user.UserId);

            return new RegistrationResult
            {
                Success = true,
                Message = "Company registered successfully. Please check your email to verify your account.",
                TenantId = tenant.TenantId,
                UserId = user.UserId
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error registering company: {CompanyName}", request.CompanyName);
            return new RegistrationResult
            {
                Success = false,
                Message = $"An error occurred during registration: {ex.Message}"
            };
        }
    }

    public async Task<bool> VerifyEmailAsync(string token)
    {
        try
        {
            // Decode token to get userId and email
            var decoded = DecodeVerificationToken(token);
            if (decoded == null)
                return false;

            var (userId, email) = decoded.Value;
            var user = await _userRepository.GetByIdAsync(userId);

            if (user == null || user.Email != email)
                return false;

            if (user.IsEmailVerified)
                return true; // Already verified

            user.IsEmailVerified = true;
            await _userRepository.UpdateAsync(user);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Email verified for user: {Email} (UserId: {UserId})", email, userId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error verifying email with token");
            return false;
        }
    }

    public async Task<bool> ResendVerificationEmailAsync(string email)
    {
        try
        {
            var user = await _userRepository.GetByEmailAsync(email);
            if (user == null)
                return false;

            if (user.IsEmailVerified)
                return false; // Already verified

            var verificationToken = GenerateVerificationToken(user.UserId, user.Email);
            // TODO: Send verification email
            // await SendVerificationEmailAsync(user.Email, user.FullName, verificationToken);

            _logger.LogInformation("Verification email resent to: {Email}", email);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resending verification email to: {Email}", email);
            return false;
        }
    }

    private string GenerateVerificationToken(int userId, string email)
    {
        var data = $"{userId}:{email}:{DateTime.UtcNow.AddDays(7):O}"; // 7 days expiry
        var bytes = Encoding.UTF8.GetBytes(data);
        var encoded = Convert.ToBase64String(bytes);
        // Simple encoding - in production, use proper encryption
        return encoded.Replace('+', '-').Replace('/', '_').TrimEnd('=');
    }

    private (int userId, string email)? DecodeVerificationToken(string token)
    {
        try
        {
            var decoded = token.Replace('-', '+').Replace('_', '/');
            switch (decoded.Length % 4)
            {
                case 2: decoded += "=="; break;
                case 3: decoded += "="; break;
            }
            var bytes = Convert.FromBase64String(decoded);
            var data = Encoding.UTF8.GetString(bytes);
            var parts = data.Split(':');
            
            if (parts.Length != 3 || !int.TryParse(parts[0], out var userId))
                return null;

            var expiry = DateTime.Parse(parts[2]);
            if (expiry < DateTime.UtcNow)
                return null; // Token expired

            return (userId, parts[1]);
        }
        catch
        {
            return null;
        }
    }
}


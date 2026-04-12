using HRMS.Core.Application.DTOs;
using HRMS.Core.Application.Interfaces;
using HRMS.Core.Domain.Entities;
using HRMS.Core.Domain.Enums;
using HRMS.Core.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;

namespace HRMS.Core.Application.Services;

public class UserService : IUserService
{
    private readonly ILogger<UserService> _logger;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IUserRoleService _userRoleService;
    private static readonly Regex PasswordRegex = new(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$", RegexOptions.Compiled);

    public UserService(
        ILogger<UserService> logger,
        IUnitOfWork unitOfWork,
        IPasswordHasher passwordHasher,
        IUserRoleService userRoleService)
    {
        _logger = logger;
        _unitOfWork = unitOfWork;
        _passwordHasher = passwordHasher;
        _userRoleService = userRoleService;
    }

    public async Task<CreateUserResult> CreateUserAsync(CreateUserRequest request, int tenantId, int adminUserId, string? ipAddress = null)
    {
        try
        {
            // 1. Validate staff exists and belongs to tenant
            var staff = await _unitOfWork.Staff.GetByIdWithDetailsAsync(request.StaffId, tenantId);
            if (staff == null || !staff.IsActive)
            {
                return new CreateUserResult
                {
                    Success = false,
                    Message = "Staff not found or does not belong to your tenant."
                };
            }

            // 2. Check if user account already exists for this email
            if (await EmailExistsAsync(request.Email, tenantId))
            {
                return new CreateUserResult
                {
                    Success = false,
                    Message = $"Email '{request.Email}' already exists for this tenant."
                };
            }

            // 3. Validate password
            if (!request.AutoGeneratePassword)
            {
                if (string.IsNullOrWhiteSpace(request.Password))
                {
                    return new CreateUserResult
                    {
                        Success = false,
                        Message = "Password is required."
                    };
                }

                if (!PasswordRegex.IsMatch(request.Password))
                {
                    return new CreateUserResult
                    {
                        Success = false,
                        Message = "Password must be at least 8 characters and contain uppercase, lowercase, number, and special character."
                    };
                }
            }

            // 4. Validate role
            var validRoles = new[] { "Super Admin", "HR Admin", "Staff", "Trainer", "Manager", "IT Admin" };
            if (!validRoles.Contains(request.Role))
            {
                return new CreateUserResult
                {
                    Success = false,
                    Message = $"Invalid role. Must be one of: {string.Join(", ", validRoles)}"
                };
            }

            // 5. Generate password if auto-generate is enabled
            string password = request.AutoGeneratePassword 
                ? GenerateRandomPassword() 
                : request.Password;

            // 6. Create user account
            var passwordHash = _passwordHasher.HashPassword(password);
            var user = new User
            {
                TenantId = tenantId,
                FullName = staff.Name ?? string.Empty,
                Email = request.Email,
                PasswordHash = passwordHash,
                LoginProvider = "Email",
                Role = request.Role,
                IsEmailVerified = true, // Default = 1
                FailedLoginAttempts = 0, // Default = 0
                IsActive = true, // Default = 1
                IsLocked = false, // Default = 0
                StaffId = request.StaffId, // Link to Staff
                CreatedDate = DateTime.UtcNow,
                CreatedBy = adminUserId // Database uses int, not string
            };

            await _unitOfWork.User.AddAsync(user);
            
            try
            {
                await _unitOfWork.SaveChangesAsync();
            }
            catch (Microsoft.EntityFrameworkCore.DbUpdateException dbEx)
            {
                // Log the inner exception for more details
                var innerEx = dbEx.InnerException;
                var errorDetails = innerEx?.Message ?? dbEx.Message;
                _logger.LogError(dbEx, "Database error creating user: {ErrorDetails}", errorDetails);
                
                // Check for specific constraint violations
                if (errorDetails.Contains("FOREIGN KEY") || errorDetails.Contains("StaffId"))
                {
                    return new CreateUserResult
                    {
                        Success = false,
                        Message = $"Invalid staff ID. Please ensure the staff member exists and is active."
                    };
                }
                
                if (errorDetails.Contains("UNIQUE") || errorDetails.Contains("Email"))
                {
                    return new CreateUserResult
                    {
                        Success = false,
                        Message = $"Email '{request.Email}' already exists for this tenant."
                    };
                }
                
                return new CreateUserResult
                {
                    Success = false,
                    Message = $"Database error: {errorDetails}"
                };
            }

            // 7. Assign default Staff role permission
            try
            {
                var roles = await _unitOfWork.Role.GetByTenantIdAsync(tenantId);
                var staffRole = roles.FirstOrDefault(r => r.RoleName == "Staff" && r.IsActive);
                
                if (staffRole != null)
                {
                    var assignRequest = new AssignUserRolesRequest
                    {
                        UserId = user.UserId,
                        RoleIds = new List<int> { staffRole.RoleId }
                    };
                    await _userRoleService.AssignRolesAsync(assignRequest, tenantId, adminUserId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to assign default Staff role to user {UserId}. User created but role assignment failed.", user.UserId);
                // Don't fail user creation if role assignment fails
            }

            _logger.LogInformation("User account created: {Email} (UserId: {UserId}) for staff {StaffId} by admin {AdminUserId}", 
                user.Email, user.UserId, request.StaffId, adminUserId);

            // 8. Log audit entry
            try
            {
                await LogAuditAsync(tenantId, adminUserId, user.UserId, ActionType.Create, 
                    $"User account created for staff {request.StaffId} (Email: {user.Email}, Role: {request.Role})", ipAddress);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to log audit entry for user creation");
                // Don't fail user creation if audit logging fails
            }

            return new CreateUserResult
            {
                Success = true,
                Message = request.AutoGeneratePassword 
                    ? $"User account created successfully. Password: {password}" 
                    : "User account created successfully.",
                UserId = user.UserId
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating user account for staff {StaffId} in tenant {TenantId}", request.StaffId, tenantId);
            return new CreateUserResult
            {
                Success = false,
                Message = $"Error creating user account: {ex.Message}"
            };
        }
    }

    public async Task<bool> EmailExistsAsync(string email, int tenantId, int? excludeUserId = null)
    {
        // Check if email exists for active users only (IsActive = 1)
        return await _unitOfWork.User.EmailExistsInTenantAsync(email, tenantId, excludeUserId);
    }

    public async Task<List<StaffListDto>> GetStaffWithoutUserAccountsAsync(int tenantId)
    {
        try
        {
            // Get all staff for tenant
            var allStaff = await _unitOfWork.Staff.GetByTenantIdAsync(tenantId);
            
            // Get all users for tenant
            var allUsers = await _unitOfWork.User.GetAllUsers();
            var tenantUserEmails = allUsers
                .Where(u => u.TenantId == tenantId && u.IsActive)
                .Select(u => u.Email.ToLower().Trim())
                .ToHashSet();

            // Filter staff without user accounts
            var staffWithoutUsers = allStaff
                .Where(s => s.IsActive && 
                           !string.IsNullOrEmpty(s.Email) &&
                           !tenantUserEmails.Contains(s.Email.ToLower().Trim()))
                .Select(s => new StaffListDto
                {
                    StaffId = s.StaffId,
                    EmployeeCode = s.EmployeeCode ?? string.Empty,
                    Name = s.Name ?? string.Empty,
                    Email = s.Email ?? string.Empty,
                    Division = s.Division ?? string.Empty,
                    Department = s.Department ?? string.Empty,
                    Position = s.Position ?? string.Empty,
                    DateJoined = s.DateJoined,
                    Photo = s.Photo,
                    IsActive = s.IsActive
                })
                .OrderBy(s => s.Name)
                .ToList();

            return staffWithoutUsers;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting staff without user accounts for tenant {TenantId}", tenantId);
            return new List<StaffListDto>();
        }
    }

    public async Task<PagedResult<UserListDto>> GetPagedAsync(int tenantId, int pageNumber, int pageSize, string? searchTerm = null, string? role = null)
    {
        try
        {
            var allUsers = await _unitOfWork.User.GetAllUsers();
            var tenantUsers = allUsers
                .Where(u => u.TenantId == tenantId && u.IsActive) // Only show active users
                .AsQueryable();

            // Apply search filter
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var searchLower = searchTerm.ToLower();
                tenantUsers = tenantUsers.Where(u =>
                    (u.FullName != null && u.FullName.ToLower().Contains(searchLower)) ||
                    (u.Email != null && u.Email.ToLower().Contains(searchLower))
                );
            }

            // Apply role filter
            if (!string.IsNullOrWhiteSpace(role))
            {
                tenantUsers = tenantUsers.Where(u => u.Role == role);
            }

            var totalCount = tenantUsers.Count();

            // Get staff data for mapping
            var allStaff = await _unitOfWork.Staff.GetByTenantIdAsync(tenantId);
            var staffDict = allStaff.ToDictionary(s => s.StaffId, s => s);

            // Apply pagination and map to DTO
            var users = tenantUsers
                .OrderByDescending(u => u.CreatedDate)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var userDtos = users.Select(u =>
            {
                var staff = u.StaffId.HasValue && staffDict.ContainsKey(u.StaffId.Value)
                    ? staffDict[u.StaffId.Value]
                    : null;

                return new UserListDto
                {
                    UserId = u.UserId,
                    FullName = u.FullName,
                    Email = u.Email,
                    Role = u.Role,
                    IsEmailVerified = u.IsEmailVerified,
                    IsActive = u.IsActive,
                    IsLocked = u.IsLocked,
                    FailedLoginAttempts = u.FailedLoginAttempts,
                    StaffId = u.StaffId,
                    StaffName = staff?.Name,
                    StaffDepartment = staff?.Department,
                    CreatedDate = u.CreatedDate,
                    UpdatedDate = u.UpdatedDate
                };
            }).ToList();

            return new PagedResult<UserListDto>
            {
                Items = userDtos,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting paged users for tenant {TenantId}", tenantId);
            throw;
        }
    }

    private string GenerateRandomPassword()
    {
        const string lowercase = "abcdefghijklmnopqrstuvwxyz";
        const string uppercase = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        const string numbers = "0123456789";
        const string special = "@$!%*?&";
        const string all = lowercase + uppercase + numbers + special;

        var random = new Random();
        var password = new char[12];

        // Ensure at least one of each type
        password[0] = lowercase[random.Next(lowercase.Length)];
        password[1] = uppercase[random.Next(uppercase.Length)];
        password[2] = numbers[random.Next(numbers.Length)];
        password[3] = special[random.Next(special.Length)];

        // Fill the rest randomly
        for (int i = 4; i < password.Length; i++)
        {
            password[i] = all[random.Next(all.Length)];
        }

        // Shuffle
        for (int i = password.Length - 1; i > 0; i--)
        {
            int j = random.Next(i + 1);
            (password[i], password[j]) = (password[j], password[i]);
        }

        return new string(password);
    }

    public async Task<UserListDto?> GetByIdAsync(int userId, int tenantId)
    {
        try
        {
            var user = await _unitOfWork.User.GetByIdAsync(userId);
            if (user == null || user.TenantId != tenantId || !user.IsActive)
            {
                return null;
            }

            var staff = user.StaffId.HasValue 
                ? await _unitOfWork.Staff.GetByIdAsync(user.StaffId.Value)
                : null;

            return new UserListDto
            {
                UserId = user.UserId,
                FullName = user.FullName,
                Email = user.Email,
                Role = user.Role,
                IsEmailVerified = user.IsEmailVerified,
                IsActive = user.IsActive,
                IsLocked = user.IsLocked,
                FailedLoginAttempts = user.FailedLoginAttempts,
                StaffId = user.StaffId,
                StaffName = staff?.Name,
                StaffDepartment = staff?.Department,
                CreatedDate = user.CreatedDate,
                UpdatedDate = user.UpdatedDate
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user by ID: {UserId} for tenant: {TenantId}", userId, tenantId);
            throw;
        }
    }

    public async Task<bool> UpdateAsync(UpdateUserRequest request, int tenantId, int adminUserId, string? ipAddress = null)
    {
        try
        {
            var user = await _unitOfWork.User.GetByIdAsync(request.UserId);
            if (user == null || user.TenantId != tenantId)
            {
                throw new InvalidOperationException("User not found or access denied.");
            }

            // Validate email uniqueness (exclude current user)
            if (await EmailExistsAsync(request.Email, tenantId, request.UserId))
            {
                throw new InvalidOperationException($"Email '{request.Email}' already exists for another user.");
            }

            // Validate role
            var validRoles = new[] { "Super Admin", "HR Admin", "Staff", "Trainer", "Manager", "IT Admin" };
            if (!validRoles.Contains(request.Role))
            {
                throw new InvalidOperationException($"Invalid role. Must be one of: {string.Join(", ", validRoles)}");
            }

            user.Email = request.Email;
            user.Role = request.Role;
            user.IsActive = request.IsActive;
            user.UpdatedBy = adminUserId;
            user.UpdatedDate = DateTime.UtcNow;

            await _unitOfWork.User.UpdateAsync(user);
            await _unitOfWork.SaveChangesAsync();

            // Log audit
            await LogAuditAsync(tenantId, adminUserId, user.UserId, ActionType.Update,
                $"Updated user account (ID: {user.UserId}, Email: {user.Email}, Role: {user.Role})", ipAddress);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user: {UserId} for tenant: {TenantId}", request.UserId, tenantId);
            throw;
        }
    }

    public async Task<bool> DeleteAsync(int userId, int tenantId, int adminUserId, string? ipAddress = null)
    {
        try
        {
            var user = await _unitOfWork.User.GetByIdAsync(userId);
            if (user == null || user.TenantId != tenantId || !user.IsActive)
            {
                throw new InvalidOperationException("User not found or access denied.");
            }

            // Soft delete user
            user.IsActive = false;
            user.UpdatedBy = adminUserId;
            user.UpdatedDate = DateTime.UtcNow;

            await _unitOfWork.User.UpdateAsync(user);
            await _unitOfWork.SaveChangesAsync();

            // Log audit
            await LogAuditAsync(tenantId, adminUserId, user.UserId, ActionType.Delete,
                $"Deleted user account (ID: {userId}, Email: {user.Email})", ipAddress);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting user: {UserId} for tenant: {TenantId}", userId, tenantId);
            throw;
        }
    }

    private async Task LogAuditAsync(int tenantId, int adminUserId, int? targetUserId, ActionType actionType, string description, string? ipAddress)
    {
        try
        {
            var auditLog = new AuditLog
            {
                TenantId = tenantId,
                UserId = adminUserId,
                ActionType = actionType,
                Description = description,
                IPAddress = ipAddress,
                IsActive = true,
                CreatedBy = adminUserId,
                CreatedDate = DateTime.UtcNow
            };

            await _unitOfWork.AuditLogs.AddAsync(auditLog);
            await _unitOfWork.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to log audit entry");
            // Don't throw - audit logging failure shouldn't break the operation
        }
    }
}


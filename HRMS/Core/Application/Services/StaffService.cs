using HRMS.Core.Application.DTOs;
using HRMS.Core.Application.Interfaces;
using HRMS.Core.Domain.Entities;
using HRMS.Core.Domain.Enums;
using HRMS.Core.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace HRMS.Core.Application.Services;

public class StaffService : IStaffService
{
    private readonly ILogger<StaffService> _logger;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IFileStorageService _fileStorageService;
    private static readonly Regex EmailRegex = new(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.IgnoreCase | RegexOptions.Compiled);

    public StaffService(
        ILogger<StaffService> logger,
        IUnitOfWork unitOfWork,
        IFileStorageService fileStorageService)
    {
        _logger = logger;
        _unitOfWork = unitOfWork;
        _fileStorageService = fileStorageService;
    }

    public async Task<StaffDto?> GetByIdAsync(int staffId, int tenantId)
    {
        try
        {
            // Tenant validation: Ensure staff belongs to the requested tenant
            var staff = await _unitOfWork.Staff.GetByIdWithDetailsAsync(staffId, tenantId);
            if (staff == null) return null;

            // Additional tenant validation
            if (staff.TenantId != tenantId)
            {
                _logger.LogWarning("Tenant mismatch: Staff {StaffId} belongs to tenant {StaffTenantId}, requested tenant {RequestedTenantId}", 
                    staffId, staff.TenantId, tenantId);
                throw new UnauthorizedAccessException("Access denied: Staff does not belong to your tenant.");
            }

            return await MapToDtoAsync(staff, tenantId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting staff by ID: {StaffId} for tenant: {TenantId}", staffId, tenantId);
            throw;
        }
    }

    public async Task<PagedResult<StaffListDto>> GetPagedAsync(int tenantId, int pageNumber, int pageSize, string? searchTerm, string? division, string? department)
    {
        try
        {
            var allStaff = await _unitOfWork.Staff.SearchAsync(tenantId, searchTerm, division, department);
            var totalCount = allStaff.Count();

            var pagedStaff = allStaff
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var items = pagedStaff.Select(s => new StaffListDto
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
            }).ToList();

            return new PagedResult<StaffListDto>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting paged staff for tenant: {TenantId}", tenantId);
            throw;
        }
    }

    public async Task<StaffDto> CreateAsync(CreateStaffRequest request, int tenantId, int userId, string? ipAddress)
    {
        ValidateRequest(request);

        int createdStaffId = 0;

        await _unitOfWork.ExecuteInTransactionAsync(async () =>
        {
            // Normalize email for validation (ValidateRequest ensures Email is not null)
            var normalizedEmail = request.Email.ToLower().Trim();
            
            // Validate email uniqueness
            if (await _unitOfWork.Staff.EmailExistsAsync(normalizedEmail, tenantId))
            {
                throw new InvalidOperationException($"Email '{request.Email}' already exists for this tenant.");
            }

            // Validate dropdown values exist in MasterDropdown
            await ValidateDropdownValuesAsync(request.Division, request.Department, request.Position, tenantId);

            // Generate EmployeeCode if not provided (based on tenant)
            var employeeCode = string.IsNullOrWhiteSpace(request.EmployeeCode) 
                ? await GenerateEmployeeCodeAsync(tenantId) 
                : request.EmployeeCode;

            var staff = new Staff
            {
                TenantId = tenantId,
                EmployeeCode = employeeCode,
                Name = request.Name,
                Company = request.Company,
                DateOfBirth = request.DateOfBirth,
                GenderId = request.GenderId,
                Email = request.Email.ToLower().Trim(),
                PhoneNumber = request.PhoneNumber,
                Address = request.Address,
                IdentityCard = request.IdentityCard,
                Division = request.Division,
                Department = request.Department,
                Position = request.Position,
                EmploymentStatusId = request.EmploymentStatusId,
                DateJoined = request.DateJoined,
                RetirementDate = request.RetirementDate,
                ReportingManagerId = request.ReportingManagerId,
                IsActive = true,
                CreatedBy = userId.ToString(),
                CreatedDate = DateTime.Now
            };

            await _unitOfWork.Staff.AddAsync(staff);
            await _unitOfWork.SaveChangesAsync();
            createdStaffId = staff.StaffId;

            // Add Education Details (only if they have required data)
            foreach (var edu in request.EducationDetails)
            {
                // Skip empty/incomplete education details
                if (!edu.StartDate.HasValue || !edu.EndDate.HasValue)
                {
                    // Only validate if there's some data entered (institution or qualification)
                    if (!string.IsNullOrWhiteSpace(edu.Institution) || !string.IsNullOrWhiteSpace(edu.Qualification))
                    {
                        throw new ArgumentException("Education Start Date and End Date are required when Institution or Qualification is provided.");
                    }
                    // Skip this entry if it's completely empty
                    continue;
                }

                if (edu.StartDate.Value > edu.EndDate.Value)
                {
                    throw new ArgumentException("Education Start Date must be before or equal to End Date.");
                }

                var educationDetail = new EducationDetail
                {
                    TenantId = tenantId,
                    StaffId = staff.StaffId,
                    StartDate = edu.StartDate.Value,
                    EndDate = edu.EndDate.Value,
                    Institution = edu.Institution ?? string.Empty,
                    Qualification = edu.Qualification ?? string.Empty,
                    YearOfPassing = edu.YearOfPassing,
                    GradeOrPercentage = edu.GradeOrPercentage,
                    IsActive = true,
                    CreatedBy = userId.ToString(),
                    CreatedDate = DateTime.Now
                };
                await _unitOfWork.EducationDetail.AddAsync(educationDetail);
            }

            // Add Experience Details (only if they have required data)
            foreach (var exp in request.ExperienceDetails)
            {
                // Skip empty/incomplete experience details
                if (!exp.StartDate.HasValue || !exp.EndDate.HasValue)
                {
                    // Only validate if there's some data entered (company or position)
                    if (!string.IsNullOrWhiteSpace(exp.Company) || !string.IsNullOrWhiteSpace(exp.Position))
                    {
                        throw new ArgumentException("Experience Start Date and End Date are required when Company or Position is provided.");
                    }
                    // Skip this entry if it's completely empty
                    continue;
                }

                if (exp.StartDate.Value > exp.EndDate.Value)
                {
                    throw new ArgumentException("Experience Start Date must be before or equal to End Date.");
                }

                // Calculate total experience
                var totalDays = (exp.EndDate.Value - exp.StartDate.Value).Days;
                var years = totalDays / 365;
                var months = (totalDays % 365) / 30;
                var totalExperience = $"{years} years, {months} months";

                var experienceDetail = new ExperienceDetail
                {
                    TenantId = tenantId,
                    StaffId = staff.StaffId,
                    StartDate = exp.StartDate.Value,
                    EndDate = exp.EndDate.Value,
                    Company = exp.Company,
                    Position = exp.Position ?? string.Empty,
                    TotalExperience = exp.TotalExperience ?? totalExperience,
                    IsActive = true,
                    CreatedBy = userId.ToString(),
                    CreatedDate = DateTime.Now
                };
                await _unitOfWork.ExperienceDetail.AddAsync(experienceDetail);
            }

            await _unitOfWork.SaveChangesAsync();

            // Log audit
            await LogAuditAsync(tenantId, userId, ActionType.Create, $"Created staff: {staff.Name} (ID: {staff.StaffId})", ipAddress);
        });

        // Get the created staff with details after transaction
        var createdStaff = await _unitOfWork.Staff.GetByIdWithDetailsAsync(createdStaffId, tenantId);
        return await MapToDtoAsync(createdStaff!, tenantId);
    }

    public async Task<StaffDto> UpdateAsync(UpdateStaffRequest request, int tenantId, int userId, string? ipAddress)
    {
        try
        {
            ValidateRequest(request);

            await _unitOfWork.ExecuteInTransactionAsync(async () =>
            {
            var staff = await _unitOfWork.Staff.GetByIdAsync(request.StaffId);
                if (staff == null || staff.TenantId != tenantId || !staff.IsActive)
                {
                    throw new InvalidOperationException("Staff not found or access denied.");
                }

                // Normalize email for validation (ValidateRequest ensures Email is not null)
                var normalizedEmail = request.Email.ToLower().Trim();
                
                // Validate email uniqueness (exclude current staff)
                if (await _unitOfWork.Staff.EmailExistsAsync(normalizedEmail, tenantId, request.StaffId))
                {
                    throw new InvalidOperationException($"Email '{request.Email}' already exists for another staff.");
                }

                // Validate dropdown values
                await ValidateDropdownValuesAsync(request.Division, request.Department, request.Position, tenantId);

                // Update staff properties
                if (!string.IsNullOrWhiteSpace(request.EmployeeCode))
                {
                    staff.EmployeeCode = request.EmployeeCode;
                }
                staff.Name = request.Name;
                staff.Company = request.Company;
                staff.DateOfBirth = request.DateOfBirth;
                staff.GenderId = request.GenderId;
                staff.Email = request.Email.ToLower().Trim();
                staff.PhoneNumber = request.PhoneNumber;
                staff.Address = request.Address;
                staff.IdentityCard = request.IdentityCard;
                staff.Division = request.Division;
                staff.Department = request.Department;
                staff.Position = request.Position;
                staff.EmploymentStatusId = request.EmploymentStatusId ?? staff.EmploymentStatusId;
                staff.DateJoined = request.DateJoined;
                staff.RetirementDate = request.RetirementDate;
                staff.ReportingManagerId = request.ReportingManagerId;
                staff.UpdatedBy = userId.ToString();
                staff.UpdatedDate = DateTime.Now;

                await _unitOfWork.Staff.UpdateAsync(staff);

                // Soft delete existing education details
                var existingEducation = await _unitOfWork.EducationDetail.GetByStaffIdAsync(request.StaffId, tenantId);
                foreach (var edu in existingEducation)
                {
                    edu.IsActive = false;
                    edu.UpdatedBy = userId.ToString();
                    edu.UpdatedDate = DateTime.Now;
                    await _unitOfWork.EducationDetail.UpdateAsync(edu);
                }

                // Add new education details (only if they have required data)
                foreach (var edu in request.EducationDetails)
                {
                    // Skip empty/incomplete education details
                    if (!edu.StartDate.HasValue || !edu.EndDate.HasValue)
                    {
                        // Only validate if there's some data entered (institution or qualification)
                        if (!string.IsNullOrWhiteSpace(edu.Institution) || !string.IsNullOrWhiteSpace(edu.Qualification))
                        {
                            throw new ArgumentException("Education Start Date and End Date are required when Institution or Qualification is provided.");
                        }
                        // Skip this entry if it's completely empty
                        continue;
                    }

                    if (edu.StartDate.Value > edu.EndDate.Value)
                    {
                        throw new ArgumentException("Education Start Date must be before or equal to End Date.");
                    }

                    var educationDetail = new EducationDetail
                    {
                        TenantId = tenantId,
                        StaffId = staff.StaffId,
                        StartDate = edu.StartDate.Value,
                        EndDate = edu.EndDate.Value,
                        Institution = edu.Institution ?? string.Empty,
                        Qualification = edu.Qualification ?? string.Empty,
                        YearOfPassing = edu.YearOfPassing,
                        GradeOrPercentage = edu.GradeOrPercentage,
                        IsActive = true,
                        CreatedBy = userId.ToString(),
                        CreatedDate = DateTime.Now
                    };
                    await _unitOfWork.EducationDetail.AddAsync(educationDetail);
                }

                // Soft delete existing experience details
                var existingExperience = await _unitOfWork.ExperienceDetail.GetByStaffIdAsync(request.StaffId, tenantId);
                foreach (var exp in existingExperience)
                {
                    exp.IsActive = false;
                    exp.UpdatedBy = userId.ToString();
                    exp.UpdatedDate = DateTime.Now;
                    await _unitOfWork.ExperienceDetail.UpdateAsync(exp);
                }

                // Add new experience details (only if they have required data)
                foreach (var exp in request.ExperienceDetails)
                {
                    // Skip empty/incomplete experience details
                    if (!exp.StartDate.HasValue || !exp.EndDate.HasValue)
                    {
                        // Only validate if there's some data entered (company or position)
                        if (!string.IsNullOrWhiteSpace(exp.Company) || !string.IsNullOrWhiteSpace(exp.Position))
                        {
                            throw new ArgumentException("Experience Start Date and End Date are required when Company or Position is provided.");
                        }
                        // Skip this entry if it's completely empty
                        continue;
                    }

                    if (exp.StartDate.Value > exp.EndDate.Value)
                    {
                        throw new ArgumentException("Experience Start Date must be before or equal to End Date.");
                    }

                    // Calculate total experience
                    var totalDays = (exp.EndDate.Value - exp.StartDate.Value).Days;
                    var years = totalDays / 365;
                    var months = (totalDays % 365) / 30;
                    var totalExperience = $"{years} years, {months} months";

                    var experienceDetail = new ExperienceDetail
                    {
                        TenantId = tenantId,
                        StaffId = staff.StaffId,
                        StartDate = exp.StartDate.Value,
                        EndDate = exp.EndDate.Value,
                        Company = exp.Company,
                        Position = exp.Position ?? string.Empty,
                        TotalExperience = exp.TotalExperience ?? totalExperience,
                        IsActive = true,
                        CreatedBy = userId.ToString(),
                        CreatedDate = DateTime.Now
                    };
                    await _unitOfWork.ExperienceDetail.AddAsync(experienceDetail);
                }

                await _unitOfWork.SaveChangesAsync();

                // Log audit
                await LogAuditAsync(tenantId, userId, ActionType.Update, $"Updated staff: {staff.Name} (ID: {staff.StaffId})", ipAddress);
            });

            var updatedStaff = await _unitOfWork.Staff.GetByIdWithDetailsAsync(request.StaffId, tenantId);
            return await MapToDtoAsync(updatedStaff!, tenantId);
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Database error updating staff: {StaffId} for tenant: {TenantId}. Inner exception: {InnerException}", 
                request.StaffId, tenantId, ex.InnerException?.Message);
            
            // Extract the actual error message from the inner exception
            var errorMessage = ex.InnerException?.Message ?? ex.Message;
            if (errorMessage != null && errorMessage.Contains("See the inner exception"))
            {
                errorMessage = ex.InnerException?.InnerException?.Message ?? errorMessage;
            }
            
            throw new InvalidOperationException($"Error saving employee: {errorMessage}", ex);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Validation error updating staff: {StaffId}", request.StaffId);
            throw; // Re-throw validation errors as-is
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating staff: {StaffId} for tenant: {TenantId}", request.StaffId, tenantId);
            throw;
        }
    }

    public async Task<bool> DeleteAsync(int staffId, int tenantId, int userId, string? ipAddress)
    {
        try
        {
            await _unitOfWork.ExecuteInTransactionAsync(async () =>
            {
                var staff = await _unitOfWork.Staff.GetByIdAsync(staffId);
                if (staff == null || staff.TenantId != tenantId || !staff.IsActive)
                {
                    throw new InvalidOperationException("Staff not found or access denied.");
                }

                // Soft delete staff
                staff.IsActive = false;
                staff.UpdatedBy = userId.ToString();
                staff.UpdatedDate = DateTime.Now;
                await _unitOfWork.Staff.UpdateAsync(staff);

                // Soft delete related records
                var educationDetails = await _unitOfWork.EducationDetail.GetByStaffIdAsync(staffId, tenantId);
                foreach (var edu in educationDetails)
                {
                    edu.IsActive = false;
                    edu.UpdatedBy = userId.ToString();
                    edu.UpdatedDate = DateTime.Now;
                    await _unitOfWork.EducationDetail.UpdateAsync(edu);
                }

                var experienceDetails = await _unitOfWork.ExperienceDetail.GetByStaffIdAsync(staffId, tenantId);
                foreach (var exp in experienceDetails)
                {
                    exp.IsActive = false;
                    exp.UpdatedBy = userId.ToString();
                    exp.UpdatedDate = DateTime.Now;
                    await _unitOfWork.ExperienceDetail.UpdateAsync(exp);
                }

                var documents = await _unitOfWork.LegalDocument.GetByStaffIdAsync(staffId, tenantId);
                foreach (var doc in documents)
                {
                    doc.IsActive = false;
                    doc.UpdatedBy = userId.ToString();
                    doc.UpdatedDate = DateTime.Now;
                    await _unitOfWork.LegalDocument.UpdateAsync(doc);
                }

                await _unitOfWork.SaveChangesAsync();

                // Log audit
                await LogAuditAsync(tenantId, userId, ActionType.Delete, $"Deleted staff: {staff.Name} (ID: {staffId})", ipAddress);
            });

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting staff: {StaffId} for tenant: {TenantId}", staffId, tenantId);
            throw;
        }
    }

    public async Task<string> UploadPhotoAsync(int staffId, int tenantId, Stream fileStream, string fileName, int userId)
    {
        try
        {
            var staff = await _unitOfWork.Staff.GetByIdAsync(staffId);
            if (staff == null || staff.TenantId != tenantId || !staff.IsActive)
            {
                throw new InvalidOperationException("Staff not found or access denied.");
            }

            var filePath = await _fileStorageService.SaveStaffPhotoAsync(tenantId, staffId, fileStream, fileName);
            
            staff.Photo = filePath;
            staff.UpdatedBy = userId.ToString();
            staff.UpdatedDate = DateTime.Now;
            await _unitOfWork.Staff.UpdateAsync(staff);
            await _unitOfWork.SaveChangesAsync();

            return filePath;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading photo for staff: {StaffId}", staffId);
            throw;
        }
    }

    public async Task<string> UploadDocumentAsync(int staffId, int tenantId, string documentType, Stream fileStream, string fileName, int userId)
    {
        try
        {
            var staff = await _unitOfWork.Staff.GetByIdAsync(staffId);
            if (staff == null || staff.TenantId != tenantId || !staff.IsActive)
            {
                throw new InvalidOperationException("Staff not found or access denied.");
            }

            // Validate document type exists in MasterDropdown
            var docType = await _unitOfWork.MasterDropdown.GetByCategoryAndNameAsync("DocumentType", documentType, tenantId);
            if (docType == null)
            {
                throw new InvalidOperationException($"Document type '{documentType}' not found in MasterDropdown.");
            }

            var filePath = await _fileStorageService.SaveLegalDocumentAsync(tenantId, staffId, fileStream, fileName);
            
            var document = new LegalDocument
            {
                TenantId = tenantId,
                StaffId = staffId,
                DocumentType = documentType,
                FileName = fileName,
                IsActive = true,
                CreatedBy = userId.ToString(),
                CreatedDate = DateTime.Now
            };

            await _unitOfWork.LegalDocument.AddAsync(document);
            await _unitOfWork.SaveChangesAsync();

            return filePath;
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Database error uploading document for staff: {StaffId} for tenant: {TenantId}. Inner exception: {InnerException}",
                staffId, tenantId, ex.InnerException?.Message);

            var errorMessage = ex.InnerException?.Message ?? ex.Message;
            if (errorMessage != null && errorMessage.Contains("See the inner exception"))
            {
                errorMessage = ex.InnerException?.InnerException?.Message ?? errorMessage;
            }

            throw new InvalidOperationException($"Error uploading document: {errorMessage}", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading document for staff: {StaffId}", staffId);
            throw;
        }
    }

    public async Task<bool> DeleteDocumentAsync(int documentId, int tenantId, int userId)
    {
        try
        {
            var document = await _unitOfWork.LegalDocument.GetByIdAsync(documentId);
            if (document == null || document.TenantId != tenantId || !document.IsActive)
            {
                return false;
            }

            document.IsActive = false;
            document.UpdatedBy = userId.ToString();
            document.UpdatedDate = DateTime.Now;
            await _unitOfWork.LegalDocument.UpdateAsync(document);
            await _unitOfWork.SaveChangesAsync();

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting document: {DocumentId}", documentId);
            throw;
        }
    }

    private void ValidateRequest(CreateStaffRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            throw new ArgumentException("Name is required.");

        if (string.IsNullOrWhiteSpace(request.Email))
            throw new ArgumentException("Email is required.");

        if (!EmailRegex.IsMatch(request.Email))
            throw new ArgumentException("Invalid email format.");

        if (string.IsNullOrWhiteSpace(request.Division))
            throw new ArgumentException("Division is required.");

        if (string.IsNullOrWhiteSpace(request.Department))
            throw new ArgumentException("Department is required.");

        if (string.IsNullOrWhiteSpace(request.Position))
            throw new ArgumentException("Position is required.");

        if (request.DateOfBirth.HasValue && request.DateJoined.HasValue && request.DateOfBirth >= request.DateJoined)
            throw new ArgumentException("Date of Birth must be before Date Joined.");

        if (request.DateJoined.HasValue && request.RetirementDate.HasValue && request.DateJoined >= request.RetirementDate)
            throw new ArgumentException("Date Joined must be before Retirement Date.");
    }

    private async Task ValidateDropdownValuesAsync(string division, string department, string position, int tenantId)
    {
        var divisions = await _unitOfWork.MasterDropdown.GetByCategoryAsync("Division", tenantId);
        if (!divisions.Any(d => d.Name == division))
            throw new ArgumentException($"Division '{division}' not found in MasterDropdown.");

        var departments = await _unitOfWork.MasterDropdown.GetByCategoryAsync("Department", tenantId);
        if (!departments.Any(d => d.Name == department))
            throw new ArgumentException($"Department '{department}' not found in MasterDropdown.");

        var positions = await _unitOfWork.MasterDropdown.GetByCategoryAsync("Position", tenantId);
        if (!positions.Any(p => p.Name == position))
            throw new ArgumentException($"Position '{position}' not found in MasterDropdown.");
    }

    private async Task<StaffDto> MapToDtoAsync(Staff staff, int tenantId)
    {
        string? genderName = null;
        string? employmentStatusName = null;
        
        if (staff.GenderId.HasValue)
        {
            try
            {
                var gender = await _unitOfWork.MasterDropdown.GetByIdAsync(staff.GenderId.Value);
                if (gender != null && gender.TenantId == tenantId && gender.IsActive)
                {
                    genderName = gender.Name;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error loading gender for staff {StaffId}, GenderId: {GenderId}", staff.StaffId, staff.GenderId);
                // Continue without gender name
            }
        }
        
        if (staff.EmploymentStatusId.HasValue)
        {
            try
            {
                var status = await _unitOfWork.MasterDropdown.GetByIdAsync(staff.EmploymentStatusId.Value);
                if (status != null && status.TenantId == tenantId && status.IsActive)
                {
                    employmentStatusName = status.Name;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error loading employment status for staff {StaffId}, EmploymentStatusId: {EmploymentStatusId}", staff.StaffId, staff.EmploymentStatusId);
                // Continue without employment status name
            }
        }
        
        // Get reporting manager name
        string? reportingManagerName = null;
        if (staff.ReportingManagerId.HasValue)
        {
            try
            {
                var manager = await _unitOfWork.Staff.GetByIdAsync(staff.ReportingManagerId.Value);
                if (manager != null && manager.TenantId == tenantId && manager.IsActive)
                {
                    reportingManagerName = manager.Name;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error loading reporting manager for staff {StaffId}, ReportingManagerId: {ReportingManagerId}", staff.StaffId, staff.ReportingManagerId);
                // Continue without reporting manager name
            }
        }

        return new StaffDto
        {
            StaffId = staff.StaffId,
            EmployeeCode = staff.EmployeeCode ?? string.Empty,
            Name = staff.Name ?? string.Empty,
            Company = staff.Company,
            DateOfBirth = staff.DateOfBirth,
            GenderId = staff.GenderId,
            GenderName = genderName,
            Email = staff.Email ?? string.Empty,
            PhoneNumber = staff.PhoneNumber,
            Address = staff.Address,
            IdentityCard = staff.IdentityCard,
            Division = staff.Division ?? string.Empty,
            Department = staff.Department ?? string.Empty,
            Position = staff.Position ?? string.Empty,
            EmploymentStatusId = staff.EmploymentStatusId,
            EmploymentStatusName = employmentStatusName,
            DateJoined = staff.DateJoined,
            RetirementDate = staff.RetirementDate,
            Photo = staff.Photo,
            IsActive = staff.IsActive,
            TenantId = tenantId,
            ReportingManagerId = staff.ReportingManagerId,
            ReportingManagerName = reportingManagerName,
            EducationDetails = (staff.EducationDetails ?? new List<EducationDetail>())
                .Where(e => e.IsActive)
                .Select(e => new EducationDetailDto
                {
                    Id = e.Id,
                    StartDate = e.StartDate,
                    EndDate = e.EndDate,
                    Institution = e.Institution,
                    Qualification = e.Qualification,
                    YearOfPassing = e.YearOfPassing,
                    GradeOrPercentage = e.GradeOrPercentage
                }).ToList(),
            ExperienceDetails = (staff.ExperienceDetails ?? new List<ExperienceDetail>())
                .Where(e => e.IsActive)
                .Select(e => new ExperienceDetailDto
                {
                    Id = e.Id,
                    StartDate = e.StartDate,
                    EndDate = e.EndDate,
                    Company = e.Company,
                    Position = e.Position,
                    TotalExperience = e.TotalExperience
                }).ToList(),
            LegalDocuments = (staff.LegalDocuments ?? new List<LegalDocument>())
                .Where(d => d.IsActive)
                .Select(d => new LegalDocumentDto
                {
                    Id = d.Id,
                    DocumentType = d.DocumentType ?? string.Empty,
                    FileName = d.FileName ?? string.Empty
                }).ToList()
        };
    }

    public async Task LogViewAsync(int staffId, int tenantId, int userId, string? ipAddress)
    {
        try
        {
            var staff = await _unitOfWork.Staff.GetByIdAsync(staffId);
            if (staff != null && staff.TenantId == tenantId)
            {
                await LogAuditAsync(tenantId, userId, ActionType.View, $"Viewed staff: {staff.Name} (ID: {staffId})", ipAddress);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error logging view for staff: {StaffId}", staffId);
            // Don't throw - view logging is non-critical
        }
    }

    public async Task<StaffDto?> GetLastEmployeeCodeAsync(int tenantId)
    {
        try
        {
            var lastStaff = await _unitOfWork.Staff.GetLastByTenantAsync(tenantId);
            if (lastStaff != null)
            {
                return await MapToDtoAsync(lastStaff, tenantId);
            }
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting last employee code for tenant: {TenantId}", tenantId);
            return null;
        }
    }
    
    private async Task<string> GenerateEmployeeCodeAsync(int tenantId)
    {
        try
        {
            // Get the last employee code for this tenant
            var lastStaff = await _unitOfWork.Staff.GetLastByTenantAsync(tenantId);
            int nextNumber = 1;
            
            if (lastStaff != null && !string.IsNullOrEmpty(lastStaff.EmployeeCode))
            {
                // Extract number from employee code (format: EMP001001, EMP001002, etc.)
                var codeParts = lastStaff.EmployeeCode.Where(char.IsDigit).ToArray();
                if (codeParts.Length > 0)
                {
                    // Get the last 4 digits (sequential number)
                    var last4Digits = codeParts.Length >= 4 
                        ? new string(codeParts.Skip(codeParts.Length - 4).Take(4).ToArray())
                        : new string(codeParts);
                    
                    if (int.TryParse(last4Digits, out int lastNumber))
                    {
                        nextNumber = lastNumber + 1;
                    }
                }
            }
            
            // Format: EMP + TenantId (3 digits) + Sequential Number (4 digits) (e.g., EMP0010001, EMP0010002)
            return $"EMP{tenantId:D3}{nextNumber:D4}";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating employee code for tenant: {TenantId}", tenantId);
            // Fallback to timestamp-based code
            return $"EMP{tenantId:D3}{DateTime.Now:yyyyMMddHHmmss}";
        }
    }

    private async Task LogAuditAsync(int tenantId, int userId, ActionType actionType, string description, string? ipAddress)
    {
        try
        {
            var auditLog = new AuditLog
            {
                TenantId = tenantId,
                UserId = userId,
                ActionType = actionType,
                Description = description,
                IPAddress = ipAddress,
                IsActive = true,
                CreatedBy = userId,
                CreatedDate = DateTime.Now
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

    #region Stored Procedure Methods

    public async Task<PagedResult<StaffListSpDto>> GetStaffListSpAsync(GetStaffListRequest request)
    {
        try
        {
            var (items, totalCount) = await _unitOfWork.Staff.GetStaffListSpAsync(request);

            return new PagedResult<StaffListSpDto>
            {
                Items = items.ToList(),
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting staff list from stored procedure for tenant: {TenantId}", request.TenantId);
            throw;
        }
    }

    public async Task<StaffDto?> GetByUserIdAsync(int userId)
    {
        try
        {
            var user = await _unitOfWork.User.GetByIdAsync(userId);
            if (user == null || !user.StaffId.HasValue)
                return null;

            return await GetByIdAsync(user.StaffId.Value, user.TenantId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting staff by user ID: {UserId}", userId);
            throw;
        }
    }

    #endregion
}


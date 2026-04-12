using HRMS.Core.Application.DTOs;
using HRMS.Core.Application.Interfaces;
using HRMS.Core.Domain.Entities;
using HRMS.Core.Domain.Enums;
using HRMS.Core.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace HRMS.Core.Application.Services;

public class LeaveOtService : ILeaveOtService
{
    private readonly ILogger<LeaveOtService> _logger;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILeaveBalanceService? _leaveBalanceService;
    private readonly IEmailService? _emailService;
    private readonly IFileStorageService? _fileStorageService;

    public LeaveOtService(ILogger<LeaveOtService> logger, IUnitOfWork unitOfWork,
        ILeaveBalanceService? leaveBalanceService = null, IEmailService? emailService = null, IFileStorageService? fileStorageService = null)
    {
        _logger = logger;
        _unitOfWork = unitOfWork;
        _leaveBalanceService = leaveBalanceService;
        _emailService = emailService;
        _fileStorageService = fileStorageService;
    }

    public async Task<LeaveOtRequestDto> CreateRequestAsync(CreateLeaveOtRequestDto request, int tenantId, int userId, string? ipAddress)
    {
        try
        {
            // Validate staff exists and belongs to tenant
            var staff = await _unitOfWork.Staff.GetByIdWithDetailsAsync(request.StaffId, tenantId);
            if (staff == null || !staff.IsActive)
            {
                throw new InvalidOperationException("Staff not found or inactive.");
            }

            // Validate dates
            if (request.FromDate > request.ToDate)
            {
                throw new ArgumentException("From Date cannot be after To Date.");
            }

            if (request.FromDate < DateTime.Today)
            {
                throw new ArgumentException("Cannot select past dates.");
            }

            // Validate reporting manager
            if (!request.ReportingManagerId.HasValue)
            {
                throw new ArgumentException("Reporting Manager is required.");
            }

            var reportingManager = await _unitOfWork.Staff.GetByIdAsync(request.ReportingManagerId.Value);
            if (reportingManager == null || reportingManager.TenantId != tenantId || !reportingManager.IsActive)
            {
                throw new InvalidOperationException("Invalid Reporting Manager.");
            }

            // Get RequestType to determine if it's Leave or OT
            var requestType = await _unitOfWork.MasterDropdown.FindAsync(m => 
                m.TenantId == tenantId && 
                m.Category == "RequestType" && 
                m.Id == request.RequestTypeId && 
                m.IsActive);
            var requestTypeEntity = requestType.FirstOrDefault();
            var isLeaveRequest = requestTypeEntity?.Code?.ToUpper() == "LEAVE" || 
                               (requestTypeEntity == null && request.RequestTypeId == 1); // Fallback for backward compatibility

            // Calculate days/hours
            decimal? totalDays = null;
            decimal? totalHours = null;

            if (isLeaveRequest)
            {
                totalDays = await CalculateLeaveDaysAsync(request.FromDate, request.ToDate, tenantId);
                
                // Validate leave type (if specified)
                if (request.LeaveTypeId.HasValue)
                {
                    // First check MasterDropdown table (primary source)
                    var leaveTypesFromDropdown = await _unitOfWork.MasterDropdown.FindAsync(m =>
                        m.TenantId == tenantId &&
                        m.Category.ToLower() == "leavetype" &&
                        m.Id == request.LeaveTypeId.Value &&
                        m.IsActive);

                    var leaveTypeDropdown = leaveTypesFromDropdown.FirstOrDefault();

                    if (leaveTypeDropdown == null)
                    {
                        // Fallback: check LeaveTypeMaster table for backward compatibility
                        var leaveType = await _unitOfWork.LeaveTypeMaster.GetByIdAsync(request.LeaveTypeId.Value);
                        if (leaveType == null || leaveType.TenantId != tenantId || !leaveType.IsActive)
                        {
                            throw new InvalidOperationException("Invalid Leave Type.");
                        }

                        // Validate against max days per year (only for LeaveTypeMaster)
                        if (totalDays > leaveType.MaxDaysPerYear)
                        {
                            throw new InvalidOperationException($"Leave days ({totalDays}) cannot exceed maximum days per year ({leaveType.MaxDaysPerYear}).");
                        }
                    }
                }
            }
            else // OT request
            {
                totalHours = await CalculateOTHoursAsync(request.FromDate, request.ToDate);
            }

            // Get PENDING status ID from MasterDropdown
            var pendingStatusId = await GetLeaveStatusIdWithFallbackAsync("PENDING", tenantId, 1);

            // Create request
            var leaveOtRequest = new LeaveOtRequest
            {
                TenantId = tenantId,
                StaffId = request.StaffId,
                RequestTypeId = request.RequestTypeId,
                LeaveTypeId = request.LeaveTypeId,
                FromDate = request.FromDate,
                ToDate = request.ToDate,
                TotalDays = totalDays,
                TotalHours = totalHours,
                Reason = request.Reason,
                LeaveStatus = pendingStatusId, // PENDING - using ID from MasterDropdown
                ReportingManagerId = request.ReportingManagerId,
                HRApprovalRequired = request.HRApprovalRequired,
                Attachment = request.Attachment,
                IsActive = true,
                CreatedBy = userId.ToString(),
                CreatedDate = DateTime.UtcNow
            };

            await _unitOfWork.LeaveOtRequest.AddAsync(leaveOtRequest);
            await _unitOfWork.SaveChangesAsync();

            // Log audit
            var requestTypeName = request.RequestTypeId == 1 ? "Leave" : "OT";
            await LogAuditAsync(tenantId, userId, ActionType.Create, 
                $"Created Leave/OT request: {requestTypeName} from {request.FromDate:yyyy-MM-dd} to {request.ToDate:yyyy-MM-dd}", ipAddress);

            // Send email notification to staff
            if (_emailService != null && !string.IsNullOrEmpty(staff.Email))
            {
                try
                {
                    await _emailService.SendLeaveRequestSubmittedEmailAsync(
                        staff.Email, staff.Name, requestTypeName, request.FromDate, request.ToDate, totalDays, totalHours);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to send email notification for leave request submission");
                }
            }

            return await MapToDtoAsync(leaveOtRequest, tenantId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating Leave/OT request for staff: {StaffId}, tenant: {TenantId}", request.StaffId, tenantId);
            throw;
        }
    }

    public async Task<LeaveOtRequestDto> UpdateRequestAsync(UpdateLeaveOtRequestDto request, int tenantId, int userId, string? ipAddress)
    {
        try
        {
            var existingRequest = await _unitOfWork.LeaveOtRequest.GetByIdAsync(request.RequestId);
            if (existingRequest == null || existingRequest.TenantId != tenantId || !existingRequest.IsActive)
            {
                throw new InvalidOperationException("Request not found or inactive.");
            }

            // Get PENDING status ID from MasterDropdown
            var pendingStatusId = await GetLeaveStatusIdWithFallbackAsync("PENDING", tenantId, 1);
            
            // Only allow update if status is PENDING
            if (existingRequest.LeaveStatus != pendingStatusId)
            {
                throw new InvalidOperationException("Only pending requests can be updated.");
            }

            // Validate dates
            if (request.FromDate > request.ToDate)
            {
                throw new ArgumentException("From Date cannot be after To Date.");
            }

            // Get RequestType to determine if it's Leave or OT
            var requestType = await _unitOfWork.MasterDropdown.FindAsync(m => 
                m.TenantId == tenantId && 
                m.Category == "RequestType" && 
                m.Id == request.RequestTypeId && 
                m.IsActive);
            var requestTypeEntity = requestType.FirstOrDefault();
            var isLeaveRequest = requestTypeEntity?.Code?.ToUpper() == "LEAVE" || 
                               (requestTypeEntity == null && request.RequestTypeId == 1); // Fallback for backward compatibility

            // Recalculate days/hours
            decimal? totalDays = null;
            decimal? totalHours = null;

            if (isLeaveRequest)
            {
                totalDays = await CalculateLeaveDaysAsync(request.FromDate, request.ToDate, tenantId);
            }
            else // OT request
            {
                totalHours = await CalculateOTHoursAsync(request.FromDate, request.ToDate);
            }

            // Update request
            existingRequest.RequestTypeId = request.RequestTypeId;
            existingRequest.LeaveTypeId = request.LeaveTypeId;
            existingRequest.FromDate = request.FromDate;
            existingRequest.ToDate = request.ToDate;
            existingRequest.TotalDays = totalDays;
            existingRequest.TotalHours = totalHours;
            existingRequest.Reason = request.Reason;
            existingRequest.ReportingManagerId = request.ReportingManagerId;
            existingRequest.HRApprovalRequired = request.HRApprovalRequired;
            existingRequest.Attachment = request.Attachment;
            existingRequest.UpdatedBy = userId.ToString();
            existingRequest.UpdatedDate = DateTime.UtcNow;

            await _unitOfWork.LeaveOtRequest.UpdateAsync(existingRequest);
            await _unitOfWork.SaveChangesAsync();

            // Log audit
            await LogAuditAsync(tenantId, userId, ActionType.Update, 
                $"Updated Leave/OT request: {request.RequestId}", ipAddress);

            return await MapToDtoAsync(existingRequest, tenantId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating Leave/OT request: {RequestId}, tenant: {TenantId}", request.RequestId, tenantId);
            throw;
        }
    }

    public async Task<bool> ApproveRequestAsync(ApproveLeaveOtRequestDto request, int tenantId, int approverUserId, bool isHRApproval, string? ipAddress)
    {
        try
        {
            var leaveOtRequest = await _unitOfWork.LeaveOtRequest.GetByIdAsync(request.RequestId);
            if (leaveOtRequest == null || leaveOtRequest.TenantId != tenantId || !leaveOtRequest.IsActive)
            {
                throw new InvalidOperationException("Request not found or inactive.");
            }

            // Validate approver
            var approver = await _unitOfWork.User.GetByIdAsync(approverUserId);
            if (approver == null || approver.TenantId != tenantId || !approver.IsActive)
            {
                throw new InvalidOperationException("Invalid approver.");
            }

            // Get status IDs from MasterDropdown
            var pendingStatusId = await GetLeaveStatusIdWithFallbackAsync("PENDING", tenantId, 1);
            var approvedStatusId = await GetLeaveStatusIdWithFallbackAsync("APPROVED", tenantId, 2);
            var rejectedStatusId = await GetLeaveStatusIdWithFallbackAsync("REJECTED", tenantId, 3);

            if (isHRApproval)
            {
                // HR approval
                if (!leaveOtRequest.HRApprovalRequired)
                {
                    throw new InvalidOperationException("HR approval is not required for this request.");
                }

                if (leaveOtRequest.LeaveStatus != pendingStatusId && leaveOtRequest.LeaveStatus != approvedStatusId) // Not PENDING or APPROVED by L1
                {
                    throw new InvalidOperationException("Request must be approved by manager before HR approval.");
                }

                leaveOtRequest.ApprovedBy_HR = approverUserId;
                leaveOtRequest.ApprovedDate_HR = DateTime.UtcNow;
                leaveOtRequest.LeaveStatus = request.IsApproved ? approvedStatusId : rejectedStatusId; // APPROVED or REJECTED
            }
            else
            {
                // Manager (L1) approval - Reporting Manager is the 1st approver
                // Check if approver is the reporting manager
                var approverUser = await _unitOfWork.User.GetByIdAsync(approverUserId);
                if (approverUser == null || !approverUser.StaffId.HasValue)
                {
                    throw new InvalidOperationException("Approver must have a staff record.");
                }

                var approverStaffId = approverUser.StaffId.Value;
                if (leaveOtRequest.ReportingManagerId != approverStaffId)
                {
                    throw new InvalidOperationException("Only the reporting manager can approve this request.");
                }

                if (leaveOtRequest.LeaveStatus != pendingStatusId) // Not PENDING
                {
                    throw new InvalidOperationException("Only pending requests can be approved.");
                }

                leaveOtRequest.ApprovedBy_L1 = approverUserId;
                leaveOtRequest.ApprovedDate_L1 = DateTime.UtcNow;
                leaveOtRequest.LeaveStatus = request.IsApproved ? approvedStatusId : rejectedStatusId; // APPROVED or REJECTED

                // If HR approval is not required, final status is set
                if (!leaveOtRequest.HRApprovalRequired && request.IsApproved)
                {
                    leaveOtRequest.LeaveStatus = approvedStatusId; // APPROVED
                }
            }

            leaveOtRequest.UpdatedBy = approverUserId.ToString();
            leaveOtRequest.UpdatedDate = DateTime.UtcNow;

            await _unitOfWork.LeaveOtRequest.UpdateAsync(leaveOtRequest);
            await _unitOfWork.SaveChangesAsync();

            // Get staff details for email notification and leave balance update
            var staffMember = await _unitOfWork.Staff.GetByIdAsync(leaveOtRequest.StaffId);
            var approverName = approver.FullName;

            // Get RequestType to determine if it's Leave
            var requestTypeForApproval = await _unitOfWork.MasterDropdown.FindAsync(m => 
                m.TenantId == tenantId && 
                m.Category == "RequestType" && 
                m.Id == leaveOtRequest.RequestTypeId && 
                m.IsActive);
            var requestTypeEntityForApproval = requestTypeForApproval.FirstOrDefault();
            var isLeaveRequestForApproval = requestTypeEntityForApproval?.Code?.ToUpper() == "LEAVE" || 
                                           (requestTypeEntityForApproval == null && leaveOtRequest.RequestTypeId == 1);

            // Update leave balance if approved and it's a leave request
            if (request.IsApproved && isLeaveRequestForApproval && leaveOtRequest.LeaveTypeId.HasValue && leaveOtRequest.TotalDays.HasValue)
            {
                // Only update balance if it's final approval (HR approval or no HR approval required)
                bool isFinalApproval = isHRApproval || !leaveOtRequest.HRApprovalRequired;
                
                if (isFinalApproval && _leaveBalanceService != null)
                {
                    try
                    {
                        var currentYear = DateTime.Now.Year;
                        await _leaveBalanceService.DeductLeaveDaysAsync(
                            leaveOtRequest.StaffId,
                            leaveOtRequest.LeaveTypeId.Value,
                            leaveOtRequest.TotalDays.Value,
                            currentYear,
                            tenantId,
                            approverUserId);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to update leave balance for request: {RequestId}", request.RequestId);
                    }
                }
            }

            // Send email notifications
            if (_emailService != null && staffMember != null && !string.IsNullOrEmpty(staffMember.Email))
            {
                    try
                    {
                        var requestTypeName = requestTypeEntityForApproval?.Name ?? (leaveOtRequest.RequestTypeId == 1 ? "Leave" : "OT");
                    
                    if (request.IsApproved)
                    {
                        // Send approval email to staff
                        await _emailService.SendLeaveRequestApprovedEmailAsync(
                            staffMember.Email,
                            staffMember.Name,
                            requestTypeName,
                            leaveOtRequest.FromDate,
                            leaveOtRequest.ToDate,
                            approverName,
                            request.Comments);

                        // If manager approved and HR approval is required, notify HR
                        if (!isHRApproval && leaveOtRequest.HRApprovalRequired)
                        {
                            // Get HR email (you may need to get this from configuration or user roles)
                            // For now, we'll skip HR notification email as we don't have HR email list
                            // This can be enhanced later
                        }
                    }
                    else
                    {
                        // Send rejection email to staff
                        await _emailService.SendLeaveRequestRejectedEmailAsync(
                            staffMember.Email,
                            staffMember.Name,
                            requestTypeName,
                            leaveOtRequest.FromDate,
                            leaveOtRequest.ToDate,
                            approverName,
                            request.Comments);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to send email notification for leave request approval/rejection");
                }
            }

            // Log audit
            var action = request.IsApproved ? "Approved" : "Rejected";
            var approvalType = isHRApproval ? "HR" : "Manager";
            await LogAuditAsync(tenantId, approverUserId, ActionType.Approve, 
                $"{approvalType} {action} Leave/OT request: {request.RequestId}", ipAddress);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error approving Leave/OT request: {RequestId}, tenant: {TenantId}", request.RequestId, tenantId);
            throw;
        }
    }

    public async Task<bool> CancelRequestAsync(int requestId, int tenantId, int userId, string? ipAddress)
    {
        try
        {
            var leaveOtRequest = await _unitOfWork.LeaveOtRequest.GetByIdAsync(requestId);
            if (leaveOtRequest == null || leaveOtRequest.TenantId != tenantId || !leaveOtRequest.IsActive)
            {
                throw new InvalidOperationException("Request not found or inactive.");
            }

            // Get status IDs from MasterDropdown
            var pendingStatusId = await GetLeaveStatusIdWithFallbackAsync("PENDING", tenantId, 1);
            var cancelledStatusId = await GetLeaveStatusIdWithFallbackAsync("CANCELLED", tenantId, 4);

            // Only allow cancellation if status is PENDING
            if (leaveOtRequest.LeaveStatus != pendingStatusId)
            {
                throw new InvalidOperationException("Only pending requests can be cancelled.");
            }

            // Delete attachment file if exists
            if (!string.IsNullOrEmpty(leaveOtRequest.Attachment) && _fileStorageService != null)
            {
                try
                {
                    await _fileStorageService.DeleteFileAsync(leaveOtRequest.Attachment);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to delete attachment file: {Attachment}", leaveOtRequest.Attachment);
                    // Continue with cancellation even if file deletion fails
                }
            }

            leaveOtRequest.LeaveStatus = cancelledStatusId; // CANCELLED - using ID from MasterDropdown
            leaveOtRequest.IsActive = false;
            leaveOtRequest.UpdatedBy = userId.ToString();
            leaveOtRequest.UpdatedDate = DateTime.UtcNow;

            await _unitOfWork.LeaveOtRequest.UpdateAsync(leaveOtRequest);
            await _unitOfWork.SaveChangesAsync();

            // Log audit
            await LogAuditAsync(tenantId, userId, ActionType.Delete, 
                $"Cancelled Leave/OT request: {requestId}", ipAddress);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cancelling Leave/OT request: {RequestId}, tenant: {TenantId}", requestId, tenantId);
            throw;
        }
    }

    public async Task<IEnumerable<LeaveOtRequestDto>> GetByStaffIdAsync(int staffId, int tenantId)
    {
        try
        {
            _logger.LogInformation("Loading leave requests for StaffId: {StaffId}, TenantId: {TenantId}", staffId, tenantId);
            
            if (staffId <= 0 || tenantId <= 0)
            {
                _logger.LogWarning("Invalid parameters: StaffId={StaffId}, TenantId={TenantId}", staffId, tenantId);
                return new List<LeaveOtRequestDto>();
            }
            
            var requests = await _unitOfWork.LeaveOtRequest.GetByStaffIdAsync(staffId, tenantId);
            var requestsList = requests?.ToList() ?? new List<LeaveOtRequest>();
            _logger.LogInformation("Retrieved {Count} raw requests from database", requestsList.Count);
            
            var dtos = new List<LeaveOtRequestDto>();

            foreach (var request in requestsList)
            {
                try
                {
                    if (request == null)
                    {
                        _logger.LogWarning("Null request found in collection, skipping");
                        continue;
                    }
                    
                    var dto = await MapToDtoAsync(request, tenantId);
                    if (dto != null)
                    {
                        dtos.Add(dto);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error mapping request {RequestId} to DTO. RequestTypeId: {RequestTypeId}, LeaveStatus: {LeaveStatus}, Exception: {ExceptionMessage}", 
                        request?.RequestId ?? 0, request?.RequestTypeId ?? 0, request?.LeaveStatus, ex.Message);
                    // Continue with other requests even if one fails
                }
            }

            _logger.LogInformation("Successfully mapped {Count} requests out of {Total} for StaffId: {StaffId}", 
                dtos.Count, requestsList.Count, staffId);
            return dtos;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting Leave/OT requests for staff: {StaffId}, tenant: {TenantId}. Exception: {ExceptionMessage}, InnerException: {InnerException}, StackTrace: {StackTrace}", 
                staffId, tenantId, ex.Message, ex.InnerException?.Message, ex.StackTrace);
            throw;
        }
    }

    public async Task<IEnumerable<LeaveOtRequestDto>> GetPendingByReportingManagerIdAsync(int reportingManagerId, int tenantId)
    {
        try
        {
            // Get PENDING status ID from MasterDropdown
            var pendingStatusId = await GetLeaveStatusIdWithFallbackAsync("PENDING", tenantId, 1);
            
            var requests = await _unitOfWork.LeaveOtRequest.GetPendingByReportingManagerIdAsync(reportingManagerId, tenantId, pendingStatusId);
            var dtos = new List<LeaveOtRequestDto>();

            foreach (var request in requests)
            {
                dtos.Add(await MapToDtoAsync(request, tenantId));
            }

            return dtos;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting pending Leave/OT requests for manager: {ManagerId}, tenant: {TenantId}", reportingManagerId, tenantId);
            throw;
        }
    }

    public async Task<IEnumerable<LeaveOtRequestDto>> GetPendingForHRAsync(int tenantId)
    {
        try
        {
            // Get status IDs from MasterDropdown
            var pendingStatusId = await GetLeaveStatusIdWithFallbackAsync("PENDING", tenantId, 1);
            var approvedStatusId = await GetLeaveStatusIdWithFallbackAsync("APPROVED", tenantId, 2);
            
            _logger.LogInformation("Loading HR pending requests for tenant {TenantId} with PENDING status ID: {PendingId}, APPROVED status ID: {ApprovedId}", 
                tenantId, pendingStatusId, approvedStatusId);
            
            var requests = await _unitOfWork.LeaveOtRequest.GetPendingForHRAsync(tenantId, pendingStatusId, approvedStatusId);
            _logger.LogInformation("Found {Count} raw requests for HR approval", requests.Count());
            
            var dtos = new List<LeaveOtRequestDto>();

            foreach (var request in requests)
            {
                try
                {
                    var dto = await MapToDtoAsync(request, tenantId);
                    dtos.Add(dto);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error mapping request {RequestId} to DTO for HR approval", request.RequestId);
                    // Continue with other requests even if one fails
                }
            }

            _logger.LogInformation("Successfully mapped {Count} requests for HR approval", dtos.Count);
            return dtos;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting pending Leave/OT requests for HR, tenant: {TenantId}. Exception: {ExceptionMessage}", tenantId, ex.Message);
            throw;
        }
    }

    public async Task<IEnumerable<LeaveOtRequestDto>> GetByTenantIdAsync(int tenantId, bool includeInactive = false)
    {
        try
        {
            var requests = await _unitOfWork.LeaveOtRequest.GetByTenantIdAsync(tenantId, includeInactive);
            var dtos = new List<LeaveOtRequestDto>();

            foreach (var request in requests)
            {
                dtos.Add(await MapToDtoAsync(request, tenantId));
            }

            return dtos;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting Leave/OT requests for tenant: {TenantId}", tenantId);
            throw;
        }
    }

    public async Task<LeaveOtRequestDto?> GetByIdAsync(int requestId, int tenantId)
    {
        try
        {
            var request = await _unitOfWork.LeaveOtRequest.GetByIdAsync(requestId);
            if (request == null || request.TenantId != tenantId)
            {
                return null;
            }

            return await MapToDtoAsync(request, tenantId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting Leave/OT request: {RequestId}, tenant: {TenantId}", requestId, tenantId);
            throw;
        }
    }

    public async Task<decimal> CalculateLeaveDaysAsync(DateTime fromDate, DateTime toDate, int tenantId)
    {
        try
        {
            // Get holidays for the date range
            var holidays = await _unitOfWork.HolidayMaster.GetByDateRangeAsync(tenantId, fromDate, toDate);
            var holidayDates = holidays.Select(h => h.HolidayDate.Date).ToHashSet();

            decimal totalDays = 0;
            var currentDate = fromDate.Date;

            while (currentDate <= toDate.Date)
            {
                // Exclude weekends (Saturday = 6, Sunday = 0)
                if (currentDate.DayOfWeek != DayOfWeek.Saturday && currentDate.DayOfWeek != DayOfWeek.Sunday)
                {
                    // Exclude holidays
                    if (!holidayDates.Contains(currentDate))
                    {
                        totalDays++;
                    }
                }

                currentDate = currentDate.AddDays(1);
            }

            return totalDays;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating leave days from {FromDate} to {ToDate}, tenant: {TenantId}", fromDate, toDate, tenantId);
            throw;
        }
    }

    public async Task<decimal> CalculateOTHoursAsync(DateTime fromDate, DateTime toDate)
    {
        try
        {
            var timeSpan = toDate - fromDate;
            var totalHours = (decimal)timeSpan.TotalHours;
            return totalHours;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating OT hours from {FromDate} to {ToDate}", fromDate, toDate);
            throw;
        }
    }

    private async Task<LeaveOtRequestDto> MapToDtoAsync(LeaveOtRequest request, int tenantId)
    {
        try
        {
            // Get staff name
            var staff = await _unitOfWork.Staff.GetByIdAsync(request.StaffId);
            var staffName = staff?.Name ?? "Unknown";

            // Get request type name - lookup by ID directly from MasterDropdown
            string requestTypeName = "Unknown";
            try
            {
                var requestTypeId = request.RequestTypeId;
                var requestTypes = await _unitOfWork.MasterDropdown.FindAsync(m => 
                    m.TenantId == tenantId && 
                    m.Category == "RequestType" && 
                    m.Id == requestTypeId && 
                    m.IsActive);
                var requestType = requestTypes.FirstOrDefault();
                if (requestType != null)
                {
                    requestTypeName = requestType.Name;
                }
                else
                {
                    requestTypeName = $"RequestType {requestTypeId}";
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error loading request type for RequestTypeId: {RequestTypeId}. Exception: {ExceptionMessage}", 
                    request.RequestTypeId, ex.Message);
                requestTypeName = $"RequestType {request.RequestTypeId}";
            }

            // Get leave type name - first try LeaveTypeMaster, then fallback to MasterDropdown
            string? leaveTypeName = null;
            if (request.LeaveTypeId.HasValue)
            {
                try
                {
                    // Primary: lookup from LeaveTypeMaster table
                    var leaveTypeMaster = await _unitOfWork.LeaveTypeMaster.GetByIdAsync(request.LeaveTypeId.Value);
                    if (leaveTypeMaster != null && leaveTypeMaster.TenantId == tenantId && leaveTypeMaster.IsActive)
                    {
                        leaveTypeName = leaveTypeMaster.LeaveTypeName;
                    }
                    else
                    {
                        // Fallback: lookup from MasterDropdown table (category: leavetype)
                        var leaveTypes = await _unitOfWork.MasterDropdown.FindAsync(m =>
                            m.TenantId == tenantId &&
                            m.Category.ToLower() == "leavetype" &&
                            m.Id == request.LeaveTypeId.Value &&
                            m.IsActive);
                        var leaveType = leaveTypes.FirstOrDefault();
                        if (leaveType != null)
                        {
                            leaveTypeName = leaveType.Name;
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error loading leave type for LeaveTypeId: {LeaveTypeId}", request.LeaveTypeId.Value);
                }
            }

            // Get leave status name - lookup by code first (more reliable than ID)
            string? leaveStatusName = "Unknown";
            if (request.LeaveStatus.HasValue)
            {
                try
                {
                    var statusId = request.LeaveStatus.Value;
                    // Use code lookup first (more reliable than ID comparison which may have type mismatch issues)
                    var statusCode = GetLeaveStatusCode(request.LeaveStatus);
                    var leaveStatuses = await _unitOfWork.MasterDropdown.FindAsync(m => 
                        m.TenantId == tenantId && 
                        m.Category == "LeaveStatus" && 
                        m.Code == statusCode && 
                        m.IsActive);
                    var leaveStatusList = leaveStatuses.ToList();
                    var leaveStatus = leaveStatusList.FirstOrDefault();
                    
                    if (leaveStatus != null)
                    {
                        leaveStatusName = leaveStatus.Name;
                    }
                    else
                    {
                        // Fallback: try to find by ID (but catch any type conversion errors)
                        try
                        {
                            var leaveStatusById = await _unitOfWork.MasterDropdown.FindAsync(m => 
                                m.TenantId == tenantId && 
                                m.Category == "LeaveStatus" && 
                                m.Id == statusId && 
                                m.IsActive);
                            var statusById = leaveStatusById.FirstOrDefault();
                            if (statusById != null)
                            {
                                leaveStatusName = statusById.Name;
                            }
                            else
                            {
                                leaveStatusName = statusCode; // Use code as fallback
                            }
                        }
                        catch (InvalidCastException castEx)
                        {
                            // Handle type mismatch - database Id column might have type mismatch
                            _logger.LogWarning(castEx, "Type mismatch when looking up leave status by ID: {StatusId}, tenant: {TenantId}. Using code as fallback.", 
                                statusId, tenantId);
                            leaveStatusName = statusCode;
                        }
                        catch (Exception idEx)
                        {
                            _logger.LogWarning(idEx, "Error looking up leave status by ID: {StatusId}. Using code as fallback.", statusId);
                            leaveStatusName = statusCode;
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error looking up leave status for ID: {StatusId}, tenant: {TenantId}. Exception: {ExceptionMessage}", 
                        request.LeaveStatus.Value, tenantId, ex.Message);
                    // Use code as fallback
                    leaveStatusName = GetLeaveStatusCode(request.LeaveStatus);
                }
            }

            // Get reporting manager name
            string? reportingManagerName = null;
            if (request.ReportingManagerId.HasValue)
            {
                try
                {
                    var manager = await _unitOfWork.Staff.GetByIdAsync(request.ReportingManagerId.Value);
                    reportingManagerName = manager?.Name;
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error loading reporting manager for ReportingManagerId: {ManagerId}", request.ReportingManagerId.Value);
                }
            }

            // Get approver names
            string? approvedBy_L1Name = null;
            if (request.ApprovedBy_L1.HasValue)
            {
                try
                {
                    var approver = await _unitOfWork.User.GetByIdAsync(request.ApprovedBy_L1.Value);
                    approvedBy_L1Name = approver?.FullName;
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error loading L1 approver for UserId: {UserId}", request.ApprovedBy_L1.Value);
                }
            }

            string? approvedBy_HRName = null;
            if (request.ApprovedBy_HR.HasValue)
            {
                try
                {
                    var approver = await _unitOfWork.User.GetByIdAsync(request.ApprovedBy_HR.Value);
                    approvedBy_HRName = approver?.FullName;
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error loading HR approver for UserId: {UserId}", request.ApprovedBy_HR.Value);
                }
            }

            return new LeaveOtRequestDto
            {
                RequestId = request.RequestId,
                StaffId = request.StaffId,
                StaffName = staffName,
                RequestTypeId = request.RequestTypeId,
                RequestTypeName = requestTypeName,
                LeaveTypeId = request.LeaveTypeId,
                LeaveTypeName = leaveTypeName,
                FromDate = request.FromDate,
                ToDate = request.ToDate,
                TotalDays = request.TotalDays,
                TotalHours = request.TotalHours,
                Reason = request.Reason,
                LeaveStatus = request.LeaveStatus,
                LeaveStatusName = leaveStatusName,
                ReportingManagerId = request.ReportingManagerId,
                ReportingManagerName = reportingManagerName,
                HRApprovalRequired = request.HRApprovalRequired,
                ApprovedBy_L1 = request.ApprovedBy_L1,
                ApprovedBy_L1Name = approvedBy_L1Name,
                ApprovedDate_L1 = request.ApprovedDate_L1,
                ApprovedBy_HR = request.ApprovedBy_HR,
                ApprovedBy_HRName = approvedBy_HRName,
                ApprovedDate_HR = request.ApprovedDate_HR,
                CreatedDate = request.CreatedDate,
                Attachment = request.Attachment
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error mapping LeaveOtRequest {RequestId} to DTO for tenant {TenantId}", request.RequestId, tenantId);
            // Return a minimal DTO to prevent complete failure
            return new LeaveOtRequestDto
            {
                RequestId = request.RequestId,
                StaffId = request.StaffId,
                StaffName = "Unknown",
                RequestTypeId = request.RequestTypeId,
                RequestTypeName = request.RequestTypeId == 1 ? "Leave" : "OT",
                LeaveTypeId = request.LeaveTypeId,
                FromDate = request.FromDate,
                ToDate = request.ToDate,
                TotalDays = request.TotalDays,
                TotalHours = request.TotalHours,
                Reason = request.Reason,
                LeaveStatus = request.LeaveStatus,
                LeaveStatusName = "Unknown",
                ReportingManagerId = request.ReportingManagerId,
                HRApprovalRequired = request.HRApprovalRequired,
                ApprovedBy_L1 = request.ApprovedBy_L1,
                ApprovedDate_L1 = request.ApprovedDate_L1,
                ApprovedBy_HR = request.ApprovedBy_HR,
                ApprovedDate_HR = request.ApprovedDate_HR,
                CreatedDate = request.CreatedDate
            };
        }
    }

    private string GetLeaveStatusCode(int? status)
    {
        return status switch
        {
            1 => "PENDING",
            2 => "APPROVED",
            3 => "REJECTED",
            4 => "CANCELLED",
            _ => "PENDING"
        };
    }

    /// <summary>
    /// Gets the LeaveStatus ID from MasterDropdown by code
    /// </summary>
    private async Task<int?> GetLeaveStatusIdByCodeAsync(string code, int tenantId)
    {
        try
        {
            var leaveStatuses = await _unitOfWork.MasterDropdown.FindAsync(m => 
                m.TenantId == tenantId && 
                m.Category == "LeaveStatus" && 
                m.Code == code && 
                m.IsActive);
            
            var leaveStatus = leaveStatuses.FirstOrDefault();
            return leaveStatus?.Id;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error getting LeaveStatus ID for code: {Code}, tenant: {TenantId}", code, tenantId);
            return null;
        }
    }

    /// <summary>
    /// Gets the LeaveStatus ID by code, with fallback to hardcoded values if not found in MasterDropdown
    /// This ensures backward compatibility
    /// </summary>
    private async Task<int> GetLeaveStatusIdWithFallbackAsync(string code, int tenantId, int fallbackValue)
    {
        var statusId = await GetLeaveStatusIdByCodeAsync(code, tenantId);
        if (statusId.HasValue)
        {
            return statusId.Value;
        }
        
        _logger.LogWarning("LeaveStatus with code '{Code}' not found in MasterDropdown for tenant {TenantId}, using fallback value {Fallback}", 
            code, tenantId, fallbackValue);
        return fallbackValue;
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
                CreatedDate = DateTime.UtcNow
            };

            await _unitOfWork.AuditLogs.AddAsync(auditLog);
            await _unitOfWork.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to log audit entry for Leave/OT operation");
            // Don't throw - audit logging failure shouldn't break the operation
        }
    }

    #region Stored Procedure Methods

    public async Task<PagedResult<LeaveRequestListSpDto>> GetLeaveRequestListSpAsync(GetLeaveRequestListRequest request)
    {
        try
        {
            var (items, totalCount) = await _unitOfWork.LeaveOtRequest.GetLeaveRequestListSpAsync(request);

            return new PagedResult<LeaveRequestListSpDto>
            {
                Items = items.ToList(),
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting leave request list from stored procedure for tenant: {TenantId}", request.TenantId);
            throw;
        }
    }

    public async Task<IEnumerable<LeaveRequestListSpDto>> GetLeaveRequestByStaffSpAsync(int tenantId, int staffId, int? requestTypeId = null, int? year = null, bool isActive = true)
    {
        try
        {
            return await _unitOfWork.LeaveOtRequest.GetLeaveRequestByStaffSpAsync(tenantId, staffId, requestTypeId, year, isActive);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting leave requests by staff from stored procedure for tenant: {TenantId}, staff: {StaffId}", tenantId, staffId);
            throw;
        }
    }

    public async Task<PagedResult<LeaveApprovalListSpDto>> GetLeaveApprovalListSpAsync(GetLeaveApprovalListRequest request)
    {
        try
        {
            var (items, totalCount) = await _unitOfWork.LeaveOtRequest.GetLeaveApprovalListSpAsync(request);

            return new PagedResult<LeaveApprovalListSpDto>
            {
                Items = items.ToList(),
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting leave approval list from stored procedure for tenant: {TenantId}", request.TenantId);
            throw;
        }
    }

    public async Task<IEnumerable<PendingApprovalByManagerDto>> GetPendingApprovalsByManagerSpAsync(int tenantId, int reportingManagerId, int pendingStatusId)
    {
        try
        {
            return await _unitOfWork.LeaveOtRequest.GetPendingApprovalsByManagerSpAsync(tenantId, reportingManagerId, pendingStatusId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting pending approvals by manager from stored procedure for tenant: {TenantId}, manager: {ManagerId}", tenantId, reportingManagerId);
            throw;
        }
    }

    public async Task<IEnumerable<PendingApprovalForHRDto>> GetPendingApprovalsForHRSpAsync(int tenantId, int pendingStatusId, int approvedStatusId)
    {
        try
        {
            return await _unitOfWork.LeaveOtRequest.GetPendingApprovalsForHRSpAsync(tenantId, pendingStatusId, approvedStatusId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting pending approvals for HR from stored procedure for tenant: {TenantId}", tenantId);
            throw;
        }
    }

    #endregion
}


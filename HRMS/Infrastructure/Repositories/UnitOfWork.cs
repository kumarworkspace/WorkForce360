using HRMS.Core.Domain.Interfaces;
using HRMS.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace HRMS.Infrastructure.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly HRMSDbContext _context;
    private IDbContextTransaction? _transaction;

    public IUserRepository User { get; }
    public IAuditLogRepository AuditLogs { get; }
    public IStaffRepository Staff { get; }
    public IMasterDropdownRepository MasterDropdown { get; }
    public IEducationDetailRepository EducationDetail { get; }
    public IExperienceDetailRepository ExperienceDetail { get; }
    public ILegalDocumentRepository LegalDocument { get; }
    public IRoleRepository Role { get; }
    public IPermissionRepository Permission { get; }
    public IRolePermissionRepository RolePermission { get; }
    public IUserRoleRepository UserRole { get; }
    public ILeaveTypeMasterRepository LeaveTypeMaster { get; }
    public IHolidayMasterRepository HolidayMaster { get; }
    public ILeaveOtRequestRepository LeaveOtRequest { get; }
    public ILeaveBalanceRepository LeaveBalance { get; }
    public ICourseRegistrationRepository CourseRegistration { get; }
    public ICoursePlanningRepository CoursePlanning { get; }
    public ICourseAttendanceRepository CourseAttendance { get; }
    public ICourseParticipantRepository CourseParticipant { get; }
    public ICourseAttendanceDateWiseRepository CourseAttendanceDateWise { get; }
    public ICourseResultRepository CourseResult { get; }
    public IMenuGroupRepository MenuGroup { get; }
    public IMenuItemRepository MenuItem { get; }
    public IMasterCategoryRepository MasterCategory { get; }
    public IMasterValueRepository MasterValue { get; }
    public ILmsCourseRepository LmsCourse { get; }
    public ILmsModuleRepository LmsModule { get; }
    public ILearningPathRepository LearningPath { get; }
    public IEnrollmentRepository Enrollment { get; }
    public IProgressTrackingRepository ProgressTracking { get; }

    public UnitOfWork(
        HRMSDbContext context,
        IUserRepository users,
        IAuditLogRepository auditLogs,
        IStaffRepository staff,
        IMasterDropdownRepository masterDropdown,
        IEducationDetailRepository educationDetail,
        IExperienceDetailRepository experienceDetail,
        ILegalDocumentRepository legalDocument,
        IRoleRepository role,
        IPermissionRepository permission,
        IRolePermissionRepository rolePermission,
        IUserRoleRepository userRole,
        ILeaveTypeMasterRepository leaveTypeMaster,
        IHolidayMasterRepository holidayMaster,
        ILeaveOtRequestRepository leaveOtRequest,
        ILeaveBalanceRepository leaveBalance,
        ICourseRegistrationRepository courseRegistration,
        ICoursePlanningRepository coursePlanning,
        ICourseAttendanceRepository courseAttendance,
        ICourseParticipantRepository courseParticipant,
        ICourseAttendanceDateWiseRepository courseAttendanceDateWise,
        ICourseResultRepository courseResult,
        IMenuGroupRepository menuGroup,
        IMenuItemRepository menuItem,
        IMasterCategoryRepository masterCategory,
        IMasterValueRepository masterValue,
        ILmsCourseRepository lmsCourse,
        ILmsModuleRepository lmsModule,
        ILearningPathRepository learningPath,
        IEnrollmentRepository enrollment,
        IProgressTrackingRepository progressTracking)
    {
        _context = context;
        AuditLogs = auditLogs;
        User = users;
        Staff = staff;
        MasterDropdown = masterDropdown;
        EducationDetail = educationDetail;
        ExperienceDetail = experienceDetail;
        LegalDocument = legalDocument;
        Role = role;
        Permission = permission;
        RolePermission = rolePermission;
        UserRole = userRole;
        LeaveTypeMaster = leaveTypeMaster;
        HolidayMaster = holidayMaster;
        LeaveOtRequest = leaveOtRequest;
        LeaveBalance = leaveBalance;
        CourseRegistration = courseRegistration;
        CoursePlanning = coursePlanning;
        CourseAttendance = courseAttendance;
        CourseParticipant = courseParticipant;
        CourseAttendanceDateWise = courseAttendanceDateWise;
        CourseResult = courseResult;
        MenuGroup = menuGroup;
        MenuItem = menuItem;
        MasterCategory = masterCategory;
        MasterValue = masterValue;
        LmsCourse = lmsCourse;
        LmsModule = lmsModule;
        LearningPath = learningPath;
        Enrollment = enrollment;
        ProgressTracking = progressTracking;
    }

    public async Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
    }

    public async Task BeginTransactionAsync()
    {
        _transaction = await _context.Database.BeginTransactionAsync();
    }

    public async Task CommitTransactionAsync()
    {
        if (_transaction != null)
        {
            await _transaction.CommitAsync();
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public async Task RollbackTransactionAsync()
    {
        if (_transaction != null)
        {
            await _transaction.RollbackAsync();
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public void Dispose()
    {
        _transaction?.Dispose();
        _context.Dispose();
    }

    public async Task ExecuteInTransactionAsync(Func<Task> action)
    {
        var strategy = _context.Database.CreateExecutionStrategy();
        await strategy.ExecuteAsync(async () =>
        {
            await BeginTransactionAsync();
            try
            {
                await action();
                await SaveChangesAsync();
                await CommitTransactionAsync();
            }
            catch
            {
                await RollbackTransactionAsync();
                throw;
            }
        });
    }

}

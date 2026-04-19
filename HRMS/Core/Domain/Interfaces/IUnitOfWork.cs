namespace HRMS.Core.Domain.Interfaces;

public interface IUnitOfWork : IDisposable
{
    IUserRepository User { get; }
    IAuditLogRepository AuditLogs { get; }
    IStaffRepository Staff { get; }
    IMasterDropdownRepository MasterDropdown { get; }
    IEducationDetailRepository EducationDetail { get; }
    IExperienceDetailRepository ExperienceDetail { get; }
    ILegalDocumentRepository LegalDocument { get; }
    IRoleRepository Role { get; }
    IPermissionRepository Permission { get; }
    IRolePermissionRepository RolePermission { get; }
    IUserRoleRepository UserRole { get; }
        ILeaveTypeMasterRepository LeaveTypeMaster { get; }
        IHolidayMasterRepository HolidayMaster { get; }
        ILeaveOtRequestRepository LeaveOtRequest { get; }
        ILeaveBalanceRepository LeaveBalance { get; }
    ICourseRegistrationRepository CourseRegistration { get; }
    ICoursePlanningRepository CoursePlanning { get; }
    ICourseAttendanceRepository CourseAttendance { get; }
    ICourseParticipantRepository CourseParticipant { get; }
    ICourseAttendanceDateWiseRepository CourseAttendanceDateWise { get; }
    ICourseResultRepository CourseResult { get; }
    IMenuGroupRepository MenuGroup { get; }
    IMenuItemRepository MenuItem { get; }
    IMasterCategoryRepository MasterCategory { get; }
    IMasterValueRepository MasterValue { get; }
    ILmsCourseRepository LmsCourse { get; }
    ILmsModuleRepository LmsModule { get; }
    ILearningPathRepository LearningPath { get; }
    IEnrollmentRepository Enrollment { get; }
    IProgressTrackingRepository ProgressTracking { get; }

    Task<int> SaveChangesAsync();
    Task BeginTransactionAsync();
    Task CommitTransactionAsync();
    Task RollbackTransactionAsync();
    Task ExecuteInTransactionAsync(Func<Task> action);
}

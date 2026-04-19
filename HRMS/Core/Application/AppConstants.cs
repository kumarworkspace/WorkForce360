namespace HRMS.Core.Application;

/// <summary>
/// System-wide constants. Role names here must match what is seeded in SeedingService
/// and stored in the Roles table. These are only used where the role identity is
/// unavoidable (e.g. Super Admin bypass at login, first-user creation).
/// All other access decisions must go through the DB permission system.
/// </summary>
public static class AppConstants
{
    public static class Roles
    {
        public const string SuperAdmin = "Super Admin";
        public const string HRAdmin    = "HR Admin";
        public const string Manager    = "Manager";
        public const string Trainer    = "Trainer";
        public const string Staff      = "Staff";
        public const string ITAdmin    = "IT Admin";
    }

    public static class Modules
    {
        public const string EmployeeManagement  = "Employee Management";
        public const string LeaveManagement     = "Leave Management";
        public const string LeaveApproval       = "Leave Approval";
        public const string LeaveConfiguration  = "Leave Configuration";
        public const string MyTraining          = "My Training";
        public const string CourseManagement    = "Course Management";
        public const string TmsReports         = "TMS Reports";
        public const string RoleManagement      = "Role Management";
        public const string AccessControl       = "Access Control";
        public const string AuditLogs           = "Audit Logs";
    }
}

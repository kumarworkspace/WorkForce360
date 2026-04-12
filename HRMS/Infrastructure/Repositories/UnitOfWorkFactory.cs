using HRMS.Core.Domain.Interfaces;
using HRMS.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace HRMS.Infrastructure.Repositories;

/// <summary>
/// Factory for creating UnitOfWork instances with their own DbContext.
/// Used for concurrent operations (middleware, NavMenu) to avoid DbContext concurrency issues.
/// </summary>
public class UnitOfWorkFactory : IUnitOfWorkFactory
{
    private readonly IConfiguration _configuration;
    private readonly DbContextOptions<HRMSDbContext> _options;

    public UnitOfWorkFactory(IConfiguration configuration)
    {
        _configuration = configuration;
        
        // Create options that are not scoped - this is safe for factory use
        var optionsBuilder = new DbContextOptionsBuilder<HRMSDbContext>();
        optionsBuilder.UseNpgsql(
            _configuration.GetConnectionString("DefaultConnection"),
            npgsqlOptions => npgsqlOptions
                .EnableRetryOnFailure(
                    maxRetryCount: 5,
                    maxRetryDelay: TimeSpan.FromSeconds(30),
                    errorCodesToAdd: null)
                .CommandTimeout(60));
        
        _options = optionsBuilder.Options;
    }

    public IUnitOfWork Create()
    {
        // Create a new DbContext instance for this UnitOfWork with non-scoped options
        var context = new HRMSDbContext(_options);

            // Create repositories with the new context
            return new UnitOfWork(
                context,
                new UserRepository(context),
                new AuditLogRepository(context),
                new StaffRepository(context),
                new MasterDropdownRepository(context),
                new EducationDetailRepository(context),
                new ExperienceDetailRepository(context),
                new LegalDocumentRepository(context),
                new RoleRepository(context),
                new PermissionRepository(context),
                new RolePermissionRepository(context),
                new UserRoleRepository(context),
            new LeaveTypeMasterRepository(context),
            new HolidayMasterRepository(context),
            new LeaveOtRequestRepository(context),
            new LeaveBalanceRepository(context),
                new CourseRegistrationRepository(context),
                new CoursePlanningRepository(context),
                new CourseAttendanceRepository(context),
                new CourseParticipantRepository(context),
                new CourseAttendanceDateWiseRepository(context),
                new CourseResultRepository(context));
        }

    public void Dispose()
    {
        // Factory itself doesn't hold resources, but implement IDisposable for consistency
    }
}

using MudBlazor.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Identity.Web;
using Microsoft.Identity.Web.UI;

// Clean Architecture - Infrastructure
using HRMS.Infrastructure.Data;
using HRMS.Infrastructure.Repositories;
using HRMS.Infrastructure.Services;
using HRMS.Infrastructure.Middleware;

// Clean Architecture - Application
using HRMS.Core.Application.Interfaces;
using HRMS.Core.Application.Services;

// Clean Architecture - Domain
using HRMS.Core.Domain.Interfaces;

// Fix: Allow writing DateTime.UtcNow to PostgreSQL 'timestamp without time zone' columns
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor(options =>
{
    options.DetailedErrors = true;
    options.DisconnectedCircuitMaxRetained = 100;
    options.DisconnectedCircuitRetentionPeriod = TimeSpan.FromMinutes(3);
    options.JSInteropDefaultCallTimeout = TimeSpan.FromMinutes(1);
    options.MaxBufferedUnacknowledgedRenderBatches = 10;
});
builder.Services.AddMudServices();

// Configure authentication with both Cookie and Microsoft Identity (Entra ID)
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApp(
        openIdConnectOptions =>
        {
            builder.Configuration.Bind("AzureAd", openIdConnectOptions);

            openIdConnectOptions.Events = new OpenIdConnectEvents
            {
                OnTokenValidated = async context =>
                {
                    // Get allowed groups from configuration
                    var allowedGroups = builder.Configuration.GetSection("AzureAd:AllowedGroups").Get<string[]>();

                    if (allowedGroups != null && allowedGroups.Length > 0)
                    {
                        // Check if user belongs to any allowed group
                        var userGroups = context.Principal?.Claims
                            .Where(c => c.Type == "groups")
                            .Select(c => c.Value)
                            .ToList();

                        if (userGroups == null || !userGroups.Any(g => allowedGroups.Contains(g)))
                        {
                            context.Fail("User is not a member of the authorized group.");
                        }
                    }

                    await Task.CompletedTask;
                },
                OnAuthenticationFailed = context =>
                {
                    context.Response.Redirect("/signin?error=authentication_failed");
                    context.HandleResponse();
                    return Task.CompletedTask;
                },
                OnAccessDenied = context =>
                {
                    context.Response.Redirect("/signin?error=access_denied");
                    context.HandleResponse();
                    return Task.CompletedTask;
                }
            };
        },
        cookieOptions =>
        {
            cookieOptions.LoginPath = "/signin";
            cookieOptions.LogoutPath = "/logout";
            cookieOptions.ExpireTimeSpan = TimeSpan.FromDays(7);
            cookieOptions.SlidingExpiration = true;
            cookieOptions.Cookie.HttpOnly = true;
            cookieOptions.Cookie.SecurePolicy = CookieSecurePolicy.None; // Changed for HTTP-only mode
            cookieOptions.Cookie.SameSite = SameSiteMode.Lax;
        });

builder.Services.AddAuthorization();
builder.Services.AddHttpContextAccessor();

// Add DbContext with PostgreSQL provider
builder.Services.AddDbContext<HRMSDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    options.UseNpgsql(
        connectionString,
        npgsqlOptions => npgsqlOptions
            .EnableRetryOnFailure(
                maxRetryCount: 5,
                maxRetryDelay: TimeSpan.FromSeconds(30),
                errorCodesToAdd: null)
            .CommandTimeout(60));
}, ServiceLifetime.Scoped); // Scoped per Blazor circuit

// Register Repositories
builder.Services.AddScoped<ITenantRepository, TenantRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IAuditLogRepository, AuditLogRepository>();
builder.Services.AddScoped<IStaffRepository, StaffRepository>();
builder.Services.AddScoped<IMasterDropdownRepository, MasterDropdownRepository>();
builder.Services.AddScoped<IEducationDetailRepository, EducationDetailRepository>();
builder.Services.AddScoped<IExperienceDetailRepository, ExperienceDetailRepository>();
builder.Services.AddScoped<ILegalDocumentRepository, LegalDocumentRepository>();
builder.Services.AddScoped<IRoleRepository, RoleRepository>();
builder.Services.AddScoped<IPermissionRepository, PermissionRepository>();
builder.Services.AddScoped<IRolePermissionRepository, RolePermissionRepository>();
builder.Services.AddScoped<IUserRoleRepository, UserRoleRepository>();
    builder.Services.AddScoped<ILeaveTypeMasterRepository, LeaveTypeMasterRepository>();
    builder.Services.AddScoped<IHolidayMasterRepository, HolidayMasterRepository>();
    builder.Services.AddScoped<ILeaveOtRequestRepository, LeaveOtRequestRepository>();
    builder.Services.AddScoped<ILeaveBalanceRepository, LeaveBalanceRepository>();
builder.Services.AddScoped<ICourseRegistrationRepository, CourseRegistrationRepository>();
builder.Services.AddScoped<ICoursePlanningRepository, CoursePlanningRepository>();
builder.Services.AddScoped<ICourseAttendanceRepository, CourseAttendanceRepository>();
builder.Services.AddScoped<ICourseParticipantRepository, CourseParticipantRepository>();
builder.Services.AddScoped<ICourseAttendanceDateWiseRepository, CourseAttendanceDateWiseRepository>();
builder.Services.AddScoped<ICourseResultRepository, CourseResultRepository>();
builder.Services.AddScoped<ITMSReportRepository, TMSReportRepository>();
builder.Services.AddScoped<IMenuGroupRepository, MenuGroupRepository>();
builder.Services.AddScoped<IMenuItemRepository, MenuItemRepository>();
builder.Services.AddScoped<IMasterCategoryRepository, MasterCategoryRepository>();
builder.Services.AddScoped<IMasterValueRepository, MasterValueRepository>();
builder.Services.AddScoped<ILmsCourseRepository, LmsCourseRepository>();
builder.Services.AddScoped<ILmsModuleRepository, LmsModuleRepository>();
builder.Services.AddScoped<ILearningPathRepository, LearningPathRepository>();
builder.Services.AddScoped<IEnrollmentRepository, EnrollmentRepository>();
builder.Services.AddScoped<IProgressTrackingRepository, ProgressTrackingRepository>();

// Register Unit of Work
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// Register UnitOfWorkFactory for concurrent operations
builder.Services.AddScoped<IUnitOfWorkFactory, UnitOfWorkFactory>();

// Register Infrastructure Services
builder.Services.AddScoped<IPasswordHasher, PasswordHasher>();
builder.Services.AddScoped<IFileStorageService, FileStorageService>();

// Register Application Services
builder.Services.AddScoped<ITenantService, TenantService>();
builder.Services.AddScoped<ICompanyRegistrationService, CompanyRegistrationService>();
builder.Services.AddScoped<IAuthenticationService, AuthenticationService>();
builder.Services.AddScoped<IStaffService, StaffService>();
builder.Services.AddScoped<IMasterDropdownService, MasterDropdownService>();
builder.Services.AddScoped<IRoleService, RoleService>();
builder.Services.AddScoped<IPermissionService, PermissionService>();
builder.Services.AddScoped<IAccessControlService, AccessControlService>();
    builder.Services.AddScoped<IUserRoleService, UserRoleService>();
    builder.Services.AddScoped<ISeedingService, SeedingService>();
    builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IAuthorizationHelper, AuthorizationHelper>();
builder.Services.AddScoped<ITenantValidationService, TenantValidationService>();
builder.Services.AddScoped<IAuditLogService, AuditLogService>();
builder.Services.AddScoped<ILeaveOtService, LeaveOtService>();
builder.Services.AddScoped<ILeaveTypeMasterService, LeaveTypeMasterService>();
builder.Services.AddScoped<IHolidayMasterService, HolidayMasterService>();
builder.Services.AddScoped<ILeaveBalanceService, LeaveBalanceService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<ICourseRegistrationService, CourseRegistrationService>();
builder.Services.AddScoped<ICoursePlanningService, CoursePlanningService>();
builder.Services.AddScoped<ICourseAttendanceService, CourseAttendanceService>();
builder.Services.AddScoped<ICourseParticipantService, CourseParticipantService>();
builder.Services.AddScoped<ITMSReportService, TMSReportService>();
builder.Services.AddScoped<IMenuService, MenuService>();
builder.Services.AddScoped<IMasterDataService, MasterDataService>();
builder.Services.AddScoped<ILmsCourseService, LmsCourseService>();
builder.Services.AddScoped<ILearningPathService, LearningPathService>();
builder.Services.AddScoped<IEnrollmentService, EnrollmentService>();

var app = builder.Build();

// Auto-seed roles, permissions, and UserRole entries for tenants that bypassed registration
using (var scope = app.Services.CreateScope())
{
    var startupLogger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    try
    {
        var unitOfWork    = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        var tenantRepo    = scope.ServiceProvider.GetRequiredService<ITenantRepository>();
        var seedingSvc    = scope.ServiceProvider.GetRequiredService<ISeedingService>();
        var menuSvc       = scope.ServiceProvider.GetRequiredService<IMenuService>();
        var masterSvc     = scope.ServiceProvider.GetRequiredService<IMasterDataService>();

        var allUsersCache = (await unitOfWork.User.GetAllUsers()).ToList();

        var tenants = await tenantRepo.GetAllActiveTenantsAsync();
        foreach (var tenant in tenants)
        {
            var firstUser = allUsersCache.FirstOrDefault(u => u.TenantId == tenant.TenantId && u.IsActive);
            var createdBy = firstUser?.UserId ?? 0;

            // 1. Full seed if roles/permissions are completely missing
            if (!await seedingSvc.HasRolesAndPermissionsAsync(tenant.TenantId))
            {
                await seedingSvc.SeedDefaultRolesAndPermissionsAsync(tenant.TenantId, createdBy);
                startupLogger.LogInformation("Auto-seeded roles/permissions for tenant {TenantId}", tenant.TenantId);
            }

            // 2. Add any new canonical permissions missing from this tenant's Permission table
            //    (runs automatically when CanonicalPermissions list grows)
            await seedingSvc.EnsureMissingPermissionsAsync(tenant.TenantId, createdBy);

            // 3. Ensure RolePermission entries exist — fixes tenants that have roles+permissions
            //    but were never assigned access levels (most common manual-setup gap)
            await seedingSvc.EnsureRolePermissionsSeededAsync(tenant.TenantId, createdBy);

            // 4. Seed menus and master data if missing
            var createdByStr = firstUser?.UserId.ToString() ?? "system";
            await menuSvc.SeedDefaultMenuAsync(tenant.TenantId, createdByStr);
            await masterSvc.SeedDefaultCategoriesAsync(tenant.TenantId, createdByStr);

            // 5. For any user whose Role string is set but has no UserRole row, create it now
            var tenantUsers = allUsersCache
                .Where(u => u.TenantId == tenant.TenantId && u.IsActive && !string.IsNullOrEmpty(u.Role))
                .ToList();

            var tenantRoles = (await unitOfWork.Role.GetByTenantIdAsync(tenant.TenantId, includeInactive: false)).ToList();

            foreach (var user in tenantUsers)
            {
                var existingUserRoles = await unitOfWork.UserRole.GetByUserIdAsync(user.UserId, tenant.TenantId);
                if (existingUserRoles.Any(ur => ur.IsActive)) continue; // already has an active role

                var matchedRole = tenantRoles.FirstOrDefault(r =>
                    r.RoleName.Equals(user.Role, StringComparison.OrdinalIgnoreCase));

                if (matchedRole == null) continue;

                await unitOfWork.UserRole.AddAsync(new HRMS.Core.Domain.Entities.UserRole
                {
                    UserId     = user.UserId,
                    RoleId     = matchedRole.RoleId,
                    TenantId   = tenant.TenantId,
                    IsActive   = true,
                    CreatedBy  = user.UserId,
                    CreatedDate = DateTime.UtcNow
                });
                startupLogger.LogInformation(
                    "Auto-created UserRole '{Role}' for user {UserId} in tenant {TenantId}",
                    matchedRole.RoleName, user.UserId, tenant.TenantId);
            }

            await unitOfWork.SaveChangesAsync();
        }
    }
    catch (Exception ex)
    {
        startupLogger.LogError(ex, "Error during startup auto-seeding");
    }
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

// app.UseHttpsRedirection(); // Disabled for HTTP-only mode
app.UseStaticFiles();

// Serve files from uploads folder
var uploadsPath = Path.Combine(app.Environment.ContentRootPath, "uploads");
if (!Directory.Exists(uploadsPath))
{
    Directory.CreateDirectory(uploadsPath);
}
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new Microsoft.Extensions.FileProviders.PhysicalFileProvider(uploadsPath),
    RequestPath = "/uploads"
});

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

// Add authentication and audit logging middleware
app.UseMiddleware<AuthenticationMiddleware>();

// Add role-based authorization middleware (runs after authentication)
app.UseMiddleware<RoleBasedAuthorizationMiddleware>();

app.MapRazorPages();
app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();


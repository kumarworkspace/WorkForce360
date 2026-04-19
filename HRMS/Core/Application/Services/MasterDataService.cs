using HRMS.Core.Application.DTOs;
using HRMS.Core.Application.Interfaces;
using HRMS.Core.Domain.Entities;
using HRMS.Core.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace HRMS.Core.Application.Services;

public class MasterDataService : IMasterDataService
{
    private readonly IUnitOfWork _uow;
    private readonly ILogger<MasterDataService> _logger;

    private static readonly List<(string Code, string Name, List<(string Code, string Name)> Values)> DefaultCategories =
    [
        ("CourseType", "Course Type",
        [
            ("Internal",  "Internal"),
            ("External",  "External"),
            ("ELearning", "E-Learning"),
            ("Blended",   "Blended"),
        ]),
        ("AssessmentType", "Assessment Type",
        [
            ("MCQ",        "Multiple Choice"),
            ("Written",    "Written"),
            ("Practical",  "Practical"),
            ("Assignment", "Assignment"),
        ]),
        ("DifficultyLevel", "Difficulty Level",
        [
            ("Beginner",      "Beginner"),
            ("Intermediate",  "Intermediate"),
            ("Advanced",      "Advanced"),
            ("Expert",        "Expert"),
        ]),
        ("SkillCategory", "Skill Category",
        [
            ("Technical",   "Technical"),
            ("SoftSkills",  "Soft Skills"),
            ("Leadership",  "Leadership"),
            ("Compliance",  "Compliance"),
        ]),
        ("CandidateStatus", "Candidate Status",
        [
            ("New",         "New"),
            ("Screening",   "Screening"),
            ("Interview",   "Interview"),
            ("Offer",       "Offer"),
            ("Hired",       "Hired"),
            ("Rejected",    "Rejected"),
        ]),
        ("JobStatus", "Job Status",
        [
            ("Open",    "Open"),
            ("OnHold",  "On Hold"),
            ("Closed",  "Closed"),
        ]),
        ("LeaveType", "Leave Type",
        [
            ("Annual",   "Annual Leave"),
            ("Sick",     "Sick Leave"),
            ("Maternity","Maternity Leave"),
            ("Unpaid",   "Unpaid Leave"),
        ]),
        ("Department", "Department",
        [
            ("HR",          "Human Resources"),
            ("IT",          "Information Technology"),
            ("Finance",     "Finance"),
            ("Operations",  "Operations"),
            ("Training",    "Training"),
        ]),
    ];

    public MasterDataService(IUnitOfWork uow, ILogger<MasterDataService> logger)
    {
        _uow = uow;
        _logger = logger;
    }

    public async Task<IEnumerable<MasterCategoryDto>> GetCategoriesAsync(int tenantId, bool includeInactive = false)
    {
        var categories = await _uow.MasterCategory.GetByTenantIdAsync(tenantId, includeInactive);
        var result = new List<MasterCategoryDto>();
        foreach (var c in categories)
        {
            var values = await _uow.MasterValue.GetByCategoryIdAsync(c.MasterCategoryId, includeInactive);
            result.Add(ToCategoryDto(c, values));
        }
        return result;
    }

    public async Task<MasterCategoryDto?> GetCategoryByIdAsync(int categoryId, int tenantId)
    {
        var c = await _uow.MasterCategory.GetByIdAsync(categoryId, tenantId);
        if (c is null) return null;
        var values = await _uow.MasterValue.GetByCategoryIdAsync(categoryId, true);
        return ToCategoryDto(c, values);
    }

    public async Task<IEnumerable<MasterValueDto>> GetValuesByCategoryCodeAsync(string categoryCode, int tenantId)
    {
        var values = await _uow.MasterValue.GetByCategoryCodeAsync(categoryCode, tenantId);
        return values.Select(ToValueDto);
    }

    public async Task<IEnumerable<MasterValueDto>> GetValuesByCategoryIdAsync(int categoryId, bool includeInactive = false)
    {
        var values = await _uow.MasterValue.GetByCategoryIdAsync(categoryId, includeInactive);
        return values.Select(ToValueDto);
    }

    public async Task<MasterCategoryDto> CreateCategoryAsync(CreateMasterCategoryRequest req, int tenantId, string createdBy)
    {
        var entity = new MasterCategory
        {
            TenantId     = tenantId,
            CategoryCode = req.CategoryCode.Trim(),
            CategoryName = req.CategoryName.Trim(),
            IsActive     = true,
            CreatedBy    = createdBy,
            CreatedDate  = DateTime.UtcNow
        };
        await _uow.MasterCategory.AddAsync(entity);
        await _uow.SaveChangesAsync();
        return ToCategoryDto(entity, []);
    }

    public async Task<MasterValueDto> CreateValueAsync(CreateMasterValueRequest req, int tenantId, string createdBy)
    {
        var entity = new MasterValue
        {
            TenantId          = tenantId,
            MasterCategoryId  = req.MasterCategoryId,
            ValueCode         = req.ValueCode.Trim(),
            ValueName         = req.ValueName.Trim(),
            SortOrder         = req.SortOrder,
            IsActive          = true,
            CreatedBy         = createdBy,
            CreatedDate       = DateTime.UtcNow
        };
        await _uow.MasterValue.AddAsync(entity);
        await _uow.SaveChangesAsync();
        return ToValueDto(entity);
    }

    public async Task UpdateValueAsync(UpdateMasterValueRequest req, int tenantId, string updatedBy)
    {
        var entity = await _uow.MasterValue.GetByIdAsync(req.MasterValueId, tenantId)
                     ?? throw new KeyNotFoundException($"MasterValue {req.MasterValueId} not found");
        entity.ValueCode   = req.ValueCode.Trim();
        entity.ValueName   = req.ValueName.Trim();
        entity.SortOrder   = req.SortOrder;
        entity.IsActive    = req.IsActive;
        entity.UpdatedBy   = updatedBy;
        entity.UpdatedDate = DateTime.UtcNow;
        await _uow.MasterValue.UpdateAsync(entity);
        await _uow.SaveChangesAsync();
    }

    public async Task ToggleValueActiveAsync(int valueId, int tenantId, string updatedBy)
    {
        var entity = await _uow.MasterValue.GetByIdAsync(valueId, tenantId)
                     ?? throw new KeyNotFoundException($"MasterValue {valueId} not found");
        entity.IsActive    = !entity.IsActive;
        entity.UpdatedBy   = updatedBy;
        entity.UpdatedDate = DateTime.UtcNow;
        await _uow.MasterValue.UpdateAsync(entity);
        await _uow.SaveChangesAsync();
    }

    public async Task<bool> HasCategoriesAsync(int tenantId)
    {
        var cats = await _uow.MasterCategory.GetByTenantIdAsync(tenantId);
        return cats.Any();
    }

    public async Task SeedDefaultCategoriesAsync(int tenantId, string createdBy)
    {
        if (await HasCategoriesAsync(tenantId)) return;

        for (int ci = 0; ci < DefaultCategories.Count; ci++)
        {
            var (catCode, catName, values) = DefaultCategories[ci];
            var category = new MasterCategory
            {
                TenantId     = tenantId,
                CategoryCode = catCode,
                CategoryName = catName,
                IsActive     = true,
                CreatedBy    = createdBy,
                CreatedDate  = DateTime.UtcNow
            };
            await _uow.MasterCategory.AddAsync(category);
            await _uow.SaveChangesAsync();

            for (int vi = 0; vi < values.Count; vi++)
            {
                var (vCode, vName) = values[vi];
                await _uow.MasterValue.AddAsync(new MasterValue
                {
                    TenantId         = tenantId,
                    MasterCategoryId = category.MasterCategoryId,
                    ValueCode        = vCode,
                    ValueName        = vName,
                    SortOrder        = vi,
                    IsActive         = true,
                    CreatedBy        = createdBy,
                    CreatedDate      = DateTime.UtcNow
                });
            }
            await _uow.SaveChangesAsync();
        }
        _logger.LogInformation("Seeded default master data for tenant {TenantId}", tenantId);
    }

    private static MasterCategoryDto ToCategoryDto(MasterCategory c, IEnumerable<MasterValue> values) => new()
    {
        MasterCategoryId = c.MasterCategoryId,
        CategoryCode     = c.CategoryCode,
        CategoryName     = c.CategoryName,
        IsActive         = c.IsActive,
        Values           = values.Select(ToValueDto).ToList()
    };

    private static MasterValueDto ToValueDto(MasterValue v) => new()
    {
        MasterValueId    = v.MasterValueId,
        MasterCategoryId = v.MasterCategoryId,
        ValueCode        = v.ValueCode,
        ValueName        = v.ValueName,
        SortOrder        = v.SortOrder,
        IsActive         = v.IsActive
    };
}

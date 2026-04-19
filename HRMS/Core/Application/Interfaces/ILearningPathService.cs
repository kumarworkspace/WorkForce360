using HRMS.Core.Application.DTOs;

namespace HRMS.Core.Application.Interfaces;

public interface ILearningPathService
{
    Task<IEnumerable<LearningPathDto>> GetAllAsync(int tenantId, bool includeInactive = false);
    Task<LearningPathDto?> GetByIdAsync(int pathId, int tenantId);
    Task<(bool Success, string Message, int PathId)> CreateAsync(CreateLearningPathRequest request);
    Task<(bool Success, string Message)> UpdateAsync(int pathId, CreateLearningPathRequest request);
    Task<(bool Success, string Message)> DeleteAsync(int pathId, int tenantId, string deletedBy);
}

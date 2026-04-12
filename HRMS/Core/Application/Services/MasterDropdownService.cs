using HRMS.Core.Application.Interfaces;
using HRMS.Core.Domain.Entities;
using HRMS.Core.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace HRMS.Core.Application.Services;

public class MasterDropdownService : IMasterDropdownService
{
    private readonly ILogger<MasterDropdownService> _logger;
    private readonly IUnitOfWork _unitOfWork;

    public MasterDropdownService(
        ILogger<MasterDropdownService> logger,
        IUnitOfWork unitOfWork)
    {
        _logger = logger;
        _unitOfWork = unitOfWork;
    }

    public async Task<IEnumerable<MasterDropdown>> GetByCategoryAsync(string category, int tenantId)
    {
        try
        {
            return await _unitOfWork.MasterDropdown.GetByCategoryAsync(category, tenantId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting dropdown items for category: {Category}, tenant: {TenantId}", category, tenantId);
            throw;
        }
    }

    public async Task<IEnumerable<MasterDropdown>> GetAllAsync(int tenantId)
    {
        try
        {
            return await _unitOfWork.MasterDropdown.GetActiveByTenantIdAsync(tenantId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all dropdown items for tenant: {TenantId}", tenantId);
            throw;
        }
    }
}







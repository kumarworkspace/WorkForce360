using Microsoft.EntityFrameworkCore;
using HRMS.Core.Domain.Entities;
using HRMS.Core.Domain.Interfaces;
using HRMS.Infrastructure.Data;

namespace HRMS.Infrastructure.Repositories;

public class LegalDocumentRepository : Repository<LegalDocument>, ILegalDocumentRepository
{
    public LegalDocumentRepository(HRMSDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<LegalDocument>> GetByStaffIdAsync(int staffId, int tenantId)
    {
        return await _dbSet
            .Where(d => d.StaffId == staffId && d.TenantId == tenantId && d.IsActive)
            .OrderBy(d => d.DocumentType)
            .ToListAsync();
    }
}

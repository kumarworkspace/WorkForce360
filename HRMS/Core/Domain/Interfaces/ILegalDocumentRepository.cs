using HRMS.Core.Domain.Entities;

namespace HRMS.Core.Domain.Interfaces;

public interface ILegalDocumentRepository : IRepository<LegalDocument>
{
    Task<IEnumerable<LegalDocument>> GetByStaffIdAsync(int staffId, int tenantId);
}


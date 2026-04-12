using Microsoft.EntityFrameworkCore;
using Npgsql;
using HRMS.Core.Application.DTOs;
using HRMS.Core.Domain.Entities;
using HRMS.Core.Domain.Interfaces;
using HRMS.Infrastructure.Data;

namespace HRMS.Infrastructure.Repositories;

public class StaffRepository : Repository<Staff>, IStaffRepository
{
    public StaffRepository(HRMSDbContext context) : base(context)
    {
    }

    public async Task<(IEnumerable<StaffListSpDto> Items, int TotalCount)> GetStaffListSpAsync(GetStaffListRequest request)
    {
        var parameters = new[]
        {
            new NpgsqlParameter("@TenantId", request.TenantId),
            new NpgsqlParameter("@SearchTerm", (object?)request.SearchTerm ?? DBNull.Value),
            new NpgsqlParameter("@Division", (object?)request.Division ?? DBNull.Value),
            new NpgsqlParameter("@Department", (object?)request.Department ?? DBNull.Value),
            new NpgsqlParameter("@IsActive", (object?)request.IsActive ?? DBNull.Value),
            new NpgsqlParameter("@PageNumber", request.PageNumber),
            new NpgsqlParameter("@PageSize", request.PageSize)
        };

        // PostgreSQL function returns single result set with TotalCount as a column (COUNT(*) OVER())
        using var command = _context.Database.GetDbConnection().CreateCommand();
        command.CommandText = "SELECT * FROM usp_GetStaffList(@TenantId, @SearchTerm, @Division, @Department, @IsActive, @PageNumber, @PageSize)";
        command.CommandType = System.Data.CommandType.Text;
        command.Parameters.AddRange(parameters);

        await _context.Database.OpenConnectionAsync();
        try
        {
            using var reader = await command.ExecuteReaderAsync();

            int totalCount = 0;
            var items = new List<StaffListSpDto>();
            while (await reader.ReadAsync())
            {
                totalCount = reader.GetInt32(reader.GetOrdinal("TotalCount"));
                items.Add(new StaffListSpDto
                {
                    StaffId = reader.GetInt32(reader.GetOrdinal("StaffId")),
                    EmployeeCode = reader.IsDBNull(reader.GetOrdinal("EmployeeCode")) ? string.Empty : reader.GetString(reader.GetOrdinal("EmployeeCode")),
                    Name = reader.IsDBNull(reader.GetOrdinal("Name")) ? string.Empty : reader.GetString(reader.GetOrdinal("Name")),
                    Email = reader.IsDBNull(reader.GetOrdinal("Email")) ? string.Empty : reader.GetString(reader.GetOrdinal("Email")),
                    PhoneNumber = reader.IsDBNull(reader.GetOrdinal("PhoneNumber")) ? null : reader.GetString(reader.GetOrdinal("PhoneNumber")),
                    Company = reader.IsDBNull(reader.GetOrdinal("Company")) ? null : reader.GetString(reader.GetOrdinal("Company")),
                    Division = reader.IsDBNull(reader.GetOrdinal("Division")) ? string.Empty : reader.GetString(reader.GetOrdinal("Division")),
                    Department = reader.IsDBNull(reader.GetOrdinal("Department")) ? string.Empty : reader.GetString(reader.GetOrdinal("Department")),
                    Position = reader.IsDBNull(reader.GetOrdinal("Position")) ? string.Empty : reader.GetString(reader.GetOrdinal("Position")),
                    DateOfBirth = reader.IsDBNull(reader.GetOrdinal("DateOfBirth")) ? null : reader.GetDateTime(reader.GetOrdinal("DateOfBirth")),
                    DateJoined = reader.IsDBNull(reader.GetOrdinal("DateJoined")) ? null : reader.GetDateTime(reader.GetOrdinal("DateJoined")),
                    RetirementDate = reader.IsDBNull(reader.GetOrdinal("RetirementDate")) ? null : reader.GetDateTime(reader.GetOrdinal("RetirementDate")),
                    Photo = reader.IsDBNull(reader.GetOrdinal("Photo")) ? null : reader.GetString(reader.GetOrdinal("Photo")),
                    Address = reader.IsDBNull(reader.GetOrdinal("Address")) ? null : reader.GetString(reader.GetOrdinal("Address")),
                    IdentityCard = reader.IsDBNull(reader.GetOrdinal("IdentityCard")) ? null : reader.GetString(reader.GetOrdinal("IdentityCard")),
                    GenderId = reader.IsDBNull(reader.GetOrdinal("GenderId")) ? null : reader.GetInt32(reader.GetOrdinal("GenderId")),
                    GenderName = reader.IsDBNull(reader.GetOrdinal("GenderName")) ? null : reader.GetString(reader.GetOrdinal("GenderName")),
                    EmploymentStatusId = reader.IsDBNull(reader.GetOrdinal("EmploymentStatusId")) ? null : reader.GetInt32(reader.GetOrdinal("EmploymentStatusId")),
                    EmploymentStatusName = reader.IsDBNull(reader.GetOrdinal("EmploymentStatusName")) ? null : reader.GetString(reader.GetOrdinal("EmploymentStatusName")),
                    ReportingManagerId = reader.IsDBNull(reader.GetOrdinal("ReportingManagerId")) ? null : reader.GetInt32(reader.GetOrdinal("ReportingManagerId")),
                    ReportingManagerName = reader.IsDBNull(reader.GetOrdinal("ReportingManagerName")) ? null : reader.GetString(reader.GetOrdinal("ReportingManagerName")),
                    ReportingManagerCode = reader.IsDBNull(reader.GetOrdinal("ReportingManagerCode")) ? null : reader.GetString(reader.GetOrdinal("ReportingManagerCode")),
                    IsActive = reader.GetBoolean(reader.GetOrdinal("IsActive")),
                    TenantId = reader.GetInt32(reader.GetOrdinal("TenantId")),
                    CreatedDate = reader.GetDateTime(reader.GetOrdinal("CreatedDate")),
                    CreatedBy = reader.IsDBNull(reader.GetOrdinal("CreatedBy")) ? null : reader.GetInt32(reader.GetOrdinal("CreatedBy")),
                    UpdatedDate = reader.IsDBNull(reader.GetOrdinal("UpdatedDate")) ? null : reader.GetDateTime(reader.GetOrdinal("UpdatedDate")),
                    UpdatedBy = reader.IsDBNull(reader.GetOrdinal("UpdatedBy")) ? null : reader.GetInt32(reader.GetOrdinal("UpdatedBy"))
                });
            }

            return (items, totalCount);
        }
        finally
        {
            await _context.Database.CloseConnectionAsync();
        }
    }

    public async Task<Staff?> GetByIdWithDetailsAsync(int staffId, int tenantId)
    {
        return await _dbSet
            .Where(s => s.StaffId == staffId && s.TenantId == tenantId && s.IsActive)
            .Include(s => s.EducationDetails.Where(e => e.IsActive))
            .Include(s => s.ExperienceDetails.Where(e => e.IsActive))
            .Include(s => s.LegalDocuments.Where(d => d.IsActive))
            .FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<Staff>> GetByTenantIdAsync(int tenantId)
    {
        var staff = await _dbSet
            .Where(s => s.TenantId == tenantId && s.IsActive)
            .ToListAsync();
        
        return staff.OrderBy(s => s.Name ?? string.Empty).ToList();
    }

    public async Task<bool> EmailExistsAsync(string email, int tenantId, int? excludeStaffId = null)
    {
        if (string.IsNullOrWhiteSpace(email))
            return false;
            
        var emailLower = email.ToLower().Trim();
        var query = _dbSet.Where(s => s.IsActive && s.Email != null && s.Email.ToLower().Trim() == emailLower && s.TenantId == tenantId);
        
        if (excludeStaffId.HasValue)
        {
            query = query.Where(s => s.StaffId != excludeStaffId.Value);
        }
        
        return await query.AnyAsync();
    }

    public async Task<IEnumerable<Staff>> SearchAsync(int tenantId, string? searchTerm = null, string? division = null, string? department = null)
    {
        // First, get all staff for the tenant
        var allStaff = await _dbSet
            .Where(s => s.TenantId == tenantId && s.IsActive)
            .ToListAsync();

        // Then filter in memory to avoid SQL null issues
        var query = allStaff.AsQueryable();

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var term = searchTerm.ToLower();
            query = query.Where(s => 
                (!string.IsNullOrEmpty(s.Name) && s.Name.ToLower().Contains(term)) ||
                (!string.IsNullOrEmpty(s.Department) && s.Department.ToLower().Contains(term)) ||
                (!string.IsNullOrEmpty(s.Division) && s.Division.ToLower().Contains(term)));
        }

        if (!string.IsNullOrWhiteSpace(division))
        {
            query = query.Where(s => !string.IsNullOrEmpty(s.Division) && s.Division == division);
        }

        if (!string.IsNullOrWhiteSpace(department))
        {
            query = query.Where(s => !string.IsNullOrEmpty(s.Department) && s.Department == department);
        }

        // Order by Name, handling nulls
        return query.OrderBy(s => s.Name ?? string.Empty).ToList();
    }

    public async Task<Staff?> GetLastByTenantAsync(int tenantId)
    {
        return await _dbSet
            .Where(s => s.TenantId == tenantId && s.IsActive)
            .OrderByDescending(s => s.StaffId)
            .FirstOrDefaultAsync();
    }
}

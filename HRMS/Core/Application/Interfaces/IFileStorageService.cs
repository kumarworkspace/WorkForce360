namespace HRMS.Core.Application.Interfaces;

public interface IFileStorageService
{
    Task<string> SaveStaffPhotoAsync(int tenantId, int staffId, Stream fileStream, string fileName);
    Task<string> SaveLegalDocumentAsync(int tenantId, int staffId, Stream fileStream, string fileName);
    Task<string> SaveLeaveAttachmentAsync(int tenantId, int staffId, int requestId, Stream fileStream, string fileName);
    Task<bool> DeleteFileAsync(string filePath);
    Task<Stream?> GetFileAsync(string filePath);
    string GetFileUrl(string filePath);
}

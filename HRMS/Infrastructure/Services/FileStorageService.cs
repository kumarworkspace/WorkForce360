using HRMS.Core.Application.Interfaces;

namespace HRMS.Infrastructure.Services;

public class FileStorageService : IFileStorageService
{
    private readonly IWebHostEnvironment _environment;
    private readonly ILogger<FileStorageService> _logger;
    private const string UploadsFolder = "uploads";
    private const long MaxFileSize = 5 * 1024 * 1024; // 5MB
    private static readonly string[] AllowedPhotoExtensions = { ".jpg", ".jpeg", ".png", ".gif" };
    private static readonly string[] AllowedDocumentExtensions = { ".pdf", ".doc", ".docx", ".jpg", ".jpeg", ".png" };
    private static readonly string[] AllowedLeaveAttachmentExtensions = { ".pdf", ".jpg", ".jpeg", ".png" };

    public FileStorageService(IWebHostEnvironment environment, ILogger<FileStorageService> logger)
    {
        _environment = environment;
        _logger = logger;
    }

    public async Task<string> SaveStaffPhotoAsync(int tenantId, int staffId, Stream fileStream, string fileName)
    {
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        if (!AllowedPhotoExtensions.Contains(extension))
        {
            throw new ArgumentException($"File type {extension} is not allowed for photos. Allowed types: {string.Join(", ", AllowedPhotoExtensions)}");
        }

        var folderPath = Path.Combine(_environment.WebRootPath ?? _environment.ContentRootPath, UploadsFolder, tenantId.ToString(), staffId.ToString(), "photos");
        Directory.CreateDirectory(folderPath);

        var uniqueFileName = $"{Guid.NewGuid()}{extension}";
        var filePath = Path.Combine(folderPath, uniqueFileName);

        using (var fileStreamWriter = new FileStream(filePath, FileMode.Create))
        {
            await fileStream.CopyToAsync(fileStreamWriter);
        }

        var relativePath = Path.Combine(UploadsFolder, tenantId.ToString(), staffId.ToString(), "photos", uniqueFileName).Replace('\\', '/');
        return relativePath;
    }

    public async Task<string> SaveLegalDocumentAsync(int tenantId, int staffId, Stream fileStream, string fileName)
    {
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        if (!AllowedDocumentExtensions.Contains(extension))
        {
            throw new ArgumentException($"File type {extension} is not allowed for documents. Allowed types: {string.Join(", ", AllowedDocumentExtensions)}");
        }

        var folderPath = Path.Combine(_environment.WebRootPath ?? _environment.ContentRootPath, UploadsFolder, tenantId.ToString(), staffId.ToString(), "documents");
        Directory.CreateDirectory(folderPath);

        var safeFileName = Path.GetFileNameWithoutExtension(fileName);
        var uniqueFileName = $"{Guid.NewGuid()}_{safeFileName}{extension}";
        var filePath = Path.Combine(folderPath, uniqueFileName);

        using (var fileStreamWriter = new FileStream(filePath, FileMode.Create))
        {
            await fileStream.CopyToAsync(fileStreamWriter);
        }

        var relativePath = Path.Combine(UploadsFolder, tenantId.ToString(), staffId.ToString(), "documents", uniqueFileName).Replace('\\', '/');
        return relativePath;
    }

    public async Task<string> SaveLeaveAttachmentAsync(int tenantId, int staffId, int requestId, Stream fileStream, string fileName)
    {
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        if (!AllowedLeaveAttachmentExtensions.Contains(extension))
        {
            throw new ArgumentException($"File type {extension} is not allowed for leave attachments. Allowed types: {string.Join(", ", AllowedLeaveAttachmentExtensions)}");
        }

        var folderPath = Path.Combine(_environment.WebRootPath ?? _environment.ContentRootPath, UploadsFolder, tenantId.ToString(), staffId.ToString(), "leave-attachments");
        Directory.CreateDirectory(folderPath);

        var safeFileName = Path.GetFileNameWithoutExtension(fileName);
        var uniqueFileName = $"{requestId}_{Guid.NewGuid()}_{safeFileName}{extension}";
        var filePath = Path.Combine(folderPath, uniqueFileName);

        using (var fileStreamWriter = new FileStream(filePath, FileMode.Create))
        {
            await fileStream.CopyToAsync(fileStreamWriter);
        }

        var relativePath = Path.Combine(UploadsFolder, tenantId.ToString(), staffId.ToString(), "leave-attachments", uniqueFileName).Replace('\\', '/');
        return relativePath;
    }

    public async Task<bool> DeleteFileAsync(string filePath)
    {
        try
        {
            var fullPath = Path.Combine(_environment.WebRootPath ?? _environment.ContentRootPath, filePath);
            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
                await Task.CompletedTask;
                return true;
            }
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting file: {FilePath}", filePath);
            return false;
        }
    }

    public async Task<Stream?> GetFileAsync(string filePath)
    {
        try
        {
            var fullPath = Path.Combine(_environment.WebRootPath ?? _environment.ContentRootPath, filePath);
            if (File.Exists(fullPath))
            {
                return new FileStream(fullPath, FileMode.Open, FileAccess.Read);
            }
            await Task.CompletedTask;
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reading file: {FilePath}", filePath);
            return null;
        }
    }

    public string GetFileUrl(string filePath)
    {
        return $"/{filePath.Replace('\\', '/')}";
    }
}

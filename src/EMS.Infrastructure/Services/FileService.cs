using EMS.Application.Interfaces.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

namespace EMS.Infrastructure.Services;

// Implementation stays in Infrastructure
// But now implements the interface from Application
public class FileService : IFileService
{
    private readonly string _uploadsBasePath;

    private static readonly string[] AllowedMimeTypes =
        { "image/jpeg", "image/jpg", "image/png", "image/gif", "image/webp" };

    private static readonly string[] AllowedExtensions =
        { ".jpg", ".jpeg", ".png", ".gif", ".webp" };

    private const long MaxFileSizeBytes = 5 * 1024 * 1024; // 5MB

    public FileService(IWebHostEnvironment env)
    {
        _uploadsBasePath = Path.Combine(
            env.WebRootPath ?? env.ContentRootPath,
            "uploads", "profiles");

        Directory.CreateDirectory(_uploadsBasePath);
    }

    public bool IsValidImageFile(IFormFile file)
    {
        if (file.Length == 0 || file.Length > MaxFileSizeBytes)
            return false;

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!AllowedExtensions.Contains(extension))
            return false;

        if (!AllowedMimeTypes.Contains(file.ContentType.ToLowerInvariant()))
            return false;

        return true;
    }

    public async Task<string> SaveProfilePhotoAsync(IFormFile file, string employeeCode)
    {
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        var fileName = $"PROFILE_{employeeCode}_{DateTime.UtcNow:yyyyMMdd_HHmmss}{extension}";
        var fullPath = Path.Combine(_uploadsBasePath, fileName);

        await using var stream = new FileStream(fullPath, FileMode.Create);
        await file.CopyToAsync(stream);

        return $"/uploads/profiles/{fileName}";
    }

    public void DeleteFile(string? filePath)
    {
        if (string.IsNullOrEmpty(filePath)) return;

        var fileName = Path.GetFileName(filePath);
        var physicalPath = Path.Combine(_uploadsBasePath, fileName);

        if (File.Exists(physicalPath))
            File.Delete(physicalPath);
    }
}
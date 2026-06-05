using Microsoft.AspNetCore.Http;

namespace EMS.Application.Interfaces.Services;

// Interface lives in Application (no infrastructure dependency)
// Implementation lives in Infrastructure
public interface IFileService
{
    Task<string> SaveProfilePhotoAsync(IFormFile file, string employeeCode);
    void DeleteFile(string? filePath);
    bool IsValidImageFile(IFormFile file);
}
using Microsoft.AspNetCore.Http;

namespace Document_Manager.Services.Interfaces
{
    public interface IFileStorageService
    {
        Task<string> SaveFileAsync(IFormFile file, Guid documentId);
        Task<byte[]?> GetFileAsync(string filePath);
        Task<bool> DeleteFileAsync(string filePath);
        bool ValidateFileType(IFormFile file);
        string GetFileExtension(IFormFile file);
        string GetContentType(IFormFile file);
        long GetFileSize(IFormFile file);
    }
}

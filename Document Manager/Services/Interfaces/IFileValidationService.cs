using Document_Manager.DTOs;
using Document_Manager.Models;
using Microsoft.AspNetCore.Http;

namespace Document_Manager.Services.Interfaces
{
    public interface IFileValidationService
    {
        Task<FileValidationResultDto> ValidateFileAsync(IFormFile file);
        Task<List<FileValidation>> GetAllowedFileTypesAsync();
        Task<FileValidation> AddFileTypeAsync(FileTypeDto fileType);
        Task<FileValidation> UpdateFileTypeAsync(Guid id, FileTypeDto fileType);
        Task<bool> RemoveFileTypeAsync(Guid id);
        Task<FileValidation> GetFileTypeByExtensionAsync(string extension);
    }
}

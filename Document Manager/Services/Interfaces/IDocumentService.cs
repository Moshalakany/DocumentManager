using Document_Manager.DTOs;
using Document_Manager.Models;
using Microsoft.AspNetCore.Http;

namespace Document_Manager.Services.Interfaces
{
    public interface IDocumentService
    {
        Task<Document> UploadDocumentAsync(DocumentUploadDto documentDto, Guid userId);
        Task<Document?> GetDocumentByIdAsync(Guid id);
        Task<List<Document>> GetUserDocumentsAsync(Guid userId);
        Task<bool> DeleteDocumentAsync(Guid id, Guid userId);
        Task<FileValidationResultDto> ValidateFileAsync(IFormFile file);
        Task<List<Document>> SearchDocumentsByTagsAsync(List<Guid> tagIds, Guid userId);
        
        // New folder-related document operations
        Task<Document?> MoveDocumentAsync(DocumentMoveDto moveDto, Guid userId);
        Task<List<Document>> GetDocumentsByFolderIdAsync(Guid folderId, Guid userId);
    }
}

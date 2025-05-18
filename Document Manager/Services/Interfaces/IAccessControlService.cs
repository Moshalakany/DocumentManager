using Document_Manager.DTOs;
using Document_Manager.Models;

namespace Document_Manager.Services.Interfaces
{
    public interface IAccessControlService
    {
        // Document permissions
        Task<bool> AssignDocumentPermissionToUserAsync(Guid documentId, Guid userId, PermissionDto permissions);
        Task<bool> AssignDocumentPermissionToGroupAsync(Guid documentId, int groupId, PermissionDto permissions);
        Task<bool> RemoveDocumentPermissionForUserAsync(Guid documentId, Guid userId);
        Task<bool> RemoveDocumentPermissionForGroupAsync(Guid documentId, int groupId);
        Task<List<DocumentPermissionDto>> GetDocumentPermissionsAsync(Guid documentId);
        
        // User permissions
        Task<List<DocumentPermissionDto>> GetUserAccessibleDocumentsAsync(Guid userId);
        
        // Authorization checks
        Task<bool> CanUserViewDocumentAsync(Guid documentId, Guid userId);
        Task<bool> CanUserEditDocumentAsync(Guid documentId, Guid userId);
        Task<bool> CanUserDownloadDocumentAsync(Guid documentId, Guid userId);
        Task<bool> CanUserDeleteDocumentAsync(Guid documentId, Guid userId);
        Task<bool> CanUserShareDocumentAsync(Guid documentId, Guid userId);
        Task<bool> CanUserAnnotateDocumentAsync(Guid documentId, Guid userId);
        
        // Helper methods
        Task<PermissionDto> GetUserDocumentPermissionsAsync(Guid documentId, Guid userId);
        Task<bool> IsDocumentOwnerAsync(Guid documentId, Guid userId);
    }
}

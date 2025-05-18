using Document_Manager.DTOs;
using Document_Manager.Models;

namespace Document_Manager.Services.Interfaces
{
    public interface IFolderService
    {
        Task<Folder> CreateFolderAsync(FolderCreateDto folderDto, Guid userId);
        Task<Folder?> GetFolderByIdAsync(Guid folderId);
        Task<List<Folder>> GetUserFoldersAsync(Guid userId);
        Task<List<Folder>> GetRootFoldersAsync(Guid userId);
        Task<List<Folder>> GetSubFoldersAsync(Guid parentFolderId, Guid userId);
        Task<Folder?> UpdateFolderAsync(Guid folderId, FolderUpdateDto folderDto, Guid userId);
        Task<bool> DeleteFolderAsync(Guid folderId, Guid userId);
        Task<bool> UserHasAccessToFolder(Guid folderId, Guid userId, bool requireEditPermission = false);
    }
}

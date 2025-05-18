using Document_Manager.Data;
using Document_Manager.DTOs;
using Document_Manager.Models;
using Document_Manager.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Transactions;

namespace Document_Manager.Services
{
    public class FolderService : IFolderService
    {
        private readonly AppDbContextSQL _context;

        public FolderService(AppDbContextSQL context)
        {
            _context = context;
        }

        public async Task<Folder> CreateFolderAsync(FolderCreateDto folderDto, Guid userId)
        {
            using var transaction = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

            // Validate parent folder exists and user has access to it
            if (folderDto.ParentFolderId.HasValue)
            {
                var parentFolder = await _context.Folders.FindAsync(folderDto.ParentFolderId.Value);
                if (parentFolder == null)
                {
                    throw new InvalidOperationException("Parent folder not found");
                }

                bool hasAccess = await UserHasAccessToFolder(folderDto.ParentFolderId.Value, userId, true);
                if (!hasAccess)
                {
                    throw new UnauthorizedAccessException("You don't have permission to create folders here");
                }
            }

            // Create folder entity
            var folder = new Folder
            {
                Id = Guid.NewGuid(),
                Name = folderDto.Name,
                Description = folderDto.Description,
                ParentFolderId = folderDto.ParentFolderId,
                OwnerId = userId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            // Create accessibility list for the folder
            var accessibilityList = new DocumentAccessibilityList
            {
                Id = Guid.NewGuid()
            };

            // Create initial accessibility list item for the folder owner
            var accessibilityListItem = new DocumentAccessibilityListItem
            {
                Id = Guid.NewGuid(),
                AccessibilityListId = accessibilityList.Id,
                AccessibilityList = accessibilityList,
                UserId = userId,
                AccessLevel = AccessLevel.Owner,
                CanView = true,
                CanEdit = true,
                CanDownload = true,
                CanAnnotate = true,
                CanDelete = true,
                CanShare = true
            };

            folder.AccessibilityList = accessibilityList;
            folder.AccessibilityListId = accessibilityList.Id;

            // Save to database
            await _context.Folders.AddAsync(folder);
            await _context.DocumentAccessibilityLists.AddAsync(accessibilityList);
            await _context.DocumentAccessibilityListItems.AddAsync(accessibilityListItem);
            await _context.SaveChangesAsync();

            transaction.Complete();
            
            return folder;
        }

        public async Task<Folder?> GetFolderByIdAsync(Guid folderId)
        {
            return await _context.Folders
                .Include(f => f.ParentFolder)
                .Include(f => f.SubFolders)
                .Include(f => f.Documents)
                .Include(f => f.Owner)
                .Include(f => f.AccessibilityList)
                    .ThenInclude(a => a.AccessibilityListItems)
                .FirstOrDefaultAsync(f => f.Id == folderId);
        }

        public async Task<List<Folder>> GetUserFoldersAsync(Guid userId)
        {
            // Get folders owned by user
            var ownedFolders = await _context.Folders
                .Where(f => f.OwnerId == userId)
                .ToListAsync();

            // Get folders shared with user through accessibility lists
            var sharedFolderIds = await _context.DocumentAccessibilityListItems
                .Where(a => a.UserId == userId && a.CanView)
                .Join(_context.Folders,
                    item => item.AccessibilityListId,
                    folder => folder.AccessibilityListId,
                    (item, folder) => folder.Id)
                .ToListAsync();

            var sharedFolders = await _context.Folders
                .Where(f => sharedFolderIds.Contains(f.Id))
                .ToListAsync();

            // Combine and return distinct folders
            return ownedFolders.Union(sharedFolders).ToList();
        }

        public async Task<List<Folder>> GetRootFoldersAsync(Guid userId)
        {
            var userFolders = await GetUserFoldersAsync(userId);
            return userFolders.Where(f => f.ParentFolderId == null).ToList();
        }

        public async Task<List<Folder>> GetSubFoldersAsync(Guid parentFolderId, Guid userId)
        {
            // Check if user has access to the parent folder
            bool hasAccess = await UserHasAccessToFolder(parentFolderId, userId);
            if (!hasAccess)
            {
                throw new UnauthorizedAccessException("You don't have access to this folder");
            }

            return await _context.Folders
                .Where(f => f.ParentFolderId == parentFolderId)
                .ToListAsync();
        }

        public async Task<Folder?> UpdateFolderAsync(Guid folderId, FolderUpdateDto folderDto, Guid userId)
        {
            var folder = await _context.Folders.FindAsync(folderId);
            if (folder == null)
            {
                return null;
            }

            // Check if user has permission to edit
            bool hasAccess = await UserHasAccessToFolder(folderId, userId, true);
            if (!hasAccess)
            {
                throw new UnauthorizedAccessException("You don't have permission to edit this folder");
            }

            // Check if target parent folder exists and user has access
            if (folderDto.ParentFolderId.HasValue && folderDto.ParentFolderId != folder.ParentFolderId)
            {
                // Prevent circular reference
                if (folderDto.ParentFolderId == folderId)
                {
                    throw new InvalidOperationException("A folder cannot be its own parent");
                }

                var parentFolder = await _context.Folders.FindAsync(folderDto.ParentFolderId.Value);
                if (parentFolder == null)
                {
                    throw new InvalidOperationException("Parent folder not found");
                }

                bool hasParentAccess = await UserHasAccessToFolder(folderDto.ParentFolderId.Value, userId, true);
                if (!hasParentAccess)
                {
                    throw new UnauthorizedAccessException("You don't have permission to move to the target folder");
                }

                folder.ParentFolderId = folderDto.ParentFolderId;
            }

            // Update properties
            if (!string.IsNullOrWhiteSpace(folderDto.Name))
            {
                folder.Name = folderDto.Name;
            }
            
            if (folderDto.Description != null) // Allow clearing description
            {
                folder.Description = folderDto.Description;
            }
            
            folder.UpdatedAt = DateTime.UtcNow;
            
            await _context.SaveChangesAsync();
            return folder;
        }

        public async Task<bool> DeleteFolderAsync(Guid folderId, Guid userId)
        {
            var folder = await _context.Folders
                .Include(f => f.SubFolders)
                .Include(f => f.Documents)
                .FirstOrDefaultAsync(f => f.Id == folderId);
                
            if (folder == null)
            {
                return false;
            }

            // Check if user has permission to delete
            bool hasAccess = folder.OwnerId == userId || 
                await _context.DocumentAccessibilityListItems
                    .AnyAsync(a => a.AccessibilityListId == folder.AccessibilityListId && a.UserId == userId && a.CanDelete);

            if (!hasAccess)
            {
                return false;
            }

            // Check if folder is empty
            if ((folder.SubFolders != null && folder.SubFolders.Count > 0) ||
                (folder.Documents != null && folder.Documents.Count > 0))
            {
                throw new InvalidOperationException("Cannot delete a non-empty folder");
            }

            _context.Folders.Remove(folder);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> UserHasAccessToFolder(Guid folderId, Guid userId, bool requireEditPermission = false)
        {
            var folder = await _context.Folders
                .FirstOrDefaultAsync(f => f.Id == folderId);
                
            if (folder == null)
            {
                return false;
            }

            // Owner has all permissions
            if (folder.OwnerId == userId)
            {
                return true;
            }

            // Check accessibility list for view or edit permission
            var accessItem = await _context.DocumentAccessibilityListItems
                .FirstOrDefaultAsync(a => a.AccessibilityListId == folder.AccessibilityListId && a.UserId == userId);

            if (accessItem == null)
            {
                return false;
            }

            return requireEditPermission ? accessItem.CanEdit : accessItem.CanView;
        }
    }
}

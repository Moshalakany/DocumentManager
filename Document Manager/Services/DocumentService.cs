using Document_Manager.Data;
using Document_Manager.DTOs;
using Document_Manager.Models;
using Document_Manager.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Transactions;

namespace Document_Manager.Services
{
    public class DocumentService : IDocumentService
    {
        private readonly AppDbContextSQL _context;
        private readonly IFileStorageService _fileStorageService;
        private readonly IFileValidationService _fileValidationService;
        private readonly IFolderService _folderService;

        public DocumentService(
            AppDbContextSQL context,
            IFileStorageService fileStorageService,
            IFileValidationService fileValidationService,
            IFolderService folderService)
        {
            _context = context;
            _fileStorageService = fileStorageService;
            _fileValidationService = fileValidationService;
            _folderService = folderService;
        }

        public async Task<Document> UploadDocumentAsync(DocumentUploadDto documentDto, Guid userId)
        {
            // Validate the file first
            var validationResult = await ValidateFileAsync(documentDto.File);
            if (!validationResult.IsValid)
            {
                throw new InvalidOperationException(validationResult.Message);
            }
            
            // Check if folder exists and user has access to it
            if (documentDto.FolderId.HasValue)
            {
                bool hasAccess = await _folderService.UserHasAccessToFolder(documentDto.FolderId.Value, userId, true);
                if (!hasAccess)
                {
                    throw new UnauthorizedAccessException("You don't have permission to upload to this folder");
                }
            }
            
            using var transaction = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
            
            // Create document entity
            var document = new Document
            {
                Id = Guid.NewGuid(),
                Name = documentDto.Title,
                Description = documentDto.Description,
                CreatedById = userId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                FileType = validationResult.FileExtension,
                ContentType = validationResult.ContentType,
                FileSize = validationResult.FileSize,
                IsValidated = true,
                HasOcr = validationResult.SupportsOcr,
                Version = 1,
                IsCurrentVersion = true,
                FolderId = documentDto.FolderId
            };

            // Save file to storage
            document.FilePath = await _fileStorageService.SaveFileAsync(documentDto.File, document.Id);

            // Process tags from names
            if (documentDto.Tags != null && documentDto.Tags.Count > 0)
            {
                foreach (var tagName in documentDto.Tags)
                {
                    var tag = await _context.Tags.FirstOrDefaultAsync(t => t.Name == tagName);
                    if (tag == null)
                    {
                        tag = new Tag
                        {
                            Id = Guid.NewGuid(),
                            Name = tagName,
                            CreatedById = userId,
                            CreatedAt = DateTime.UtcNow
                        };
                        
                        await _context.Tags.AddAsync(tag);
                    }
                    
                    document.Tags.Add(tag);
                }
            }

            // Process tags from IDs
            if (documentDto.TagIds != null && documentDto.TagIds.Count > 0)
            {
                var existingTags = await _context.Tags
                    .Where(t => documentDto.TagIds.Contains(t.Id))
                    .ToListAsync();

                foreach (var tag in existingTags)
                {
                    // Only add if not already added via tag name
                    if (!document.Tags.Any(t => t.Id == tag.Id))
                    {
                        document.Tags.Add(tag);
                    }
                }
            }

            // Create accessibility list for the document
            var accessibilityList = new DocumentAccessibilityList
            {
                Id = Guid.NewGuid(),
                DocumentId = document.Id,
                Document = document
            };

            // Create initial accessibility list item for the document owner
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

            document.AccessibilityList = accessibilityList;
            document.AccessibilityListId = accessibilityList.Id;

            // Save to database
            await _context.Documents.AddAsync(document);
            await _context.DocumentAccessibilityLists.AddAsync(accessibilityList);
            await _context.SaveChangesAsync();

            transaction.Complete();
            
            return document;
        }

        public async Task<Document?> GetDocumentByIdAsync(Guid id)
        {
            return await _context.Documents
                .Include(d => d.CreatedBy)
                .Include(d => d.Tags)
                .Include(d => d.AccessibilityList)
                .ThenInclude(a => a.AccessibilityListItems)
                .FirstOrDefaultAsync(d => d.Id == id && !d.IsDeleted);
        }

        public async Task<List<Document>> GetUserDocumentsAsync(Guid userId)
        {
            // Get documents created by user
            var ownedDocuments = await _context.Documents
                .Include(d => d.Tags)
                .Where(d => d.CreatedById == userId && !d.IsDeleted)
                .ToListAsync();

            // Get documents shared with user through accessibility lists
            var sharedDocumentIds = await _context.DocumentAccessibilityListItems
                .Where(a => a.UserId == userId && a.CanView)
                .Select(a => a.AccessibilityList.DocumentId)
                .ToListAsync();

            var sharedDocuments = await _context.Documents
                .Include(d => d.Tags)
                .Include(d => d.CreatedBy)
                .Where(d => sharedDocumentIds.Contains(d.Id) && !d.IsDeleted)
                .ToListAsync();

            // Combine and return distinct documents
            var result = ownedDocuments.Union(sharedDocuments).ToList();
            return result;
        }

        public async Task<bool> DeleteDocumentAsync(Guid id, Guid userId)
        {
            var document = await _context.Documents.FindAsync(id);
            if (document == null)
            {
                return false;
            }

            // Check if user has permission to delete
            var hasPermission = document.CreatedById == userId || 
                await _context.DocumentAccessibilityListItems
                    .AnyAsync(a => a.AccessibilityList.DocumentId == id && a.UserId == userId && a.CanDelete);

            if (!hasPermission)
            {
                return false;
            }

            // Soft delete
            document.IsDeleted = true;
            document.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<FileValidationResultDto> ValidateFileAsync(IFormFile file)
        {
            return await _fileValidationService.ValidateFileAsync(file);
        }

        public async Task<List<Document>> SearchDocumentsByTagsAsync(List<Guid> tagIds, Guid userId)
        {
            // Get documents that have all specified tags and are accessible to the user
            var query = _context.Documents
                .Include(d => d.Tags)
                .Include(d => d.CreatedBy)
                .Where(d => !d.IsDeleted);

            // Filter by user access
            query = query.Where(d => 
                d.CreatedById == userId || 
                d.AccessibilityList.AccessibilityListItems.Any(a => a.UserId == userId && a.CanView));

            // Filter by tags (documents that have ALL specified tags)
            if (tagIds != null && tagIds.Count > 0)
            {
                foreach (var tagId in tagIds)
                {
                    query = query.Where(d => d.Tags.Any(t => t.Id == tagId));
                }
            }

            return await query.ToListAsync();
        }

        public async Task<Document?> MoveDocumentAsync(DocumentMoveDto moveDto, Guid userId)
        {
            var document = await _context.Documents
                .Include(d => d.Tags)
                .FirstOrDefaultAsync(d => d.Id == moveDto.DocumentId && !d.IsDeleted);

            if (document == null)
            {
                return null;
            }

            // Check if user has permission to modify the document
            var hasPermission = document.CreatedById == userId || 
                await _context.DocumentAccessibilityListItems
                    .AnyAsync(a => a.AccessibilityList.DocumentId == moveDto.DocumentId && 
                                  a.UserId == userId && a.CanEdit);

            if (!hasPermission)
            {
                throw new UnauthorizedAccessException("You don't have permission to move/copy this document");
            }

            // Check if target folder exists and user has access to it
            if (moveDto.TargetFolderId.HasValue)
            {
                bool hasAccess = await _folderService.UserHasAccessToFolder(moveDto.TargetFolderId.Value, userId, true);
                if (!hasAccess)
                {
                    throw new UnauthorizedAccessException("You don't have permission to access the target folder");
                }
            }

            if (moveDto.IsCopy)
            {
                // Create a copy of the document
                var newDocument = new Document
                {
                    Id = Guid.NewGuid(),
                    Name = document.Name + " (Copy)",
                    Description = document.Description,
                    FilePath = document.FilePath, // Reference the same file
                    FileType = document.FileType,
                    ContentType = document.ContentType,
                    FileSize = document.FileSize,
                    CreatedById = userId,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    IsValidated = document.IsValidated,
                    HasOcr = document.HasOcr,
                    OcrText = document.OcrText,
                    Version = 1,
                    IsCurrentVersion = true,
                    FolderId = moveDto.TargetFolderId
                };

                // Copy tags
                if (document.Tags != null)
                {
                    foreach (var tag in document.Tags)
                    {
                        newDocument.Tags.Add(tag);
                    }
                }

                // Create new accessibility list for the document copy
                var accessibilityList = new DocumentAccessibilityList
                {
                    Id = Guid.NewGuid(),
                    DocumentId = newDocument.Id,
                    Document = newDocument
                };

                // Create initial accessibility list item for the document owner
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

                newDocument.AccessibilityList = accessibilityList;
                newDocument.AccessibilityListId = accessibilityList.Id;

                // Save to database
                await _context.Documents.AddAsync(newDocument);
                await _context.DocumentAccessibilityLists.AddAsync(accessibilityList);
                await _context.SaveChangesAsync();

                return newDocument;
            }
            else
            {
                // Move the document to the target folder
                document.FolderId = moveDto.TargetFolderId;
                document.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
                return document;
            }
        }

        public async Task<List<Document>> GetDocumentsByFolderIdAsync(Guid folderId, Guid userId)
        {
            // Check if user has access to the folder
            bool hasAccess = await _folderService.UserHasAccessToFolder(folderId, userId);
            if (!hasAccess)
            {
                throw new UnauthorizedAccessException("You don't have access to this folder");
            }

            // Get documents in the specified folder
            return await _context.Documents
                .Include(d => d.Tags)
                .Include(d => d.CreatedBy)
                .Where(d => d.FolderId == folderId && !d.IsDeleted)
                .ToListAsync();
        }
    }
}

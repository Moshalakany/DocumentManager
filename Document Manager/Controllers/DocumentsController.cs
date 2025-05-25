using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Document_Manager.Services.Interfaces;
using Document_Manager.DTOs;
using System.Security.Claims;

namespace Document_Manager.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class DocumentsController : ControllerBase
    {
        private readonly IDocumentService _documentService;
        private readonly IFileStorageService _fileStorageService;
        private readonly IAccessControlService _accessControlService;

        public DocumentsController(
            IDocumentService documentService, 
            IFileStorageService fileStorageService,
            IAccessControlService accessControlService)
        {
            _documentService = documentService;
            _fileStorageService = fileStorageService;
            _accessControlService = accessControlService;
        }

        [HttpPost("upload")]
        public async Task<ActionResult<DocumentResponseDto>> UploadDocument([FromForm] DocumentUploadDto documentDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Validate file type using the new service
            var validationResult = await _documentService.ValidateFileAsync(documentDto.File);
            if (!validationResult.IsValid)
            {
                return BadRequest(validationResult);
            }

            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized("Invalid user identification");
            }

            try
            {
                var document = await _documentService.UploadDocumentAsync(documentDto, userId);
                
                var response = new DocumentResponseDto
                {
                    Id = document.Id,
                    Name = document.Name,
                    Description = document.Description,
                    FileType = document.FileType,
                    FileSize = document.FileSize,
                    CreatedAt = document.CreatedAt,
                    UpdatedAt = document.UpdatedAt,
                    Version = document.Version,
                    Tags = document.Tags?.Select(t => t.Name) ?? new List<string>(),
                    CreatedBy = new DocumentUserSummaryDto
                    {
                        UserId = document.CreatedById.Value,
                        Username = document.CreatedBy?.Username ?? "Unknown",
                        Email = document.CreatedBy?.Email ?? string.Empty,
                        Role = document.CreatedBy?.Role ?? string.Empty
                    }
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<DocumentResponseDto>> GetDocument(Guid id)
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized("Invalid user identification");
            }

            // Check if user has view permission using the access control service
            var canView = await _accessControlService.CanUserViewDocumentAsync(id, userId);
            if (!canView)
            {
                return Forbid("You do not have permission to view this document");
            }

            var document = await _documentService.GetDocumentByIdAsync(id);
            if (document == null)
            {
                return NotFound();
            }

            var response = new DocumentResponseDto
            {
                Id = document.Id,
                Name = document.Name,
                Description = document.Description,
                FileType = document.FileType,
                FileSize = document.FileSize,
                CreatedAt = document.CreatedAt,
                UpdatedAt = document.UpdatedAt,
                Version = document.Version,
                Tags = document.Tags?.Select(t => t.Name) ?? new List<string>(),
                CreatedBy = new DocumentUserSummaryDto
                {
                    UserId = document.CreatedById.Value,
                    Username = document.CreatedBy?.Username ?? "Unknown",
                    Email = document.CreatedBy?.Email ?? string.Empty,
                    Role = document.CreatedBy?.Role ?? string.Empty
                }
            };

            return Ok(response);
        }

        [HttpGet("download/{id}")]
        public async Task<IActionResult> DownloadDocument(Guid id)
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized("Invalid user identification");
            }

            // Check if user has download permission using the access control service
            var canDownload = await _accessControlService.CanUserDownloadDocumentAsync(id, userId);
            if (!canDownload)
            {
                return Forbid("You do not have permission to download this document");
            }

            var document = await _documentService.GetDocumentByIdAsync(id);
            if (document == null)
            {
                return NotFound();
            }

            if (string.IsNullOrEmpty(document.FilePath) || !System.IO.File.Exists(document.FilePath))
            {
                return NotFound("File not found.");
            }

            var fileBytes = await _fileStorageService.GetFileAsync(document.FilePath);
            if (fileBytes == null)
            {
                return NotFound("File not found.");
            }

            return File(fileBytes, document.ContentType ?? "application/octet-stream", $"{document.Name}{document.FileType}");
        }

        [HttpGet("my-documents")]
        public async Task<ActionResult<IEnumerable<DocumentResponseDto>>> GetUserDocuments()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized("Invalid user identification");
            }

            var documents = await _documentService.GetUserDocumentsAsync(userId);
            
            var response = documents.Select(document => new DocumentResponseDto
            {
                Id = document.Id,
                Name = document.Name,
                Description = document.Description,
                FileType = document.FileType,
                FileSize = document.FileSize,
                CreatedAt = document.CreatedAt,
                UpdatedAt = document.UpdatedAt,
                Version = document.Version,
                Tags = document.Tags?.Select(t => t.Name) ?? new List<string>(),
                CreatedBy = new DocumentUserSummaryDto
                {
                    UserId = document.CreatedById.Value,
                    Username = document.CreatedBy?.Username ?? "Unknown",
                    Email = document.CreatedBy?.Email ?? string.Empty,
                    Role = document.CreatedBy?.Role ?? string.Empty
                }
            });

            return Ok(response);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDocument(Guid id)
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized("Invalid user identification");
            }

            // Check if user has delete permission using the access control service
            var canDelete = await _accessControlService.CanUserDeleteDocumentAsync(id, userId);
            if (!canDelete)
            {
                return Forbid("You do not have permission to delete this document");
            }

            var result = await _documentService.DeleteDocumentAsync(id, userId);
            if (!result)
            {
                return NotFound();
            }

            return NoContent();
        }
    }
}

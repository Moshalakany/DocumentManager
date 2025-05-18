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

        public DocumentsController(IDocumentService documentService, IFileStorageService fileStorageService)
        {
            _documentService = documentService;
            _fileStorageService = fileStorageService;
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
                    CreatedBy = new UserSummaryDto
                    {
                        UserId = document.CreatedById.Value,
                        Username = document.CreatedBy?.Username ?? "Unknown",
                        Email = document.CreatedBy?.Email
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

            var document = await _documentService.GetDocumentByIdAsync(id);
            if (document == null)
            {
                return NotFound();
            }

            // Check if user has access
            var hasAccess = document.CreatedById == userId || 
                document.AccessibilityList?.AccessibilityListItems?.Any(a => a.UserId == userId && a.CanView) == true;

            if (!hasAccess)
            {
                return Forbid();
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
                CreatedBy = new UserSummaryDto
                {
                    UserId = document.CreatedById.Value,
                    Username = document.CreatedBy?.Username ?? "Unknown",
                    Email = document.CreatedBy?.Email
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

            var document = await _documentService.GetDocumentByIdAsync(id);
            if (document == null)
            {
                return NotFound();
            }

            // Check if user has access to download
            var hasAccess = document.CreatedById == userId || 
                document.AccessibilityList?.AccessibilityListItems?.Any(a => a.UserId == userId && a.CanDownload) == true;

            if (!hasAccess)
            {
                return Forbid();
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
                CreatedBy = new UserSummaryDto
                {
                    UserId = document.CreatedById.Value,
                    Username = document.CreatedBy?.Username ?? "Unknown",
                    Email = document.CreatedBy?.Email
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

            var result = await _documentService.DeleteDocumentAsync(id, userId);
            if (!result)
            {
                return NotFound();
            }

            return NoContent();
        }
    }
}

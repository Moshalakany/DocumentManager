using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Document_Manager.DTOs;
using Document_Manager.Services.Interfaces;
using System.Security.Claims;

namespace Document_Manager.Controllers
{
    [Authorize(Roles = "Admin")]
    [Route("api/[controller]")]
    [ApiController]
    public class DocumentPermissionsController : ControllerBase
    {
        private readonly IAccessControlService _accessControlService;
        private readonly IDocumentService _documentService;

        public DocumentPermissionsController(IAccessControlService accessControlService, IDocumentService documentService)
        {
            _accessControlService = accessControlService;
            _documentService = documentService;
        }

        [HttpGet("{documentId}")]
        public async Task<IActionResult> GetDocumentPermissions(Guid documentId)
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized("Invalid user identification");
            }

            // Check if user can manage permissions
            if (!await CanManagePermissions(documentId, userId))
            {
                return Forbid("You do not have permission to view document permissions");
            }

            var permissions = await _accessControlService.GetDocumentPermissionsAsync(documentId);
            return Ok(permissions);
        }

        [HttpGet("user/documents")]
        public async Task<IActionResult> GetUserAccessibleDocuments()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized("Invalid user identification");
            }

            var documents = await _accessControlService.GetUserAccessibleDocumentsAsync(userId);
            return Ok(documents);
        }

        [HttpPost("user")]
        public async Task<IActionResult> AssignDocumentPermissionToUser([FromBody] PermissionAssignmentDto dto)
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized("Invalid user identification");
            }

            // Check if user can manage permissions
            if (!await CanManagePermissions(dto.DocumentId, userId))
            {
                return Forbid("You do not have permission to assign document permissions");
            }

            var result = await _accessControlService.AssignDocumentPermissionToUserAsync(
                dto.DocumentId, dto.UserId, dto.Permissions);

            if (!result)
            {
                return NotFound("Document or user not found");
            }

            return Ok();
        }

        [HttpPost("group")]
        public async Task<IActionResult> AssignDocumentPermissionToGroup([FromBody] GroupPermissionAssignmentDto dto)
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized("Invalid user identification");
            }

            // Check if user can manage permissions
            if (!await CanManagePermissions(dto.DocumentId, userId))
            {
                return Forbid("You do not have permission to assign document permissions");
            }

            var result = await _accessControlService.AssignDocumentPermissionToGroupAsync(
                dto.DocumentId, dto.GroupId, dto.Permissions);

            if (!result)
            {
                return NotFound("Document or group not found");
            }

            return Ok();
        }

        [HttpDelete("{documentId}/user/{targetUserId}")]
        public async Task<IActionResult> RemoveDocumentPermissionForUser(Guid documentId, Guid targetUserId)
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized("Invalid user identification");
            }

            // Check if user can manage permissions
            if (!await CanManagePermissions(documentId, userId))
            {
                return Forbid("You do not have permission to remove document permissions");
            }

            var result = await _accessControlService.RemoveDocumentPermissionForUserAsync(documentId, targetUserId);
            if (!result)
            {
                return NotFound("Document permission not found");
            }

            return NoContent();
        }

        [HttpDelete("{documentId}/group/{groupId}")]
        public async Task<IActionResult> RemoveDocumentPermissionForGroup(Guid documentId, int groupId)
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized("Invalid user identification");
            }

            // Check if user can manage permissions
            if (!await CanManagePermissions(documentId, userId))
            {
                return Forbid("You do not have permission to remove document permissions");
            }

            var result = await _accessControlService.RemoveDocumentPermissionForGroupAsync(documentId, groupId);
            if (!result)
            {
                return NotFound("Group or document not found");
            }

            return NoContent();
        }

        [HttpGet("{documentId}/permissions")]
        public async Task<IActionResult> GetUserDocumentPermissions(Guid documentId)
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized("Invalid user identification");
            }

            var permissions = await _accessControlService.GetUserDocumentPermissionsAsync(documentId, userId);
            return Ok(permissions);
        }

        // Helper method to check if user can manage permissions
        private async Task<bool> CanManagePermissions(Guid documentId, Guid userId)
        {
            // Admin can always manage permissions
            if (User.IsInRole("Admin"))
                return true;

            // Document owner can manage permissions
            return await _accessControlService.IsDocumentOwnerAsync(documentId, userId) ||
                   await _accessControlService.CanUserShareDocumentAsync(documentId, userId);
        }
    }
}

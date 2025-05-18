using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Document_Manager.DTOs;
using Document_Manager.Services.Interfaces;

namespace Document_Manager.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class TagsController : ControllerBase
    {
        private readonly ITagService _tagService;
        private readonly IDocumentService _documentService;

        public TagsController(ITagService tagService, IDocumentService documentService)
        {
            _tagService = tagService;
            _documentService = documentService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllTags()
        {
            var tags = await _tagService.GetAllTagsAsync();
            return Ok(tags);
        }

        [HttpGet("my-tags")]
        public async Task<IActionResult> GetMyTags()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized("Invalid user ID.");
            }

            var tags = await _tagService.GetTagsByUserIdAsync(userId);
            return Ok(tags);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetTag(Guid id)
        {
            var tag = await _tagService.GetTagByIdAsync(id);
            if (tag == null)
            {
                return NotFound();
            }

            return Ok(tag);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> CreateTag(TagCreateDto tagDto)
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized("Invalid user ID.");
            }

            var tag = await _tagService.CreateTagAsync(tagDto, userId);
            return CreatedAtAction(nameof(GetTag), new { id = tag.Id }, tag);
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTag(Guid id, TagUpdateDto tagDto)
        {
            var tag = await _tagService.UpdateTagAsync(id, tagDto);
            if (tag == null)
            {
                return NotFound();
            }

            return Ok(tag);
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTag(Guid id)
        {
            var result = await _tagService.DeleteTagAsync(id);
            if (!result)
            {
                return NotFound();
            }

            return NoContent();
        }

        [HttpGet("document/{documentId}")]
        public async Task<IActionResult> GetDocumentTags(Guid documentId)
        {
            var tags = await _tagService.GetTagsForDocumentAsync(documentId);
            return Ok(tags);
        }

        [HttpPost("document/{documentId}/tag/{tagId}")]
        public async Task<IActionResult> AddTagToDocument(Guid documentId, Guid tagId)
        {
            if (!await IsUserAuthorizedForDocumentTagOperation(documentId))
            {
                return Forbid("You do not have permission to modify tags for this document.");
            }

            var result = await _tagService.AddTagToDocumentAsync(documentId, tagId);
            if (!result)
            {
                return NotFound("Document or tag not found.");
            }

            return Ok();
        }

        [HttpDelete("document/{documentId}/tag/{tagId}")]
        public async Task<IActionResult> RemoveTagFromDocument(Guid documentId, Guid tagId)
        {
            if (!await IsUserAuthorizedForDocumentTagOperation(documentId))
            {
                return Forbid("You do not have permission to modify tags for this document.");
            }

            var result = await _tagService.RemoveTagFromDocumentAsync(documentId, tagId);
            if (!result)
            {
                return NotFound("Document or tag not found.");
            }

            return NoContent();
        }

        [HttpPut("document/{documentId}/tags")]
        public async Task<IActionResult> UpdateDocumentTags(Guid documentId, UpdateDocumentTagsDto request)
        {
            if (!await IsUserAuthorizedForDocumentTagOperation(documentId))
            {
                return Forbid("You do not have permission to modify tags for this document.");
            }

            try
            {
                var tags = await _tagService.UpdateDocumentTagsAsync(documentId, request.TagIds);
                return Ok(tags);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }

        // Helper method to check if user is authorized to modify document tags
        private async Task<bool> IsUserAuthorizedForDocumentTagOperation(Guid documentId)
        {
            // If user is an admin, always authorize
            if (User.IsInRole("Admin"))
            {
                return true;
            }

            // Otherwise check if user is document owner
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdClaim, out var userId))
            {
                return false;
            }

            var document = await _documentService.GetDocumentByIdAsync(documentId);
            return document?.CreatedById == userId;
        }
    }
}

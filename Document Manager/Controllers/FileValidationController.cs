using Document_Manager.DTOs;
using Document_Manager.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Document_Manager.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FileValidationController : ControllerBase
    {
        private readonly IFileValidationService _fileValidationService;

        public FileValidationController(IFileValidationService fileValidationService)
        {
            _fileValidationService = fileValidationService;
        }

        [HttpPost("validate")]
        [Consumes("multipart/form-data")]
        [SwaggerOperation("Validates a file for upload", "Validates if a file meets the system requirements")]
        [SwaggerResponse(StatusCodes.Status200OK, "File validation successful", typeof(FileValidationResultDto))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "File validation failed")]
        public async Task<IActionResult> ValidateFile([SwaggerParameter("File to validate")] IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("No file was uploaded.");
            }

            var result = await _fileValidationService.ValidateFileAsync(file);
            if (!result.IsValid)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        [HttpGet("allowed-types")]
        public async Task<IActionResult> GetAllowedTypes()
        {
            var allowedTypes = await _fileValidationService.GetAllowedFileTypesAsync();
            return Ok(allowedTypes);
        }

        [HttpGet("type/{extension}")]
        public async Task<IActionResult> GetFileTypeByExtension(string extension)
        {
            var fileType = await _fileValidationService.GetFileTypeByExtensionAsync(extension);
            if (fileType == null)
            {
                return NotFound($"File type with extension {extension} not found.");
            }
            
            return Ok(fileType);
        }

        [Authorize]
        [HttpPost("type")]
        public async Task<IActionResult> AddFileType([FromBody] FileTypeDto fileType)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var newFileType = await _fileValidationService.AddFileTypeAsync(fileType);
                return CreatedAtAction(nameof(GetFileTypeByExtension), new { extension = newFileType.FileExtension }, newFileType);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(ex.Message);
            }
        }

        [Authorize]
        [HttpPut("type/{id}")]
        public async Task<IActionResult> UpdateFileType(Guid id, [FromBody] FileTypeDto fileType)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var updatedFileType = await _fileValidationService.UpdateFileTypeAsync(id, fileType);
                return Ok(updatedFileType);
            }
            catch (KeyNotFoundException)
            {
                return NotFound($"File type with ID {id} not found.");
            }
        }

        [Authorize]
        [HttpDelete("type/{id}")]
        public async Task<IActionResult> RemoveFileType(Guid id)
        {
            var result = await _fileValidationService.RemoveFileTypeAsync(id);
            if (!result)
            {
                return NotFound($"File type with ID {id} not found.");
            }

            return NoContent();
        }
    }
}

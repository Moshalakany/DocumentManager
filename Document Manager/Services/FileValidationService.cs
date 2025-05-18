using Document_Manager.Data;
using Document_Manager.DTOs;
using Document_Manager.Models;
using Document_Manager.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;

namespace Document_Manager.Services
{
    public class FileValidationService : IFileValidationService
    {
        private readonly AppDbContextSQL _context;

        public FileValidationService(AppDbContextSQL context)
        {
            _context = context;
        }

        public async Task<FileValidationResultDto> ValidateFileAsync(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return new FileValidationResultDto
                {
                    IsValid = false,
                    Message = "File is empty or null."
                };
            }

            string fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
            
            // Sanitize the file extension
            fileExtension = SanitizeFileExtension(fileExtension);
            
            // Get validation record from database
            var validationType = await _context.FileValidations
                .FirstOrDefaultAsync(v => v.FileExtension == fileExtension);

            // If no specific validation found, return invalid
            if (validationType == null)
            {
                return new FileValidationResultDto
                {
                    IsValid = false,
                    Message = $"File type {fileExtension} is not supported.",
                    FileExtension = fileExtension,
                    ContentType = file.ContentType,
                    FileSize = file.Length
                };
            }

            // Check if file type is allowed
            if (!validationType.IsAllowed)
            {
                return new FileValidationResultDto
                {
                    IsValid = false,
                    Message = $"File type {fileExtension} is not allowed.",
                    FileExtension = fileExtension,
                    ContentType = file.ContentType,
                    FileSize = file.Length
                };
            }

            // Check file size
            if (file.Length > validationType.MaxSizeInBytes)
            {
                return new FileValidationResultDto
                {
                    IsValid = false,
                    Message = $"File size exceeds the maximum allowed size of {validationType.MaxSizeInBytes / (1024 * 1024)} MB.",
                    FileExtension = fileExtension,
                    ContentType = file.ContentType,
                    FileSize = file.Length
                };
            }

            // Validate content type
            if (!string.IsNullOrEmpty(validationType.ContentType) && 
                !file.ContentType.Equals(validationType.ContentType, StringComparison.OrdinalIgnoreCase))
            {
                // Check if content type matches with expected pattern (some files may have variations of content types)
                var isValidContentType = IsContentTypeCompatible(file.ContentType, validationType.ContentType);
                if (!isValidContentType)
                {
                    return new FileValidationResultDto
                    {
                        IsValid = false,
                        Message = $"Invalid content type. Expected {validationType.ContentType}, got {file.ContentType}.",
                        FileExtension = fileExtension,
                        ContentType = file.ContentType,
                        FileSize = file.Length
                    };
                }
            }

            // File is valid
            return new FileValidationResultDto
            {
                IsValid = true,
                Message = "File validation successful.",
                FileExtension = fileExtension,
                ContentType = file.ContentType,
                FileSize = file.Length,
                SupportsOcr = validationType.SupportsOcr,
                SupportsPreview = validationType.SupportsPreview
            };
        }

        public async Task<List<FileValidation>> GetAllowedFileTypesAsync()
        {
            return await _context.FileValidations
                .Where(v => v.IsAllowed)
                .ToListAsync();
        }

        public async Task<FileValidation> AddFileTypeAsync(FileTypeDto fileType)
        {
            var sanitizedExtension = SanitizeFileExtension(fileType.FileExtension);
            
            var existingType = await _context.FileValidations
                .FirstOrDefaultAsync(v => v.FileExtension == sanitizedExtension);

            if (existingType != null)
            {
                throw new InvalidOperationException($"File type {sanitizedExtension} already exists");
            }

            var newFileType = new FileValidation
            {
                Id = Guid.NewGuid(),
                FileExtension = sanitizedExtension,
                ContentType = fileType.ContentType,
                MaxSizeInBytes = fileType.MaxSizeInBytes,
                IsAllowed = fileType.IsAllowed,
                SupportsOcr = fileType.SupportsOcr,
                SupportsPreview = fileType.SupportsPreview
            };

            _context.FileValidations.Add(newFileType);
            await _context.SaveChangesAsync();

            return newFileType;
        }

        public async Task<FileValidation> UpdateFileTypeAsync(Guid id, FileTypeDto fileType)
        {
            var existingType = await _context.FileValidations.FindAsync(id);
            if (existingType == null)
            {
                throw new KeyNotFoundException($"File validation with ID {id} not found");
            }

            existingType.FileExtension = SanitizeFileExtension(fileType.FileExtension);
            existingType.ContentType = fileType.ContentType;
            existingType.MaxSizeInBytes = fileType.MaxSizeInBytes;
            existingType.IsAllowed = fileType.IsAllowed;
            existingType.SupportsOcr = fileType.SupportsOcr;
            existingType.SupportsPreview = fileType.SupportsPreview;

            _context.FileValidations.Update(existingType);
            await _context.SaveChangesAsync();

            return existingType;
        }

        public async Task<bool> RemoveFileTypeAsync(Guid id)
        {
            var existingType = await _context.FileValidations.FindAsync(id);
            if (existingType == null)
            {
                return false;
            }

            _context.FileValidations.Remove(existingType);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<FileValidation> GetFileTypeByExtensionAsync(string extension)
        {
            var sanitizedExtension = SanitizeFileExtension(extension);
            return await _context.FileValidations
                .FirstOrDefaultAsync(v => v.FileExtension == sanitizedExtension);
        }

        // Helper methods
        private string SanitizeFileExtension(string extension)
        {
            if (string.IsNullOrEmpty(extension))
            {
                return string.Empty;
            }
            
            // Make sure extension starts with a dot
            if (!extension.StartsWith("."))
            {
                extension = "." + extension;
            }
            
            // Convert to lowercase and trim
            extension = extension.ToLowerInvariant().Trim();
            
            // Remove any characters that are not alphanumeric or dot
            extension = Regex.Replace(extension, @"[^a-z0-9.]", "");
            
            return extension;
        }
        
        private bool IsContentTypeCompatible(string actualContentType, string expectedContentType)
        {
            if (string.IsNullOrEmpty(actualContentType) || string.IsNullOrEmpty(expectedContentType))
            {
                return false;
            }

            // Direct match
            if (actualContentType.Equals(expectedContentType, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            // Handle common subtypes (e.g., application/pdf, application/vnd.ms-excel)
            var actualParts = actualContentType.Split('/');
            var expectedParts = expectedContentType.Split('/');
            
            if (actualParts.Length >= 2 && expectedParts.Length >= 2)
            {
                // Match main type (e.g., application, image, video)
                if (!actualParts[0].Equals(expectedParts[0], StringComparison.OrdinalIgnoreCase))
                {
                    return false;
                }
                
                // Check if expected subtype is contained in the actual subtype
                // Handles cases like "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet" for Excel files
                return actualParts[1].Contains(expectedParts[1], StringComparison.OrdinalIgnoreCase) ||
                       expectedParts[1].Contains(actualParts[1], StringComparison.OrdinalIgnoreCase);
            }
            
            return false;
        }
    }
}

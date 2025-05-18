using Document_Manager.Data;
using Document_Manager.Models;
using Microsoft.EntityFrameworkCore;

namespace Document_Manager.Services
{
    public class FileTypeSeedService
    {
        private readonly IServiceProvider _serviceProvider;
        
        public FileTypeSeedService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }
        
        public async Task SeedFileTypesAsync()
        {
            using var scope = _serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContextSQL>();
            
            // Check if file types already exist
            if (await dbContext.FileValidations.AnyAsync())
            {
                return; // Already seeded
            }
            
            // Define common file types
            var fileTypes = new List<FileValidation>
            {
                new FileValidation
                {
                    Id = Guid.NewGuid(),
                    FileExtension = ".pdf",
                    ContentType = "application/pdf",
                    MaxSizeInBytes = 50 * 1024 * 1024, // 50MB
                    IsAllowed = true,
                    SupportsOcr = true,
                    SupportsPreview = true
                },
                new FileValidation
                {
                    Id = Guid.NewGuid(),
                    FileExtension = ".doc",
                    ContentType = "application/msword",
                    MaxSizeInBytes = 25 * 1024 * 1024, // 25MB
                    IsAllowed = true,
                    SupportsOcr = true,
                    SupportsPreview = true
                },
                new FileValidation
                {
                    Id = Guid.NewGuid(),
                    FileExtension = ".docx",
                    ContentType = "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                    MaxSizeInBytes = 25 * 1024 * 1024, // 25MB
                    IsAllowed = true,
                    SupportsOcr = true,
                    SupportsPreview = true
                },
                new FileValidation
                {
                    Id = Guid.NewGuid(),
                    FileExtension = ".xls",
                    ContentType = "application/vnd.ms-excel",
                    MaxSizeInBytes = 15 * 1024 * 1024, // 15MB
                    IsAllowed = true,
                    SupportsOcr = false,
                    SupportsPreview = true
                },
                new FileValidation
                {
                    Id = Guid.NewGuid(),
                    FileExtension = ".xlsx",
                    ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    MaxSizeInBytes = 15 * 1024 * 1024, // 15MB
                    IsAllowed = true,
                    SupportsOcr = false,
                    SupportsPreview = true
                },
                new FileValidation
                {
                    Id = Guid.NewGuid(),
                    FileExtension = ".ppt",
                    ContentType = "application/vnd.ms-powerpoint",
                    MaxSizeInBytes = 50 * 1024 * 1024, // 50MB
                    IsAllowed = true,
                    SupportsOcr = false,
                    SupportsPreview = true
                },
                new FileValidation
                {
                    Id = Guid.NewGuid(),
                    FileExtension = ".pptx",
                    ContentType = "application/vnd.openxmlformats-officedocument.presentationml.presentation",
                    MaxSizeInBytes = 50 * 1024 * 1024, // 50MB
                    IsAllowed = true,
                    SupportsOcr = false,
                    SupportsPreview = true
                },
                new FileValidation
                {
                    Id = Guid.NewGuid(),
                    FileExtension = ".txt",
                    ContentType = "text/plain",
                    MaxSizeInBytes = 10 * 1024 * 1024, // 10MB
                    IsAllowed = true,
                    SupportsOcr = false,
                    SupportsPreview = true
                },
                new FileValidation
                {
                    Id = Guid.NewGuid(),
                    FileExtension = ".jpg",
                    ContentType = "image/jpeg",
                    MaxSizeInBytes = 10 * 1024 * 1024, // 10MB
                    IsAllowed = true,
                    SupportsOcr = true,
                    SupportsPreview = true
                },
                new FileValidation
                {
                    Id = Guid.NewGuid(),
                    FileExtension = ".png",
                    ContentType = "image/png",
                    MaxSizeInBytes = 10 * 1024 * 1024, // 10MB
                    IsAllowed = true,
                    SupportsOcr = true,
                    SupportsPreview = true
                }
            };
            
            await dbContext.FileValidations.AddRangeAsync(fileTypes);
            await dbContext.SaveChangesAsync();
        }
    }
}

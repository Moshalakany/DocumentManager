using Document_Manager.Services.Interfaces;
using Microsoft.AspNetCore.Http;

namespace Document_Manager.Services
{
    public class FileStorageService(IConfiguration configuration) : IFileStorageService
    {
        private readonly string _baseStoragePath = configuration["FileStorage:Path"] ?? Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "FileStorage");
        private readonly string[] _allowedExtensions = (configuration["FileStorage:AllowedExtensions"] ?? ".pdf,.doc,.docx,.xls,.xlsx,.ppt,.pptx,.txt,.zip,.rar")
            .Split(',');
        private readonly long _maxFileSize = long.Parse(configuration["FileStorage:MaxFileSizeMB"] ?? "100") * 1024 * 1024; // Default 100MB

        public async Task<string> SaveFileAsync(IFormFile file, Guid documentId)
        {
            if (!Directory.Exists(_baseStoragePath))
            {
                Directory.CreateDirectory(_baseStoragePath);
            }

            var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
            var fileName = $"{documentId}{fileExtension}";
            var filePath = Path.Combine(_baseStoragePath, fileName);

            // Create subdirectories if needed for better organization
            var dirPath = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(dirPath) && !Directory.Exists(dirPath))
            {
                Directory.CreateDirectory(dirPath);
            }

            using var stream = new FileStream(filePath, FileMode.Create);
            await file.CopyToAsync(stream);

            return filePath;
        }

        public async Task<byte[]?> GetFileAsync(string filePath)
        {
            if (!File.Exists(filePath))
            {
                return null;
            }

            return await File.ReadAllBytesAsync(filePath);
        }

        public Task<bool> DeleteFileAsync(string filePath)
        {
            try
            {
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                    return Task.FromResult(true);
                }

                return Task.FromResult(false);
            }
            catch (Exception)
            {
                return Task.FromResult(false);
            }
        }

        public bool ValidateFileType(IFormFile file)
        {
            var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
            return _allowedExtensions.Contains(fileExtension);
        }

        public string GetFileExtension(IFormFile file)
        {
            return Path.GetExtension(file.FileName).ToLowerInvariant();
        }

        public string GetContentType(IFormFile file)
        {
            return file.ContentType;
        }

        public long GetFileSize(IFormFile file)
        {
            return file.Length;
        }
    }
}

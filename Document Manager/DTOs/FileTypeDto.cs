namespace Document_Manager.DTOs
{
    public class FileTypeDto
    {
        public string FileExtension { get; set; }
        public string ContentType { get; set; }
        public long MaxSizeInBytes { get; set; }
        public bool IsAllowed { get; set; }
        public bool SupportsOcr { get; set; }
        public bool SupportsPreview { get; set; }
    }
    
    public class FileValidationResultDto
    {
        public bool IsValid { get; set; }
        public string Message { get; set; }
        public string FileExtension { get; set; }
        public string ContentType { get; set; }
        public long FileSize { get; set; }
        public bool SupportsOcr { get; set; }
        public bool SupportsPreview { get; set; }
    }
}

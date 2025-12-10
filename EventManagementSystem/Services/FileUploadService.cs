using Microsoft.Extensions.Configuration;

namespace EventManagementSystem.Services
{
    public interface IFileUploadService
    {
        Task<(bool Success, string FilePath, string? ErrorMessage)> UploadFileAsync(IFormFile file, string destinationFolder = "events");
        Task<bool> DeleteFileAsync(string filePath);
    }

    public class FileUploadService : IFileUploadService
    {
        private readonly IConfiguration _configuration;
        private readonly ILoggingService _loggingService;
        private readonly IWebHostEnvironment _environment;

        public FileUploadService(IConfiguration configuration, ILoggingService loggingService, IWebHostEnvironment environment)
        {
            _configuration = configuration;
            _loggingService = loggingService;
            _environment = environment;
        }

        public async Task<(bool Success, string FilePath, string? ErrorMessage)> UploadFileAsync(IFormFile file, string destinationFolder = "events")
        {
            try
            {
                // Validate file
                var validationResult = ValidateFile(file);
                if (!validationResult.IsValid)
                {
                    return (false, "", validationResult.ErrorMessage);
                }

                // Get upload directory
                var uploadDirectory = Path.Combine(_environment.WebRootPath, "uploads", destinationFolder);
                
                if (!Directory.Exists(uploadDirectory))
                {
                    Directory.CreateDirectory(uploadDirectory);
                }

                // Generate unique filename
                var fileName = $"{Guid.NewGuid()}_{Path.GetFileName(file.FileName)}";
                var filePath = Path.Combine(uploadDirectory, fileName);

                // Save file
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                var relativePath = $"/uploads/{destinationFolder}/{fileName}";
                await _loggingService.LogInfoAsync($"File uploaded successfully: {relativePath}");

                return (true, relativePath, null);
            }
            catch (Exception ex)
            {
                await _loggingService.LogErrorAsync($"File upload failed", ex);
                return (false, "", "File upload failed. Please try again.");
            }
        }

        public async Task<bool> DeleteFileAsync(string filePath)
        {
            try
            {
                if (string.IsNullOrEmpty(filePath))
                    return true;

                // Remove leading slash if present
                var cleanPath = filePath.StartsWith("/") ? filePath.Substring(1) : filePath;
                var fullPath = Path.Combine(_environment.WebRootPath, cleanPath);

                if (File.Exists(fullPath))
                {
                    File.Delete(fullPath);
                    await _loggingService.LogInfoAsync($"File deleted: {filePath}");
                    return true;
                }

                return true;
            }
            catch (Exception ex)
            {
                await _loggingService.LogErrorAsync($"File deletion failed: {filePath}", ex);
                return false;
            }
        }

        private FileValidationResult ValidateFile(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return new FileValidationResult { IsValid = false, ErrorMessage = "No file selected." };
            }

            var maxFileSize = _configuration.GetValue<long>("FileUpload:MaxFileSize", 5242880); // 5MB default
            if (file.Length > maxFileSize)
            {
                return new FileValidationResult { IsValid = false, ErrorMessage = $"File size exceeds maximum allowed size of {maxFileSize / 1024 / 1024}MB." };
            }

            var allowedExtensions = _configuration.GetSection("FileUpload:AllowedExtensions").Get<string[]>() ?? 
                                   new[] { ".jpg", ".jpeg", ".png", ".gif" };
            
            var fileExtension = Path.GetExtension(file.FileName).ToLower();
            if (!allowedExtensions.Contains(fileExtension))
            {
                return new FileValidationResult { IsValid = false, ErrorMessage = $"File type {fileExtension} is not allowed. Allowed types: {string.Join(", ", allowedExtensions)}" };
            }

            return new FileValidationResult { IsValid = true };
        }

        private class FileValidationResult
        {
            public bool IsValid { get; set; }
            public string? ErrorMessage { get; set; }
        }
    }
}

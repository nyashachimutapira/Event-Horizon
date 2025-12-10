using System;
using System.IO;
using System.Text;

namespace EventManagementSystem.Services
{
    public interface ILoggingService
    {
        Task LogAsync(string level, string message, Exception? ex = null);
        Task LogErrorAsync(string message, Exception ex);
        Task LogInfoAsync(string message);
    }

    public class LoggingService : ILoggingService
    {
        private readonly string _logDirectory;
        private readonly string _logFilePath;

        public LoggingService(IHostEnvironment env)
        {
            _logDirectory = Path.Combine(env.ContentRootPath, "Logs");
            
            // Create logs directory if it doesn't exist
            if (!Directory.Exists(_logDirectory))
            {
                Directory.CreateDirectory(_logDirectory);
            }

            // Log file path with date
            var logFileName = $"app_{DateTime.Now:yyyy-MM-dd}.log";
            _logFilePath = Path.Combine(_logDirectory, logFileName);
        }

        public async Task LogAsync(string level, string message, Exception? ex = null)
        {
            var logMessage = new StringBuilder();
            logMessage.AppendLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [{level}] {message}");
            
            if (ex != null)
            {
                logMessage.AppendLine($"Exception: {ex.Message}");
                logMessage.AppendLine($"StackTrace: {ex.StackTrace}");
            }

            try
            {
                await File.AppendAllTextAsync(_logFilePath, logMessage.ToString());
            }
            catch
            {
                // Silently fail if logging fails to prevent cascading errors
            }
        }

        public async Task LogErrorAsync(string message, Exception ex)
        {
            await LogAsync("ERROR", message, ex);
        }

        public async Task LogInfoAsync(string message)
        {
            await LogAsync("INFO", message);
        }
    }
}

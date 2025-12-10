using Xunit;
using System.Threading.Tasks;
using Moq;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using EventManagementSystem.Services;

namespace EventManagementSystem.Tests.Services
{
    public class FileUploadServiceTests
    {
        private readonly IConfiguration _configuration;
        private readonly Mock<ILoggingService> _mockLoggingService;
        private readonly Mock<IWebHostEnvironment> _mockEnvironment;
        private readonly IFileUploadService _fileUploadService;

        public FileUploadServiceTests()
        {
            // Setup in-memory configuration
            var myConfiguration = new Dictionary<string, string?>
            {
                {"FileUpload:MaxFileSize", "5242880"},
                {"FileUpload:AllowedExtensions:0", ".jpg"},
                {"FileUpload:AllowedExtensions:1", ".jpeg"},
                {"FileUpload:AllowedExtensions:2", ".png"},
                {"FileUpload:AllowedExtensions:3", ".gif"}
            };

            _configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(myConfiguration)
                .Build();

            _mockLoggingService = new Mock<ILoggingService>();
            _mockEnvironment = new Mock<IWebHostEnvironment>();

            _mockEnvironment
                .Setup(x => x.WebRootPath)
                .Returns(Path.Combine(Path.GetTempPath(), "wwwroot"));

            _fileUploadService = new FileUploadService(_configuration, _mockLoggingService.Object, _mockEnvironment.Object);
        }

        [Fact]
        public async Task UploadFileAsync_With_Valid_File_Should_Succeed()
        {
            // Arrange
            var fileName = "test-image.jpg";
            var fileContent = "fake image content";
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write(fileContent);
            writer.Flush();
            stream.Position = 0;

            var fileMock = new Mock<IFormFile>();
            fileMock.Setup(x => x.FileName).Returns(fileName);
            fileMock.Setup(x => x.Length).Returns(fileContent.Length);
            fileMock.Setup(x => x.OpenReadStream()).Returns(stream);
            fileMock.Setup(x => x.CopyToAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
                .Returns((Stream destination, CancellationToken ct) =>
                {
                    stream.CopyTo(destination);
                    return Task.CompletedTask;
                });

            // Act
            var result = await _fileUploadService.UploadFileAsync(fileMock.Object, "events");

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.FilePath);
            Assert.Contains("test-image.jpg", result.FilePath);
        }

        [Fact]
        public async Task UploadFileAsync_With_Null_File_Should_Fail()
        {
            // Act & Assert
            var result = await _fileUploadService.UploadFileAsync(null!, "events");

            // Assert
            Assert.False(result.Success);
            Assert.NotNull(result.ErrorMessage);
            Assert.Contains("No file selected", result.ErrorMessage);
        }

        [Fact]
        public async Task UploadFileAsync_With_Disallowed_Extension_Should_Fail()
        {
            // Arrange
            var fileName = "test-file.exe";
            var stream = new MemoryStream(new byte[] { 1, 2, 3 });

            var fileMock = new Mock<IFormFile>();
            fileMock.Setup(x => x.FileName).Returns(fileName);
            fileMock.Setup(x => x.Length).Returns(100);

            // Act
            var result = await _fileUploadService.UploadFileAsync(fileMock.Object, "events");

            // Assert
            Assert.False(result.Success);
            Assert.NotNull(result.ErrorMessage);
            Assert.Contains("not allowed", result.ErrorMessage);
        }

        [Fact]
        public async Task DeleteFileAsync_With_Valid_Path_Should_Return_True()
        {
            // Arrange
            string filePath = "/uploads/events/test.jpg";

            // Act
            var result = await _fileUploadService.DeleteFileAsync(filePath);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task DeleteFileAsync_With_Null_Path_Should_Return_True()
        {
            // Act
            var result = await _fileUploadService.DeleteFileAsync(null!);

            // Assert
            Assert.True(result);
        }
    }
}

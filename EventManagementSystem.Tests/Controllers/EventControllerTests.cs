using Xunit;
using System;
using System.Threading.Tasks;
using Moq;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using EventManagementSystem.Controllers;
using EventManagementSystem.Models;
using EventManagementSystem.Services;

namespace EventManagementSystem.Tests.Controllers
{
    public class EventControllerTests
    {
        private readonly ApplicationDbContext _context;
        private readonly Mock<IEmailService> _mockEmailService;
        private readonly Mock<IFileUploadService> _mockFileUploadService;
        private readonly Mock<ILoggingService> _mockLoggingService;
        private readonly Mock<IWaitingListService> _mockWaitingListService;
        private readonly EventController _controller;

        public EventControllerTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new ApplicationDbContext(options);
            _mockEmailService = new Mock<IEmailService>();
            _mockFileUploadService = new Mock<IFileUploadService>();
            _mockLoggingService = new Mock<ILoggingService>();
            _mockWaitingListService = new Mock<IWaitingListService>();

            _controller = new EventController(
                _context,
                _mockEmailService.Object,
                _mockFileUploadService.Object,
                _mockLoggingService.Object,
                _mockWaitingListService.Object
            );
        }

        [Fact]
        public async Task Index_Should_Return_ViewResult()
        {
            // Arrange
            _context.Events.Add(new Event { Id = 1, Title = "Event 1", Status = "Active", Category = "Tech", Description = "Description 1", Location = "Room 1", StartDate = DateTime.Now.AddDays(1), EndDate = DateTime.Now.AddDays(1).AddHours(2), MaxAttendees = 100 });
            _context.Events.Add(new Event { Id = 2, Title = "Event 2", Status = "Active", Category = "Social", Description = "Description 2", Location = "Room 2", StartDate = DateTime.Now.AddDays(2), EndDate = DateTime.Now.AddDays(2).AddHours(2), MaxAttendees = 50 });
            await _context.SaveChangesAsync();

            // Act
            var result = await _controller.Index(null, null, null, null, null, 1);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.NotNull(viewResult.Model);
        }

        [Fact]
        public async Task Details_With_Null_Id_Should_Return_NotFound()
        {
            // Act
            var result = await _controller.Details(null);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Details_With_Invalid_Id_Should_Return_NotFound()
        {
            // Act
            var result = await _controller.Details(999);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public void Create_Get_Should_Return_View()
        {
            // Setup session with UserId
            var mockHttpContext = new Mock<Microsoft.AspNetCore.Http.HttpContext>();
            var mockSession = new Mock<Microsoft.AspNetCore.Http.ISession>();
            
            byte[]? userIdBytes = System.Text.Encoding.UTF8.GetBytes("test-user-id");
            mockSession
                .Setup(x => x.TryGetValue("UserId", out userIdBytes))
                .Returns(true);

            mockHttpContext.Setup(h => h.Session).Returns(mockSession.Object);
            _controller.ControllerContext.HttpContext = mockHttpContext.Object;

            // Act
            var result = _controller.Create();

            // Assert
            Assert.NotNull(result);
        }

        [Fact]
        public async Task Rsvp_Without_Login_Should_Redirect_To_Login()
        {
            // Setup session without UserId
            var mockHttpContext = new Mock<Microsoft.AspNetCore.Http.HttpContext>();
            var mockSession = new Mock<Microsoft.AspNetCore.Http.ISession>();
            
            byte[]? userIdBytes = null;
            mockSession
                .Setup(x => x.TryGetValue("UserId", out userIdBytes))
                .Returns(false);

            mockHttpContext.Setup(h => h.Session).Returns(mockSession.Object);
            _controller.ControllerContext.HttpContext = mockHttpContext.Object;

            // Act
            var result = await _controller.Rsvp(1);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Login", redirectResult.ActionName);
            Assert.Equal("Account", redirectResult.ControllerName);
        }
    }
}

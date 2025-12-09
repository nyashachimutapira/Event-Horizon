using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EventManagementSystem.Models;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;

namespace EventManagementSystem.ViewModels
{
    public class EventAnalyticsViewModel
    {
        public int EventId { get; set; }
        public string Title { get; set; }
        public string Category { get; set; }
        public DateTime StartDate { get; set; }
        public string Organizer { get; set; }
        public int TotalAttendees { get; set; }
        public int MaxAttendees { get; set; }
        public int TotalRsvps { get; set; }
        public int AttendanceRate { get; set; }
        public int EngagementScore { get; set; }
        public double AverageRating { get; set; }
        public int FeedbackCount { get; set; }
    }
}

namespace EventManagementSystem.Controllers
{
    using EventManagementSystem.ViewModels;
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Check if user is admin
        private bool IsAdmin()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return false;
            
            var user = _context.Users.FirstOrDefault(u => u.Id == userId);
            return user != null && user.IsAdmin;
        }

        private IActionResult AdminOnly()
        {
            if (!IsAdmin())
                return Unauthorized();
            return null;
        }

        // GET: Admin/Dashboard
        public async Task<IActionResult> Dashboard()
        {
            var authCheck = AdminOnly();
            if (authCheck != null) return authCheck;

            var stats = new AdminDashboardViewModel
            {
                TotalUsers = await _context.Users.CountAsync(),
                TotalEvents = await _context.Events.CountAsync(),
                TotalAttendances = await _context.Rsvps.CountAsync(),
                TotalFeedback = await _context.Feedbacks.CountAsync(),
                EventsThisMonth = await _context.Events
                    .Where(e => e.CreatedAt.Month == DateTime.Now.Month && e.CreatedAt.Year == DateTime.Now.Year)
                    .CountAsync(),
                PendingModerations = await _context.Feedbacks
                    .Where(f => !f.IsApproved)
                    .CountAsync(),
                RecentEvents = await _context.Events
                    .Include(e => e.CreatedBy)
                    .OrderByDescending(e => e.CreatedAt)
                    .Take(5)
                    .ToListAsync(),
                TopEvents = await _context.Events
                    .Include(e => e.Rsvps)
                    .OrderByDescending(e => e.Rsvps.Count)
                    .Take(5)
                    .ToListAsync()
            };

            return View(stats);
        }

        // GET: Admin/EventAnalytics
        public async Task<IActionResult> EventAnalytics()
        {
            var authCheck = AdminOnly();
            if (authCheck != null) return authCheck;

            var events = await _context.Events
                .Include(e => e.CreatedBy)
                .Include(e => e.Rsvps)
                .Include(e => e.Feedbacks)
                .OrderByDescending(e => e.CreatedAt)
                .ToListAsync();

            var analytics = events.Select(e => new EventAnalyticsViewModel
            {
                EventId = e.Id,
                Title = e.Title,
                Category = e.Category,
                StartDate = e.StartDate,
                Organizer = e.CreatedBy?.Username,
                TotalAttendees = e.Rsvps?.Count(r => r.Status == "Attending") ?? 0,
                MaxAttendees = e.MaxAttendees,
                TotalRsvps = e.Rsvps?.Count ?? 0,
                AttendanceRate = e.MaxAttendees > 0 ? ((e.Rsvps?.Count(r => r.Status == "Attending") ?? 0) * 100) / e.MaxAttendees : 0,
                EngagementScore = CalculateEngagementScore(e),
                AverageRating = e.Feedbacks?.Count > 0 ? e.Feedbacks.Average(f => f.Rating) : 0,
                FeedbackCount = e.Feedbacks?.Count ?? 0
            }).ToList();

            return View(analytics);
        }

        // GET: Admin/Users
        public async Task<IActionResult> Users(string search = "")
        {
            var authCheck = AdminOnly();
            if (authCheck != null) return authCheck;

            var users = await _context.Users
                .Include(u => u.CreatedEvents)
                .Include(u => u.Rsvps)
                .Include(u => u.Feedbacks)
                .Where(u => string.IsNullOrEmpty(search) || 
                    u.Username.Contains(search) || 
                    u.Email.Contains(search) ||
                    u.FirstName.Contains(search) ||
                    u.LastName.Contains(search))
                .ToListAsync();

            ViewBag.Search = search;
            return View(users);
        }

        // GET: Admin/UserDetails
        public async Task<IActionResult> UserDetails(int? id)
        {
            var authCheck = AdminOnly();
            if (authCheck != null) return authCheck;

            if (id == null)
                return NotFound();

            var user = await _context.Users
                .Include(u => u.CreatedEvents)
                .Include(u => u.Rsvps).ThenInclude(r => r.Event)
                .Include(u => u.Feedbacks).ThenInclude(f => f.Event)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null)
                return NotFound();

            return View(user);
        }

        // POST: Admin/ToggleUserStatus
        [HttpPost]
        public async Task<IActionResult> ToggleUserStatus(int id)
        {
            var authCheck = AdminOnly();
            if (authCheck != null) return authCheck;

            var user = await _context.Users.FindAsync(id);
            if (user == null)
                return NotFound();

            user.IsActive = !user.IsActive;
            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(UserDetails), new { id });
        }

        // GET: Admin/Moderation
        public async Task<IActionResult> Moderation()
        {
            var authCheck = AdminOnly();
            if (authCheck != null) return authCheck;

            var pendingFeedback = await _context.Feedbacks
                .Include(f => f.User)
                .Include(f => f.Event)
                .Where(f => !f.IsApproved)
                .OrderByDescending(f => f.CreatedAt)
                .ToListAsync();

            return View(pendingFeedback);
        }

        // POST: Admin/ApproveFeedback
        [HttpPost]
        public async Task<IActionResult> ApproveFeedback(int id)
        {
            var authCheck = AdminOnly();
            if (authCheck != null) return authCheck;

            var feedback = await _context.Feedbacks.FindAsync(id);
            if (feedback == null)
                return NotFound();

            feedback.IsApproved = true;
            _context.Feedbacks.Update(feedback);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Moderation));
        }

        // POST: Admin/RejectFeedback
        [HttpPost]
        public async Task<IActionResult> RejectFeedback(int id)
        {
            var authCheck = AdminOnly();
            if (authCheck != null) return authCheck;

            var feedback = await _context.Feedbacks.FindAsync(id);
            if (feedback == null)
                return NotFound();

            _context.Feedbacks.Remove(feedback);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Moderation));
        }

        private int CalculateEngagementScore(Event @event)
        {
            // Score based on RSVPs + Feedback
            var rsvpScore = (@event.Rsvps?.Count ?? 0) * 10;
            var feedbackScore = (@event.Feedbacks?.Count ?? 0) * 15;
            return Math.Min(100, rsvpScore + feedbackScore);
        }
    }
}

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EventManagementSystem.Models;
using EventManagementSystem.ViewModels;

namespace EventManagementSystem.Controllers
{
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
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return false;

            var user = _context.Users.FirstOrDefault(u => u.Id == userId);
            return user?.IsAdmin == true;
        }

        // GET: Admin/Dashboard
        public async Task<IActionResult> Dashboard()
        {
            if (!IsAdmin())
                return Forbid();

            // Get statistics
            var totalUsers = await _context.Users.CountAsync();
            var totalEvents = await _context.Events.CountAsync(e => !e.IsArchived);
            var totalRsvps = await _context.Rsvps.CountAsync();
            var totalReviews = await _context.Feedbacks.CountAsync();

            ViewBag.TotalUsers = totalUsers;
            ViewBag.TotalEvents = totalEvents;
            ViewBag.TotalRsvps = totalRsvps;
            ViewBag.TotalReviews = totalReviews;

            // Recent events
            var recentEvents = await _context.Events
                .OrderByDescending(e => e.CreatedAt)
                .Take(5)
                .Include(e => e.CreatedBy)
                .ToListAsync();

            // Recent users
            var recentUsers = await _context.Users
                .OrderByDescending(u => u.CreatedAt)
                .Take(5)
                .ToListAsync();

            ViewBag.RecentEvents = recentEvents;
            ViewBag.RecentUsers = recentUsers;

            return View();
        }

        // GET: Admin/Users
        public async Task<IActionResult> Users(int page = 1)
        {
            if (!IsAdmin())
                return Forbid();

            const int pageSize = 20;

            var users = await _context.Users
                .OrderByDescending(u => u.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var totalCount = await _context.Users.CountAsync();

            ViewBag.Page = page;
            ViewBag.TotalPages = (totalCount + pageSize - 1) / pageSize;
            ViewBag.TotalCount = totalCount;

            return View(users);
        }

        // GET: Admin/Events
        public async Task<IActionResult> Events(int page = 1)
        {
            if (!IsAdmin())
                return Forbid();

            const int pageSize = 20;

            var events = await _context.Events
                .Include(e => e.CreatedBy)
                .Include(e => e.Rsvps)
                .OrderByDescending(e => e.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var totalCount = await _context.Events.CountAsync();

            ViewBag.Page = page;
            ViewBag.TotalPages = (totalCount + pageSize - 1) / pageSize;
            ViewBag.TotalCount = totalCount;

            return View(events);
        }

        // GET: Admin/ArchivedEvents
        public async Task<IActionResult> ArchivedEvents()
        {
            if (!IsAdmin())
                return Forbid();

            var archivedEvents = await _context.Events
                .Where(e => e.IsArchived)
                .Include(e => e.CreatedBy)
                .OrderByDescending(e => e.ArchivedAt)
                .ToListAsync();

            return View(archivedEvents);
        }

        // POST: Admin/RestoreEvent
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RestoreEvent(int eventId)
        {
            if (!IsAdmin())
                return Forbid();

            var @event = await _context.Events.FindAsync(eventId);
            if (@event == null)
                return NotFound();

            @event.IsArchived = false;
            @event.ArchivedAt = null;

            _context.Update(@event);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Event restored successfully";
            return RedirectToAction("ArchivedEvents");
        }

        // POST: Admin/DeactivateUser
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeactivateUser(int userId)
        {
            if (!IsAdmin())
                return Forbid();

            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                return NotFound();

            user.IsActive = false;
            _context.Update(user);
            await _context.SaveChangesAsync();

            TempData["Success"] = "User deactivated";
            return RedirectToAction("Users");
        }

        // POST: Admin/ActivateUser
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ActivateUser(int userId)
        {
            if (!IsAdmin())
                return Forbid();

            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                return NotFound();

            user.IsActive = true;
            _context.Update(user);
            await _context.SaveChangesAsync();

            TempData["Success"] = "User activated";
            return RedirectToAction("Users");
        }

        // POST: Admin/MakeAdmin
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MakeAdmin(int userId)
        {
            if (!IsAdmin())
                return Forbid();

            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                return NotFound();

            user.IsAdmin = true;
            _context.Update(user);
            await _context.SaveChangesAsync();

            TempData["Success"] = "User promoted to admin";
            return RedirectToAction("Users");
        }

        // POST: Admin/RemoveAdmin
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveAdmin(int userId)
        {
            if (!IsAdmin())
                return Forbid();

            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                return NotFound();

            user.IsAdmin = false;
            _context.Update(user);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Admin role removed";
            return RedirectToAction("Users");
        }

        // GET: Admin/Analytics
        public async Task<IActionResult> Analytics()
        {
            if (!IsAdmin())
                return Forbid();

            // Event statistics
            var totalEvents = await _context.Events.CountAsync(e => !e.IsArchived);
            var activeEvents = await _context.Events.CountAsync(e => e.Status == "Active" && !e.IsArchived);
            var cancelledEvents = await _context.Events.CountAsync(e => e.Status == "Cancelled");
            var archivedEvents = await _context.Events.CountAsync(e => e.IsArchived);

            ViewBag.TotalEvents = totalEvents;
            ViewBag.ActiveEvents = activeEvents;
            ViewBag.CancelledEvents = cancelledEvents;
            ViewBag.ArchivedEvents = archivedEvents;

            // User statistics
            var totalUsers = await _context.Users.CountAsync();
            var activeUsers = await _context.Users.CountAsync(u => u.IsActive);
            var adminUsers = await _context.Users.CountAsync(u => u.IsAdmin);

            ViewBag.TotalUsers = totalUsers;
            ViewBag.ActiveUsers = activeUsers;
            ViewBag.AdminUsers = adminUsers;

            // Category statistics
            var eventsByCategory = await _context.Events
                .Where(e => !e.IsArchived)
                .GroupBy(e => e.Category)
                .Select(g => new { Category = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .ToListAsync();

            ViewBag.EventsByCategory = eventsByCategory;

            // Top events by attendance
            var topEvents = await _context.Events
                .Include(e => e.Rsvps)
                .Where(e => !e.IsArchived)
                .OrderByDescending(e => e.Rsvps!.Count)
                .Take(10)
                .Select(e => new { e.Title, Count = e.Rsvps!.Count })
                .ToListAsync();

            ViewBag.TopEvents = topEvents;

            return View();
        }
    }
}

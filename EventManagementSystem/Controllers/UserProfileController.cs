using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EventManagementSystem.Models;
using System.Threading.Tasks;
using System.Linq;

namespace EventManagementSystem.Controllers
{
    public class UserProfileController : Controller
    {
        private readonly ApplicationDbContext _context;

        public UserProfileController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: UserProfile/Index or UserProfile/ViewProfile/{username}
        public async Task<IActionResult> ViewProfile(string username)
        {
            var user = await _context.Users
                .Include(u => u.CreatedEvents).ThenInclude(e => e.Rsvps)
                .Include(u => u.Rsvps).ThenInclude(r => r.Event)
                .Include(u => u.Feedbacks).ThenInclude(f => f.Event)
                .FirstOrDefaultAsync(u => u.Username == username);

            if (user == null)
                return NotFound();

            return View(user);
        }

        // GET: UserProfile/MyProfile
        public async Task<IActionResult> MyProfile()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return RedirectToAction("Login", "Account");

            var user = await _context.Users
                .Include(u => u.CreatedEvents).ThenInclude(e => e.Rsvps)
                .Include(u => u.Rsvps).ThenInclude(r => r.Event)
                .Include(u => u.Feedbacks).ThenInclude(f => f.Event)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
                return NotFound();

            return View("ViewProfile", user);
        }

        // GET: UserProfile/Edit
        public async Task<IActionResult> Edit()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return RedirectToAction("Login", "Account");

            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                return NotFound();

            return View(user);
        }

        // POST: UserProfile/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, User userUpdate)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null || userId != id)
                return Unauthorized();

            var user = await _context.Users.FindAsync(id);
            if (user == null)
                return NotFound();

            user.FirstName = userUpdate.FirstName;
            user.LastName = userUpdate.LastName;
            user.Bio = userUpdate.Bio;
            user.Location = userUpdate.Location;
            user.AvatarUrl = userUpdate.AvatarUrl;
            user.NotificationsEnabled = userUpdate.NotificationsEnabled;
            user.UpdatedAt = DateTime.UtcNow;

            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(MyProfile));
        }

        // GET: UserProfile/Notifications
        public async Task<IActionResult> Notifications()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return RedirectToAction("Login", "Account");

            var notifications = await _context.Notifications
                .Where(n => n.UserId == userId)
                .Include(n => n.Event)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();

            return View(notifications);
        }

        // POST: UserProfile/MarkNotificationAsRead
        [HttpPost]
        public async Task<IActionResult> MarkNotificationAsRead(int id)
        {
            var notification = await _context.Notifications.FindAsync(id);
            if (notification == null)
                return NotFound();

            notification.IsRead = true;
            _context.Notifications.Update(notification);
            await _context.SaveChangesAsync();

            return Ok();
        }

        // POST: UserProfile/ClearAllNotifications
        [HttpPost]
        public async Task<IActionResult> ClearAllNotifications()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return Unauthorized();

            var notifications = await _context.Notifications
                .Where(n => n.UserId == userId)
                .ToListAsync();

            foreach (var notification in notifications)
            {
                notification.IsRead = true;
            }

            _context.Notifications.UpdateRange(notifications);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Notifications));
        }
    }
}

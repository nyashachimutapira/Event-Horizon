using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EventManagementSystem.Models;
using EventManagementSystem.ViewModels;
using EventManagementSystem.Services;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Filters;

namespace EventManagementSystem.Controllers
{
    public class EventController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IEmailService _emailService;
        private readonly IFileUploadService _fileUploadService;
        private readonly ILoggingService _loggingService;
        private readonly IWaitingListService _waitingListService;

        public EventController(ApplicationDbContext context, IEmailService emailService, IFileUploadService fileUploadService, ILoggingService loggingService, IWaitingListService waitingListService)
        {
            _context = context;
            _emailService = emailService;
            _fileUploadService = fileUploadService;
            _loggingService = loggingService;
            _waitingListService = waitingListService;
        }

        // GET: Event
        public async Task<IActionResult> Index(string? searchTerm, string? category, string? location, DateTime? startDate, DateTime? endDate, int page = 1)
        {
            const int pageSize = 10;

            // Build query
            var query = _context.Events
                .Where(e => e.Status == "Active" && !e.IsArchived)
                .Include(e => e.CreatedBy)
                .Include(e => e.Rsvps)
                .AsQueryable();

            // Apply filters
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                query = query.Where(e => e.Title!.Contains(searchTerm) || e.Description!.Contains(searchTerm));
            }

            if (!string.IsNullOrWhiteSpace(category))
            {
                query = query.Where(e => e.Category == category);
            }

            if (!string.IsNullOrWhiteSpace(location))
            {
                query = query.Where(e => e.Location!.Contains(location));
            }

            if (startDate.HasValue)
            {
                query = query.Where(e => e.StartDate >= startDate);
            }

            if (endDate.HasValue)
            {
                query = query.Where(e => e.EndDate <= endDate.Value.AddDays(1));
            }

            // Get total count
            var totalCount = await query.CountAsync();

            // Apply pagination
            var events = await query
                .OrderBy(e => e.StartDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // Create view model
            var viewModel = new EventSearchViewModel
            {
                SearchTerm = searchTerm,
                Category = category,
                Location = location,
                StartDate = startDate,
                EndDate = endDate,
                PageNumber = page,
                PageSize = pageSize,
                TotalCount = totalCount,
                Events = events
            };

            return View(viewModel);
        }

        // GET: Event/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var evt = await _context.Events
                .Include(e => e.CreatedBy)
                .Include(e => e.Rsvps)
                .Include(e => e.Feedbacks)
                .FirstOrDefaultAsync(m => m.Id == id);
            
            if (evt == null)
            {
                return NotFound();
            }

            return View(evt);
        }

        // GET: Event/Create
        public IActionResult Create()
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }
            return View();
        }

        // POST: Event/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Title,Description,StartDate,EndDate,Location,MaxAttendees,Category")] Event evt, IFormFile? imageFile)
        {
            // Set CreatedById from session
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }
            evt.CreatedById = userId.Value;

            if (ModelState.IsValid)
            {
                // Handle image upload
                if (imageFile != null)
                {
                    var uploadResult = await _fileUploadService.UploadFileAsync(imageFile, "events");
                    if (uploadResult.Success)
                    {
                        evt.ImageUrl = uploadResult.FilePath;
                    }
                    else
                    {
                        ModelState.AddModelError("imageFile", uploadResult.ErrorMessage ?? "Image upload failed");
                        return View(evt);
                    }
                }

                _context.Add(evt);
                await _context.SaveChangesAsync();

                await _loggingService.LogInfoAsync($"Event created: {evt.Title} by user {userId}");

                return RedirectToAction(nameof(Index));
            }
            return View(evt);
        }

        // GET: Event/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            if (id == null)
            {
                return NotFound();
            }

            var evt = await _context.Events.FindAsync(id);
            if (evt == null)
            {
                return NotFound();
            }

            // Check if user is the event creator or admin
            var user = await _context.Users.FindAsync(userId);
            if (evt.CreatedById != userId && !user!.IsAdmin)
            {
                return Forbid("You don't have permission to edit this event.");
            }

            return View(evt);
        }

        // POST: Event/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Description,StartDate,EndDate,Location,MaxAttendees,Category,ImageUrl,CreatedById")] Event evt)
        {
            if (id != evt.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(evt);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(evt);
        }

        // GET: Event/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            if (id == null)
            {
                return NotFound();
            }

            var evt = await _context.Events
                .FirstOrDefaultAsync(m => m.Id == id);
            if (evt == null)
            {
                return NotFound();
            }

            // Check if user is the event creator or admin
            var user = await _context.Users.FindAsync(userId);
            if (evt.CreatedById != userId && !user!.IsAdmin)
            {
                return Forbid("You don't have permission to delete this event.");
            }

            return View(evt);
        }

        // POST: Event/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var evt = await _context.Events.FindAsync(id);
            if (evt != null)
            {
                // Check if user is the event creator or admin
                var user = await _context.Users.FindAsync(userId);
                if (evt.CreatedById != userId && !user!.IsAdmin)
                {
                    return Forbid("You don't have permission to delete this event.");
                }

                // Soft delete - archive the event instead of permanently removing it
                evt.IsArchived = true;
                evt.ArchivedAt = DateTime.UtcNow;

                _context.Update(evt);
                await _context.SaveChangesAsync();

                // Notify attendees
                var attendees = await _context.Rsvps
                    .Where(r => r.EventId == id && r.Status == "Attending")
                    .Include(r => r.User)
                    .ToListAsync();

                foreach (var attendee in attendees)
                {
                    if (attendee.User != null)
                    {
                        // Create notification
                        var notification = new Notification
                        {
                            UserId = attendee.UserId,
                            EventId = id,
                            Type = "EVENT_CANCELLED",
                            Title = "Event Archived",
                            Message = $"The event '{evt.Title}' has been archived.",
                            IsRead = false
                        };
                        _context.Add(notification);

                        // Send email
                        await _emailService.SendEmailAsync(
                            attendee.User.Email!,
                            "Event Archived",
                            $"<p>The event <strong>{evt.Title}</strong> has been archived.</p>"
                        );
                    }
                }

                await _context.SaveChangesAsync();

                await _loggingService.LogInfoAsync($"Event {id} archived by user {userId}");
            }
            return RedirectToAction(nameof(Index));
        }

        // POST: Event/Rsvp
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Rsvp(int eventId, string status = "Attending", int guestCount = 1)
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var evt = await _context.Events
                .Include(e => e.Rsvps)
                .FirstOrDefaultAsync(e => e.Id == eventId);

            if (evt == null)
            {
                return NotFound();
            }

            // Check if user already has an RSVP
            var existingRsvp = evt.Rsvps?.FirstOrDefault(r => r.UserId == userId);

            if (existingRsvp != null)
            {
                existingRsvp.Status = status;
                existingRsvp.GuestCount = guestCount;
                _context.Update(existingRsvp);
            }
            else
            {
                // Check if event is full
                var currentAttendees = evt.Rsvps?.Count(r => r.Status == "Attending") ?? 0;
                if (currentAttendees >= evt.MaxAttendees)
                {
                    // Add to waiting list instead
                    var addedToWaitingList = await _waitingListService.AddToWaitingListAsync(userId.Value, eventId);
                    if (addedToWaitingList)
                    {
                        TempData["Info"] = "Event is at full capacity. You've been added to the waiting list.";
                        await _loggingService.LogInfoAsync($"User {userId} added to waiting list for event {eventId}");
                    }
                    else
                    {
                        TempData["Error"] = "Failed to add to waiting list.";
                    }
                    return RedirectToAction("Details", new { id = eventId });
                }

                var rsvp = new Rsvp
                {
                    UserId = userId.Value,
                    EventId = eventId,
                    Status = status,
                    GuestCount = guestCount
                };
                _context.Add(rsvp);
            }

            await _context.SaveChangesAsync();

            // Send confirmation email
            var user = await _context.Users.FindAsync(userId);
            if (user != null)
            {
                await _emailService.SendRsvpConfirmationAsync(user.Email!, user.FirstName!, evt.Title!, evt.StartDate.ToString("g"));
            }

            await _loggingService.LogInfoAsync($"User {userId} RSVP'd to event {eventId} with status: {status}");

            TempData["Success"] = "Your RSVP has been confirmed!";
            return RedirectToAction("Details", new { id = eventId });
        }

        // GET: Event/Reviews/5
        public async Task<IActionResult> Reviews(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var evt = await _context.Events
                .Include(e => e.Feedbacks!)
                .ThenInclude(f => f.User)
                .Include(e => e.Rsvps)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (evt == null)
            {
                return NotFound();
            }

            var approvedFeedbacks = evt.Feedbacks?.Where(f => f.IsApproved).ToList() ?? new();

            // Calculate rating distribution
            var ratingDistribution = new Dictionary<int, int>();
            for (int i = 1; i <= 5; i++)
            {
                ratingDistribution[i] = approvedFeedbacks.Count(f => f.Rating == i);
            }

            var viewModel = new EventReviewViewModel
            {
                Event = evt,
                Reviews = approvedFeedbacks,
                AverageRating = approvedFeedbacks.Any() ? approvedFeedbacks.Average(f => f.Rating) : 0,
                RatingDistribution = ratingDistribution
            };

            return View(viewModel);
        }

        // POST: Event/CreateReview
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateReview(int eventId, int rating, string? comment)
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // Verify user attended the event
            var rsvp = await _context.Rsvps
                .FirstOrDefaultAsync(r => r.UserId == userId && r.EventId == eventId && r.Status == "Attending");

            if (rsvp == null)
            {
                TempData["Error"] = "You must attend the event to leave a review.";
                return RedirectToAction("Reviews", new { id = eventId });
            }

            // Check if already reviewed
            var existingReview = await _context.Feedbacks
                .FirstOrDefaultAsync(f => f.UserId == userId && f.EventId == eventId);

            if (existingReview != null)
            {
                TempData["Error"] = "You have already reviewed this event.";
                return RedirectToAction("Reviews", new { id = eventId });
            }

            if (rating < 1 || rating > 5)
            {
                TempData["Error"] = "Rating must be between 1 and 5.";
                return RedirectToAction("Reviews", new { id = eventId });
            }

            var feedback = new Feedback
            {
                UserId = userId.Value,
                EventId = eventId,
                Rating = rating,
                Comment = comment,
                IsApproved = true
            };

            _context.Add(feedback);
            await _context.SaveChangesAsync();

            await _loggingService.LogInfoAsync($"User {userId} created review for event {eventId}");

            TempData["Success"] = "Your review has been submitted!";
            return RedirectToAction("Reviews", new { id = eventId });
        }
    }
}

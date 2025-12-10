using Microsoft.EntityFrameworkCore;
using EventManagementSystem.Models;

namespace EventManagementSystem.Services
{
    public interface IWaitingListService
    {
        Task<bool> AddToWaitingListAsync(int userId, int eventId);
        Task<bool> RemoveFromWaitingListAsync(int userId, int eventId);
        Task AutoPromoteFromWaitingListAsync(int eventId);
        Task<int> GetWaitingListPositionAsync(int userId, int eventId);
        Task<List<WaitingList>> GetEventWaitingListAsync(int eventId);
    }

    public class WaitingListService : IWaitingListService
    {
        private readonly ApplicationDbContext _context;
        private readonly IEmailService _emailService;
        private readonly ILoggingService _loggingService;

        public WaitingListService(ApplicationDbContext context, IEmailService emailService, ILoggingService loggingService)
        {
            _context = context;
            _emailService = emailService;
            _loggingService = loggingService;
        }

        public async Task<bool> AddToWaitingListAsync(int userId, int eventId)
        {
            try
            {
                // Check if already on waiting list
                var existing = await _context.WaitingLists
                    .FirstOrDefaultAsync(w => w.UserId == userId && w.EventId == eventId);

                if (existing != null)
                {
                    return false; // Already on waiting list
                }

                // Get max priority (next position)
                var maxPriority = await _context.WaitingLists
                    .Where(w => w.EventId == eventId)
                    .MaxAsync(w => (int?)w.Priority) ?? 0;

                var waitingListEntry = new WaitingList
                {
                    UserId = userId,
                    EventId = eventId,
                    Priority = maxPriority + 1
                };

                _context.Add(waitingListEntry);
                await _context.SaveChangesAsync();

                await _loggingService.LogInfoAsync($"User {userId} added to waiting list for event {eventId}");
                return true;
            }
            catch (Exception ex)
            {
                await _loggingService.LogErrorAsync("Error adding user to waiting list", ex);
                return false;
            }
        }

        public async Task<bool> RemoveFromWaitingListAsync(int userId, int eventId)
        {
            try
            {
                var waitingListEntry = await _context.WaitingLists
                    .FirstOrDefaultAsync(w => w.UserId == userId && w.EventId == eventId);

                if (waitingListEntry == null)
                    return false;

                _context.WaitingLists.Remove(waitingListEntry);

                // Update priorities for remaining entries
                var remainingEntries = await _context.WaitingLists
                    .Where(w => w.EventId == eventId && w.Priority > waitingListEntry.Priority)
                    .OrderBy(w => w.Priority)
                    .ToListAsync();

                for (int i = 0; i < remainingEntries.Count; i++)
                {
                    remainingEntries[i].Priority = waitingListEntry.Priority + i;
                }

                await _context.SaveChangesAsync();

                await _loggingService.LogInfoAsync($"User {userId} removed from waiting list for event {eventId}");
                return true;
            }
            catch (Exception ex)
            {
                await _loggingService.LogErrorAsync("Error removing user from waiting list", ex);
                return false;
            }
        }

        public async Task AutoPromoteFromWaitingListAsync(int eventId)
        {
            try
            {
                var @event = await _context.Events
                    .Include(e => e.Rsvps)
                    .FirstOrDefaultAsync(e => e.Id == eventId);

                if (@event == null)
                    return;

                // Get current attendee count
                var currentAttendees = @event.Rsvps?.Count(r => r.Status == "Attending") ?? 0;
                var availableSpots = @event.MaxAttendees - currentAttendees;

                if (availableSpots <= 0)
                    return; // Event is full

                // Get users from waiting list in priority order
                var waitingUsers = await _context.WaitingLists
                    .Where(w => w.EventId == eventId)
                    .OrderBy(w => w.Priority)
                    .Take(availableSpots)
                    .Include(w => w.User)
                    .ToListAsync();

                foreach (var waitingEntry in waitingUsers)
                {
                    // Create RSVP for waiting list user
                    var rsvp = new Rsvp
                    {
                        UserId = waitingEntry.UserId,
                        EventId = eventId,
                        Status = "Attending",
                        GuestCount = 1
                    };

                    _context.Add(rsvp);

                    // Create notification
                    var notification = new Notification
                    {
                        UserId = waitingEntry.UserId,
                        EventId = eventId,
                        Type = "SPOT_AVAILABLE",
                        Title = "Spot Available!",
                        Message = $"A spot has opened up for {@event.Title}. You've been automatically promoted from the waiting list!",
                        IsRead = false
                    };

                    _context.Add(notification);

                    // Send email
                    if (waitingEntry.User != null)
                    {
                        await _emailService.SendEmailAsync(
                            waitingEntry.User.Email!,
                            "Spot Available - Event Registration",
                            $@"
                                <h2>Spot Available!</h2>
                                <p>Hi {waitingEntry.User.FirstName},</p>
                                <p>Great news! A spot has opened up for <strong>{@event.Title}</strong>.</p>
                                <p>You've been automatically promoted from the waiting list and are now registered to attend!</p>
                                <p><strong>Event Details:</strong></p>
                                <ul>
                                    <li><strong>Date:</strong> {@event.StartDate:g}</li>
                                    <li><strong>Location:</strong> {@event.Location}</li>
                                </ul>
                            "
                        );
                    }

                    // Remove from waiting list
                    _context.WaitingLists.Remove(waitingEntry);

                    await _loggingService.LogInfoAsync($"User {waitingEntry.UserId} auto-promoted from waiting list for event {eventId}");
                }

                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                await _loggingService.LogErrorAsync("Error auto-promoting from waiting list", ex);
            }
        }

        public async Task<int> GetWaitingListPositionAsync(int userId, int eventId)
        {
            try
            {
                var waitingEntry = await _context.WaitingLists
                    .FirstOrDefaultAsync(w => w.UserId == userId && w.EventId == eventId);

                return waitingEntry?.Priority ?? -1; // -1 means not on waiting list
            }
            catch (Exception ex)
            {
                await _loggingService.LogErrorAsync("Error getting waiting list position", ex);
                return -1;
            }
        }

        public async Task<List<WaitingList>> GetEventWaitingListAsync(int eventId)
        {
            try
            {
                return await _context.WaitingLists
                    .Where(w => w.EventId == eventId)
                    .Include(w => w.User)
                    .OrderBy(w => w.Priority)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await _loggingService.LogErrorAsync("Error retrieving event waiting list", ex);
                return new List<WaitingList>();
            }
        }
    }
}

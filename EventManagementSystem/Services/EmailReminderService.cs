using Microsoft.EntityFrameworkCore;
using EventManagementSystem.Models;

namespace EventManagementSystem.Services
{
    public interface IEmailReminderService
    {
        Task SendEventRemindersAsync();
    }

    public class EmailReminderService : IEmailReminderService
    {
        private readonly ApplicationDbContext _context;
        private readonly IEmailService _emailService;
        private readonly ILoggingService _loggingService;
        private readonly ILogger<EmailReminderService> _logger;

        public EmailReminderService(
            ApplicationDbContext context,
            IEmailService emailService,
            ILoggingService loggingService,
            ILogger<EmailReminderService> logger)
        {
            _context = context;
            _emailService = emailService;
            _loggingService = loggingService;
            _logger = logger;
        }

        public async Task SendEventRemindersAsync()
        {
            try
            {
                _logger.LogInformation("Email reminder service started");
                await _loggingService.LogInfoAsync("Email reminder service started");

                // Get events happening in 24 hours that haven't been reminded yet
                var now = DateTime.UtcNow;
                var reminderTime = now.AddHours(24);

                var upcomingEvents = await _context.Events
                    .Where(e => e.StartDate > now && 
                                e.StartDate <= reminderTime && 
                                e.Status == "Active" &&
                                !e.IsArchived)
                    .Include(e => e.Rsvps)
                    .ThenInclude(r => r.User)
                    .ToListAsync();

                _logger.LogInformation($"Found {upcomingEvents.Count} events to send reminders for");

                foreach (var @event in upcomingEvents)
                {
                    // Get attendees
                    var attendees = @event.Rsvps
                        ?.Where(r => r.Status == "Attending" && r.User != null)
                        .ToList() ?? new List<Rsvp>();

                    foreach (var attendee in attendees)
                    {
                        // Check if already notified
                        var existingNotification = await _context.Notifications
                            .FirstOrDefaultAsync(n => n.UserId == attendee.UserId &&
                                                      n.EventId == @event.Id &&
                                                      n.Type == "EVENT_REMINDER");

                        if (existingNotification != null)
                            continue; // Already reminded

                        // Send email reminder
                        await _emailService.SendEventReminderAsync(
                            attendee.User!.Email!,
                            attendee.User.FirstName!,
                            @event.Title!,
                            @event.StartDate.ToString("g"),
                            @event.Location!
                        );

                        // Create notification
                        var notification = new Notification
                        {
                            UserId = attendee.UserId,
                            EventId = @event.Id,
                            Type = "EVENT_REMINDER",
                            Title = "Event Reminder",
                            Message = $"Your event '{@event.Title}' is happening in 24 hours!",
                            IsRead = false
                        };

                        _context.Add(notification);

                        _logger.LogInformation($"Reminder sent to {attendee.User.Email} for event {attendee.EventId}");
                    }

                    await _context.SaveChangesAsync();
                }

                await _loggingService.LogInfoAsync($"Email reminder service completed. {upcomingEvents.Count} events processed");
                _logger.LogInformation("Email reminder service completed successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in email reminder service");
                await _loggingService.LogErrorAsync("Error in email reminder service", ex);
            }
        }
    }
}

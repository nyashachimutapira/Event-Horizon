using System;

namespace EventManagementSystem.Models
{
    public class Notification
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int? EventId { get; set; }
        public string? Type { get; set; } // RSVP_CONFIRMATION, EVENT_REMINDER, EVENT_CANCELLED, SPOT_AVAILABLE
        public string? Title { get; set; }
        public string? Message { get; set; }
        public bool IsRead { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public User? User { get; set; }
        public Event? Event { get; set; }
    }
}

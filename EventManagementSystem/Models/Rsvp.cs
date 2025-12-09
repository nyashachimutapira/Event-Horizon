using System;

namespace EventManagementSystem.Models
{
    public class Rsvp
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int EventId { get; set; }
        public string? Status { get; set; } // Attending, Maybe, NotAttending
        public int GuestCount { get; set; }
        public DateTime RsvpDate { get; set; } = DateTime.UtcNow;
        
        public User? User { get; set; }
        public Event? Event { get; set; }
    }
}

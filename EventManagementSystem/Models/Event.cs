using System;
using System.Collections.Generic;

namespace EventManagementSystem.Models
{
    public class Event
    {
        public int Id { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string? Location { get; set; }
        public int MaxAttendees { get; set; }
        public string? Category { get; set; }
        public string? ImageUrl { get; set; }
        public int CreatedById { get; set; }
        public string? Status { get; set; } = "Active"; // Active, Cancelled, Rescheduled
        public DateTime? CancelledAt { get; set; }
        public string? CancellationReason { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        
        public User? CreatedBy { get; set; }
        public ICollection<Rsvp>? Rsvps { get; set; } = new List<Rsvp>();
        public ICollection<Feedback>? Feedbacks { get; set; } = new List<Feedback>();
        public ICollection<WaitingList>? WaitingList { get; set; } = new List<WaitingList>();
        public ICollection<Notification>? Notifications { get; set; } = new List<Notification>();
    }
}

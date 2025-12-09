using System;
using System.Collections.Generic;

namespace EventManagementSystem.Models
{
    public class User
    {
        public int Id { get; set; }
        public string? Username { get; set; }
        public string? Email { get; set; }
        public string? PasswordHash { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? AvatarUrl { get; set; }
        public string? Bio { get; set; }
        public string? Location { get; set; }
        public bool NotificationsEnabled { get; set; } = true;
        public bool IsAdmin { get; set; } = false;
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        
        public ICollection<Event>? CreatedEvents { get; set; } = new List<Event>();
        public ICollection<Rsvp>? Rsvps { get; set; } = new List<Rsvp>();
        public ICollection<Feedback>? Feedbacks { get; set; } = new List<Feedback>();
    }
}

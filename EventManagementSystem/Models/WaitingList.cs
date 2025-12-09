using System;

namespace EventManagementSystem.Models
{
    public class WaitingList
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int EventId { get; set; }
        public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
        public int Priority { get; set; } // Order in waiting list
        public bool IsNotified { get; set; } = false;
        
        public User? User { get; set; }
        public Event? Event { get; set; }
    }
}

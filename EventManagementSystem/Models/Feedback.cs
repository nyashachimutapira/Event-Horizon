using System;
using System.ComponentModel.DataAnnotations;

namespace EventManagementSystem.Models
{
    public class Feedback
    {
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        public int EventId { get; set; }

        [Required(ErrorMessage = "Rating is required")]
        [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5")]
        public int Rating { get; set; } // 1-5 stars

        [StringLength(1000, MinimumLength = 10, ErrorMessage = "Comment must be between 10 and 1000 characters")]
        public string? Comment { get; set; }

        public bool IsApproved { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public User? User { get; set; }
        public Event? Event { get; set; }
    }
}

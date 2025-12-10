using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace EventManagementSystem.Models
{
    public class Event
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Event title is required")]
        [StringLength(200, MinimumLength = 5, ErrorMessage = "Title must be between 5 and 200 characters")]
        public string? Title { get; set; }

        [Required(ErrorMessage = "Description is required")]
        [StringLength(2000, MinimumLength = 10, ErrorMessage = "Description must be between 10 and 2000 characters")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Start date is required")]
        [DataType(DataType.DateTime)]
        public DateTime StartDate { get; set; }

        [Required(ErrorMessage = "End date is required")]
        [DataType(DataType.DateTime)]
        public DateTime EndDate { get; set; }

        [Required(ErrorMessage = "Location is required")]
        [StringLength(200, ErrorMessage = "Location cannot exceed 200 characters")]
        public string? Location { get; set; }

        [Required(ErrorMessage = "Maximum attendees is required")]
        [Range(1, 100000, ErrorMessage = "Maximum attendees must be between 1 and 100,000")]
        public int MaxAttendees { get; set; }

        [Required(ErrorMessage = "Category is required")]
        [StringLength(50, ErrorMessage = "Category cannot exceed 50 characters")]
        public string? Category { get; set; }

        [Url(ErrorMessage = "Invalid image URL format")]
        public string? ImageUrl { get; set; }

        public int CreatedById { get; set; }

        [StringLength(20)]
        public string? Status { get; set; } = "Active"; // Active, Cancelled, Rescheduled

        public DateTime? CancelledAt { get; set; }

        [StringLength(500, ErrorMessage = "Cancellation reason cannot exceed 500 characters")]
        public string? CancellationReason { get; set; }

        // Soft Delete
        public bool IsArchived { get; set; } = false;
        public DateTime? ArchivedAt { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        
        public User? CreatedBy { get; set; }
        public ICollection<Rsvp>? Rsvps { get; set; } = new List<Rsvp>();
        public ICollection<Feedback>? Feedbacks { get; set; } = new List<Feedback>();
        public ICollection<WaitingList>? WaitingList { get; set; } = new List<WaitingList>();
        public ICollection<Notification>? Notifications { get; set; } = new List<Notification>();
    }
}

namespace EventManagementSystem.ViewModels.Api
{
    public class EventApiDto
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
        public string? CreatedByName { get; set; }
        public string? Status { get; set; }
        public int AttendeeCount { get; set; }
        public double AverageRating { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class CreateEventApiDto
    {
        public required string Title { get; set; }
        public required string Description { get; set; }
        public required DateTime StartDate { get; set; }
        public required DateTime EndDate { get; set; }
        public required string Location { get; set; }
        public required int MaxAttendees { get; set; }
        public required string Category { get; set; }
        public string? ImageUrl { get; set; }
    }

    public class UpdateEventApiDto
    {
        public string? Title { get; set; }
        public string? Description { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? Location { get; set; }
        public int? MaxAttendees { get; set; }
        public string? Category { get; set; }
        public string? ImageUrl { get; set; }
    }
}

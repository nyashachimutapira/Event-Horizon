namespace EventManagementSystem.ViewModels.Api
{
    public class ReviewApiDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string? UserName { get; set; }
        public int EventId { get; set; }
        public int Rating { get; set; }
        public string? Comment { get; set; }
        public bool IsApproved { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class CreateReviewApiDto
    {
        public required int EventId { get; set; }
        public required int Rating { get; set; }
        public string? Comment { get; set; }
    }

    public class UpdateReviewApiDto
    {
        public int? Rating { get; set; }
        public string? Comment { get; set; }
    }

    public class EventRatingSummaryDto
    {
        public int EventId { get; set; }
        public string? EventTitle { get; set; }
        public double AverageRating { get; set; }
        public int TotalReviews { get; set; }
        public Dictionary<int, int> RatingDistribution { get; set; } = new();
        public List<ReviewApiDto> TopReviews { get; set; } = new();
    }
}

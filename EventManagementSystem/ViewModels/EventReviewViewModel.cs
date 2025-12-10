using EventManagementSystem.Models;

namespace EventManagementSystem.ViewModels
{
    public class EventReviewViewModel
    {
        public Event Event { get; set; } = new();
        public List<Feedback> Reviews { get; set; } = new();
        public double AverageRating { get; set; }
        public Dictionary<int, int> RatingDistribution { get; set; } = new();
    }
}

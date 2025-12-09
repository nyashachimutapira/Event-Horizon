using EventManagementSystem.Models;

namespace EventManagementSystem.ViewModels
{
    public class AdminDashboardViewModel
    {
        public int TotalUsers { get; set; }
        public int TotalEvents { get; set; }
        public int TotalAttendances { get; set; }
        public int PendingModerations { get; set; }
        public int EventsThisMonth { get; set; }
        public int TotalFeedback { get; set; }
        public List<Event> RecentEvents { get; set; } = new();
        public List<Event> TopEvents { get; set; } = new();
    }
}

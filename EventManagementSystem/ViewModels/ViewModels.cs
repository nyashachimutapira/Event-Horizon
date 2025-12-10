using EventManagementSystem.Models;

namespace EventManagementSystem.ViewModels
{
    public class UserProfileViewModel
    {
        public User User { get; set; }
        public List<Event> CreatedEvents { get; set; } = new();
        public List<Event> AttendingEvents { get; set; } = new();
    }
}

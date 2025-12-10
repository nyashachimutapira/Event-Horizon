using EventManagementSystem.Models;

namespace EventManagementSystem.ViewModels
{
    public class CalendarViewModel
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public string MonthName => new System.Globalization.CultureInfo("en-ZA").DateTimeFormat.GetMonthName(Month);
        public int DaysInMonth { get; set; }
        public bool IsMyCalendar { get; set; }
        public List<Event> Events { get; set; } = new();
    }
}

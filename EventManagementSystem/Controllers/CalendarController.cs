using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EventManagementSystem.Models;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;



namespace EventManagementSystem.Controllers
{
    using EventManagementSystem.ViewModels;
    public class CalendarController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CalendarController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Calendar/Index
        public async Task<IActionResult> Index(int? month, int? year)
        {
            var now = DateTime.Now;
            var displayMonth = month ?? now.Month;
            var displayYear = year ?? now.Year;

            var startDate = new DateTime(displayYear, displayMonth, 1);
            var endDate = startDate.AddMonths(1).AddDays(-1);

            var events = await _context.Events
                .Include(e => e.CreatedBy)
                .Include(e => e.Rsvps)
                .Where(e => e.Status == "Active" && 
                           e.StartDate.Date >= startDate.Date && 
                           e.StartDate.Date <= endDate.Date)
                .OrderBy(e => e.StartDate)
                .ToListAsync();

            var calendarModel = new CalendarViewModel
            {
                Month = displayMonth,
                Year = displayYear,
                Events = events,
                DaysInMonth = System.Globalization.CultureInfo.GetCultureInfo("en-ZA").Calendar.GetDaysInMonth(displayYear, displayMonth)
            };

            return View(calendarModel);
        }

        // GET: Calendar/MyCalendar
        public async Task<IActionResult> MyCalendar(int? month, int? year)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return RedirectToAction("Login", "Account");

            var now = DateTime.Now;
            var displayMonth = month ?? now.Month;
            var displayYear = year ?? now.Year;

            var startDate = new DateTime(displayYear, displayMonth, 1);
            var endDate = startDate.AddMonths(1).AddDays(-1);

            var userRsvps = await _context.Rsvps
                .Where(r => r.UserId == userId && r.Status == "Attending")
                .Select(r => r.EventId)
                .ToListAsync();

            var events = await _context.Events
                .Include(e => e.CreatedBy)
                .Include(e => e.Rsvps)
                .Where(e => e.Status == "Active" &&
                           userRsvps.Contains(e.Id) &&
                           e.StartDate.Date >= startDate.Date && 
                           e.StartDate.Date <= endDate.Date)
                .OrderBy(e => e.StartDate)
                .ToListAsync();

            var calendarModel = new CalendarViewModel
            {
                Month = displayMonth,
                Year = displayYear,
                Events = events,
                IsMyCalendar = true,
                DaysInMonth = System.Globalization.CultureInfo.GetCultureInfo("en-ZA").Calendar.GetDaysInMonth(displayYear, displayMonth)
            };

            return View("Index", calendarModel);
        }

        // GET: Calendar/GetDayEvents
        [HttpGet]
        public async Task<IActionResult> GetDayEvents(int day, int month, int year)
        {
            var date = new DateTime(year, month, day);
            var endDate = date.AddDays(1);

            var events = await _context.Events
                .Include(e => e.CreatedBy)
                .Include(e => e.Rsvps)
                .Where(e => e.Status == "Active" &&
                           e.StartDate >= date && 
                           e.StartDate < endDate)
                .OrderBy(e => e.StartDate)
                .ToListAsync();

            return PartialView("_DayEventsList", events);
        }
    }
}

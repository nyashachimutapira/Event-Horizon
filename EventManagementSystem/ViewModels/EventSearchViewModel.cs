using EventManagementSystem.Models;
using System;
using System.Collections.Generic;

namespace EventManagementSystem.ViewModels
{
    public class EventSearchViewModel
    {
        public string? SearchTerm { get; set; }
        public string? Category { get; set; }
        public string? Location { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        
        // Pagination
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public int TotalCount { get; set; }
        public int TotalPages => (TotalCount + PageSize - 1) / PageSize;

        // Results
        public List<Event> Events { get; set; } = new();
    }
}

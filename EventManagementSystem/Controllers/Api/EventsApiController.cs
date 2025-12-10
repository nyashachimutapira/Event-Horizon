using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EventManagementSystem.Models;
using EventManagementSystem.ViewModels.Api;
using EventManagementSystem.Services;

namespace EventManagementSystem.Controllers.Api
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class EventsApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IFileUploadService _fileUploadService;
        private readonly ILoggingService _loggingService;

        public EventsApiController(ApplicationDbContext context, IFileUploadService fileUploadService, ILoggingService loggingService)
        {
            _context = context;
            _fileUploadService = fileUploadService;
            _loggingService = loggingService;
        }

        /// <summary>
        /// Get all events with pagination and filtering
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<PaginatedApiResponse<EventApiDto>>> GetEvents(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? category = null,
            [FromQuery] string? location = null,
            [FromQuery] string? searchTerm = null)
        {
            try
            {
                const int maxPageSize = 50;
                pageSize = Math.Min(pageSize, maxPageSize);

                var query = _context.Events
                    .Where(e => e.Status == "Active")
                    .Include(e => e.CreatedBy)
                    .Include(e => e.Rsvps)
                    .Include(e => e.Feedbacks)
                    .AsQueryable();

                // Apply filters
                if (!string.IsNullOrWhiteSpace(category))
                    query = query.Where(e => e.Category == category);

                if (!string.IsNullOrWhiteSpace(location))
                    query = query.Where(e => e.Location!.Contains(location));

                if (!string.IsNullOrWhiteSpace(searchTerm))
                    query = query.Where(e => e.Title!.Contains(searchTerm) || e.Description!.Contains(searchTerm));

                var totalCount = await query.CountAsync();

                var events = await query
                    .OrderBy(e => e.StartDate)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                var eventDtos = events.Select(e => MapToEventDto(e)).ToList();

                return Ok(PaginatedApiResponse<EventApiDto>.Ok(eventDtos, page, pageSize, totalCount));
            }
            catch (Exception ex)
            {
                await _loggingService.LogErrorAsync("Error retrieving events", ex);
                return BadRequest(ApiResponse<object>.Error("Failed to retrieve events"));
            }
        }

        /// <summary>
        /// Get event by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<EventApiDto>>> GetEvent(int id)
        {
            try
            {
                var @event = await _context.Events
                    .Include(e => e.CreatedBy)
                    .Include(e => e.Rsvps)
                    .Include(e => e.Feedbacks)
                    .FirstOrDefaultAsync(e => e.Id == id);

                if (@event == null)
                    return NotFound(ApiResponse<EventApiDto>.Error("Event not found"));

                return Ok(ApiResponse<EventApiDto>.Ok(MapToEventDto(@event)));
            }
            catch (Exception ex)
            {
                await _loggingService.LogErrorAsync($"Error retrieving event {id}", ex);
                return BadRequest(ApiResponse<EventApiDto>.Error("Failed to retrieve event"));
            }
        }

        /// <summary>
        /// Create a new event (requires authentication)
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<ApiResponse<EventApiDto>>> CreateEvent([FromBody] CreateEventApiDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList();
                    return BadRequest(ApiResponse<EventApiDto>.Error("Validation failed", errors));
                }

                var userId = HttpContext.Session.GetInt32("UserId");
                if (userId == null)
                    return Unauthorized(ApiResponse<EventApiDto>.Error("User not authenticated"));

                var @event = new Event
                {
                    Title = dto.Title,
                    Description = dto.Description,
                    StartDate = dto.StartDate,
                    EndDate = dto.EndDate,
                    Location = dto.Location,
                    MaxAttendees = dto.MaxAttendees,
                    Category = dto.Category,
                    ImageUrl = dto.ImageUrl,
                    CreatedById = userId.Value,
                    Status = "Active"
                };

                _context.Add(@event);
                await _context.SaveChangesAsync();

                await _loggingService.LogInfoAsync($"API: Event created by user {userId}: {@event.Title}");

                return CreatedAtAction(nameof(GetEvent), new { id = @event.Id }, ApiResponse<EventApiDto>.Ok(MapToEventDto(@event), "Event created successfully"));
            }
            catch (Exception ex)
            {
                await _loggingService.LogErrorAsync("Error creating event via API", ex);
                return BadRequest(ApiResponse<EventApiDto>.Error("Failed to create event"));
            }
        }

        /// <summary>
        /// Update an event (only creator or admin)
        /// </summary>
        [HttpPut("{id}")]
        public async Task<ActionResult<ApiResponse<EventApiDto>>> UpdateEvent(int id, [FromBody] UpdateEventApiDto dto)
        {
            try
            {
                var userId = HttpContext.Session.GetInt32("UserId");
                if (userId == null)
                    return Unauthorized(ApiResponse<EventApiDto>.Error("User not authenticated"));

                var @event = await _context.Events.FindAsync(id);
                if (@event == null)
                    return NotFound(ApiResponse<EventApiDto>.Error("Event not found"));

                // Check permissions
                var user = await _context.Users.FindAsync(userId);
                if (@event.CreatedById != userId && !user!.IsAdmin)
                    return Forbid();

                // Update fields
                if (!string.IsNullOrWhiteSpace(dto.Title))
                    @event.Title = dto.Title;
                if (!string.IsNullOrWhiteSpace(dto.Description))
                    @event.Description = dto.Description;
                if (dto.StartDate.HasValue)
                    @event.StartDate = dto.StartDate.Value;
                if (dto.EndDate.HasValue)
                    @event.EndDate = dto.EndDate.Value;
                if (!string.IsNullOrWhiteSpace(dto.Location))
                    @event.Location = dto.Location;
                if (dto.MaxAttendees.HasValue)
                    @event.MaxAttendees = dto.MaxAttendees.Value;
                if (!string.IsNullOrWhiteSpace(dto.Category))
                    @event.Category = dto.Category;
                if (!string.IsNullOrWhiteSpace(dto.ImageUrl))
                    @event.ImageUrl = dto.ImageUrl;

                @event.UpdatedAt = DateTime.UtcNow;

                _context.Update(@event);
                await _context.SaveChangesAsync();

                await _loggingService.LogInfoAsync($"API: Event {id} updated by user {userId}");

                return Ok(ApiResponse<EventApiDto>.Ok(MapToEventDto(@event), "Event updated successfully"));
            }
            catch (Exception ex)
            {
                await _loggingService.LogErrorAsync($"Error updating event {id} via API", ex);
                return BadRequest(ApiResponse<EventApiDto>.Error("Failed to update event"));
            }
        }

        /// <summary>
        /// Delete an event
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<ActionResult<ApiResponse<object>>> DeleteEvent(int id)
        {
            try
            {
                var userId = HttpContext.Session.GetInt32("UserId");
                if (userId == null)
                    return Unauthorized(ApiResponse<object>.Error("User not authenticated"));

                var @event = await _context.Events.FindAsync(id);
                if (@event == null)
                    return NotFound(ApiResponse<object>.Error("Event not found"));

                // Check permissions
                var user = await _context.Users.FindAsync(userId);
                if (@event.CreatedById != userId && !user!.IsAdmin)
                    return Forbid();

                _context.Events.Remove(@event);
                await _context.SaveChangesAsync();

                await _loggingService.LogInfoAsync($"API: Event {id} deleted by user {userId}");

                return Ok(ApiResponse<object>.Ok(null, "Event deleted successfully"));
            }
            catch (Exception ex)
            {
                await _loggingService.LogErrorAsync($"Error deleting event {id} via API", ex);
                return BadRequest(ApiResponse<object>.Error("Failed to delete event"));
            }
        }

        private EventApiDto MapToEventDto(Event @event)
        {
            return new EventApiDto
            {
                Id = @event.Id,
                Title = @event.Title,
                Description = @event.Description,
                StartDate = @event.StartDate,
                EndDate = @event.EndDate,
                Location = @event.Location,
                MaxAttendees = @event.MaxAttendees,
                Category = @event.Category,
                ImageUrl = @event.ImageUrl,
                CreatedById = @event.CreatedById,
                CreatedByName = $"{@event.CreatedBy?.FirstName} {@event.CreatedBy?.LastName}",
                Status = @event.Status,
                AttendeeCount = @event.Rsvps?.Count(r => r.Status == "Attending") ?? 0,
                AverageRating = @event.Feedbacks?.Any() == true ? @event.Feedbacks.Average(f => f.Rating) : 0,
                CreatedAt = @event.CreatedAt
            };
        }
    }
}

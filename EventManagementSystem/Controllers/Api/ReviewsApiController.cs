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
    public class ReviewsApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILoggingService _loggingService;

        public ReviewsApiController(ApplicationDbContext context, ILoggingService loggingService)
        {
            _context = context;
            _loggingService = loggingService;
        }

        /// <summary>
        /// Get reviews for an event with pagination
        /// </summary>
        [HttpGet("event/{eventId}")]
        public async Task<ActionResult<PaginatedApiResponse<ReviewApiDto>>> GetEventReviews(
            int eventId,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            try
            {
                const int maxPageSize = 50;
                pageSize = Math.Min(pageSize, maxPageSize);

                var @event = await _context.Events.FindAsync(eventId);
                if (@event == null)
                    return NotFound(PaginatedApiResponse<ReviewApiDto>.Error("Event not found"));

                var query = _context.Feedbacks
                    .Where(f => f.EventId == eventId && f.IsApproved)
                    .Include(f => f.User)
                    .AsQueryable();

                var totalCount = await query.CountAsync();

                var reviews = await query
                    .OrderByDescending(f => f.CreatedAt)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                var reviewDtos = reviews.Select(f => MapToReviewDto(f)).ToList();

                return Ok(PaginatedApiResponse<ReviewApiDto>.Ok(reviewDtos, page, pageSize, totalCount));
            }
            catch (Exception ex)
            {
                await _loggingService.LogErrorAsync($"Error retrieving reviews for event {eventId}", ex);
                return BadRequest(PaginatedApiResponse<ReviewApiDto>.Error("Failed to retrieve reviews"));
            }
        }

        /// <summary>
        /// Get rating summary for an event
        /// </summary>
        [HttpGet("summary/{eventId}")]
        public async Task<ActionResult<ApiResponse<EventRatingSummaryDto>>> GetRatingSummary(int eventId)
        {
            try
            {
                var @event = await _context.Events
                    .Include(e => e.Feedbacks)
                    .FirstOrDefaultAsync(e => e.Id == eventId);

                if (@event == null)
                    return NotFound(ApiResponse<EventRatingSummaryDto>.Error("Event not found"));

                var approvedFeedbacks = @event.Feedbacks?.Where(f => f.IsApproved).ToList() ?? new();

                if (!approvedFeedbacks.Any())
                {
                    return Ok(ApiResponse<EventRatingSummaryDto>.Ok(new EventRatingSummaryDto
                    {
                        EventId = eventId,
                        EventTitle = @event.Title,
                        AverageRating = 0,
                        TotalReviews = 0
                    }));
                }

                // Calculate rating distribution
                var ratingDistribution = new Dictionary<int, int>();
                for (int i = 1; i <= 5; i++)
                {
                    ratingDistribution[i] = approvedFeedbacks.Count(f => f.Rating == i);
                }

                // Get top reviews
                var topReviews = approvedFeedbacks
                    .OrderByDescending(f => f.CreatedAt)
                    .Take(5)
                    .Select(f => MapToReviewDto(f))
                    .ToList();

                var summary = new EventRatingSummaryDto
                {
                    EventId = eventId,
                    EventTitle = @event.Title,
                    AverageRating = approvedFeedbacks.Average(f => f.Rating),
                    TotalReviews = approvedFeedbacks.Count,
                    RatingDistribution = ratingDistribution,
                    TopReviews = topReviews
                };

                return Ok(ApiResponse<EventRatingSummaryDto>.Ok(summary));
            }
            catch (Exception ex)
            {
                await _loggingService.LogErrorAsync($"Error retrieving rating summary for event {eventId}", ex);
                return BadRequest(ApiResponse<EventRatingSummaryDto>.Error("Failed to retrieve rating summary"));
            }
        }

        /// <summary>
        /// Get a specific review by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<ReviewApiDto>>> GetReview(int id)
        {
            try
            {
                var feedback = await _context.Feedbacks
                    .Include(f => f.User)
                    .FirstOrDefaultAsync(f => f.Id == id && f.IsApproved);

                if (feedback == null)
                    return NotFound(ApiResponse<ReviewApiDto>.Error("Review not found"));

                return Ok(ApiResponse<ReviewApiDto>.Ok(MapToReviewDto(feedback)));
            }
            catch (Exception ex)
            {
                await _loggingService.LogErrorAsync($"Error retrieving review {id}", ex);
                return BadRequest(ApiResponse<ReviewApiDto>.Error("Failed to retrieve review"));
            }
        }

        /// <summary>
        /// Create a new review/rating (requires authentication and event attendance)
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<ApiResponse<ReviewApiDto>>> CreateReview([FromBody] CreateReviewApiDto dto)
        {
            try
            {
                var userId = HttpContext.Session.GetInt32("UserId");
                if (userId == null)
                    return Unauthorized(ApiResponse<ReviewApiDto>.Error("User not authenticated"));

                // Verify user attended the event
                var rsvp = await _context.Rsvps
                    .FirstOrDefaultAsync(r => r.UserId == userId && r.EventId == dto.EventId && r.Status == "Attending");

                if (rsvp == null)
                    return BadRequest(ApiResponse<ReviewApiDto>.Error("You must attend the event to leave a review"));

                // Check if already reviewed
                var existingReview = await _context.Feedbacks
                    .FirstOrDefaultAsync(f => f.UserId == userId && f.EventId == dto.EventId);

                if (existingReview != null)
                    return BadRequest(ApiResponse<ReviewApiDto>.Error("You have already reviewed this event"));

                if (dto.Rating < 1 || dto.Rating > 5)
                    return BadRequest(ApiResponse<ReviewApiDto>.Error("Rating must be between 1 and 5"));

                var feedback = new Feedback
                {
                    UserId = userId.Value,
                    EventId = dto.EventId,
                    Rating = dto.Rating,
                    Comment = dto.Comment,
                    IsApproved = true // Auto-approve for now, can be changed
                };

                _context.Add(feedback);
                await _context.SaveChangesAsync();

                await _loggingService.LogInfoAsync($"API: Review created by user {userId} for event {dto.EventId}");

                return CreatedAtAction(nameof(GetReview), new { id = feedback.Id }, 
                    ApiResponse<ReviewApiDto>.Ok(MapToReviewDto(feedback), "Review created successfully"));
            }
            catch (Exception ex)
            {
                await _loggingService.LogErrorAsync("Error creating review via API", ex);
                return BadRequest(ApiResponse<ReviewApiDto>.Error("Failed to create review"));
            }
        }

        /// <summary>
        /// Update a review (only by the reviewer)
        /// </summary>
        [HttpPut("{id}")]
        public async Task<ActionResult<ApiResponse<ReviewApiDto>>> UpdateReview(int id, [FromBody] UpdateReviewApiDto dto)
        {
            try
            {
                var userId = HttpContext.Session.GetInt32("UserId");
                if (userId == null)
                    return Unauthorized(ApiResponse<ReviewApiDto>.Error("User not authenticated"));

                var feedback = await _context.Feedbacks
                    .Include(f => f.User)
                    .FirstOrDefaultAsync(f => f.Id == id);

                if (feedback == null)
                    return NotFound(ApiResponse<ReviewApiDto>.Error("Review not found"));

                // Check ownership
                if (feedback.UserId != userId)
                    return Forbid();

                if (dto.Rating.HasValue)
                {
                    if (dto.Rating < 1 || dto.Rating > 5)
                        return BadRequest(ApiResponse<ReviewApiDto>.Error("Rating must be between 1 and 5"));
                    feedback.Rating = dto.Rating.Value;
                }

                if (!string.IsNullOrWhiteSpace(dto.Comment))
                    feedback.Comment = dto.Comment;

                _context.Update(feedback);
                await _context.SaveChangesAsync();

                await _loggingService.LogInfoAsync($"API: Review {id} updated by user {userId}");

                return Ok(ApiResponse<ReviewApiDto>.Ok(MapToReviewDto(feedback), "Review updated successfully"));
            }
            catch (Exception ex)
            {
                await _loggingService.LogErrorAsync($"Error updating review {id} via API", ex);
                return BadRequest(ApiResponse<ReviewApiDto>.Error("Failed to update review"));
            }
        }

        /// <summary>
        /// Delete a review (only by the reviewer)
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<ActionResult<ApiResponse<object>>> DeleteReview(int id)
        {
            try
            {
                var userId = HttpContext.Session.GetInt32("UserId");
                if (userId == null)
                    return Unauthorized(ApiResponse<object>.Error("User not authenticated"));

                var feedback = await _context.Feedbacks.FindAsync(id);
                if (feedback == null)
                    return NotFound(ApiResponse<object>.Error("Review not found"));

                // Check ownership
                if (feedback.UserId != userId)
                    return Forbid();

                _context.Feedbacks.Remove(feedback);
                await _context.SaveChangesAsync();

                await _loggingService.LogInfoAsync($"API: Review {id} deleted by user {userId}");

                return Ok(ApiResponse<object>.Ok(null, "Review deleted successfully"));
            }
            catch (Exception ex)
            {
                await _loggingService.LogErrorAsync($"Error deleting review {id} via API", ex);
                return BadRequest(ApiResponse<object>.Error("Failed to delete review"));
            }
        }

        private ReviewApiDto MapToReviewDto(Feedback feedback)
        {
            return new ReviewApiDto
            {
                Id = feedback.Id,
                UserId = feedback.UserId,
                UserName = $"{feedback.User?.FirstName} {feedback.User?.LastName}",
                EventId = feedback.EventId,
                Rating = feedback.Rating,
                Comment = feedback.Comment,
                IsApproved = feedback.IsApproved,
                CreatedAt = feedback.CreatedAt
            };
        }
    }
}

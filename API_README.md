# Event Horizon REST API

## Overview
The Event Horizon API provides RESTful endpoints for managing events, RSVPs, and user reviews. It's designed to support mobile apps, web frontends, and third-party integrations.

## Features

### Events Management
- List events with pagination and filtering
- Get event details
- Create new events (authenticated)
- Update events (creator/admin only)
- Delete events (creator/admin only)
- Search events by title, location, category

### Reviews & Ratings
- Submit event reviews and ratings (1-5 stars)
- View all reviews for an event
- Get rating summary with distribution
- Update your own reviews
- Delete your own reviews
- View only approved reviews

## API Endpoints

### Base URL
```
http://localhost:5000/api
```

### Events
- `GET /api/events` - List all events
- `GET /api/events/{id}` - Get event details
- `POST /api/events` - Create event (authenticated)
- `PUT /api/events/{id}` - Update event (creator/admin)
- `DELETE /api/events/{id}` - Delete event (creator/admin)

### Reviews
- `GET /api/reviews/event/{eventId}` - Get reviews for event
- `GET /api/reviews/summary/{eventId}` - Get rating summary
- `GET /api/reviews/{id}` - Get specific review
- `POST /api/reviews` - Create review (authenticated, must have attended)
- `PUT /api/reviews/{id}` - Update review (reviewer only)
- `DELETE /api/reviews/{id}` - Delete review (reviewer only)

## Authentication

Authentication uses HTTP sessions. Include session cookies in requests.

**To authenticate:**
1. POST to `/Account/Login` with email/password
2. Session is stored in cookies
3. Include cookies in subsequent API requests

## Response Format

All responses follow a consistent JSON format:

### Success Response (200 OK)
```json
{
  "success": true,
  "message": "Success",
  "data": { ... }
}
```

### Paginated Response
```json
{
  "success": true,
  "message": "Success",
  "data": [ ... ],
  "pageNumber": 1,
  "pageSize": 10,
  "totalCount": 42,
  "totalPages": 5
}
```

### Error Response
```json
{
  "success": false,
  "message": "Error message",
  "errors": ["Field error 1", "Field error 2"]
}
```

## Query Parameters

### Pagination
- `page` (default: 1) - Page number
- `pageSize` (default: 10, max: 50) - Items per page

### Event Filtering
- `searchTerm` - Search in title/description
- `category` - Filter by category
- `location` - Filter by location

## CORS

CORS is enabled for all origins (`*`). In production, configure for specific domains:

```csharp
// In Program.cs
options.AddPolicy("AllowSpecific", builder =>
{
    builder.WithOrigins("https://yourdomain.com")
           .AllowAnyMethod()
           .AllowAnyHeader()
           .AllowCredentials();
});
```

## Rate Limiting

Currently not implemented. Consider adding in production:
- 100 requests per minute per user
- 1000 requests per hour per user

## Error Codes

| Code | Meaning |
|------|---------|
| 200 | OK - Request successful |
| 201 | Created - Resource created |
| 400 | Bad Request - Invalid input |
| 401 | Unauthorized - Not authenticated |
| 403 | Forbidden - No permission |
| 404 | Not Found - Resource doesn't exist |
| 500 | Server Error - Internal error |

## Data Types

### Event Object
```json
{
  "id": 1,
  "title": "string",
  "description": "string",
  "startDate": "2025-12-15T09:00:00",
  "endDate": "2025-12-15T17:00:00",
  "location": "string",
  "maxAttendees": 500,
  "category": "string",
  "imageUrl": "string or null",
  "createdById": 1,
  "createdByName": "string",
  "status": "Active|Cancelled|Rescheduled",
  "attendeeCount": 250,
  "averageRating": 4.5,
  "createdAt": "2025-12-01T10:00:00"
}
```

### Review Object
```json
{
  "id": 1,
  "userId": 1,
  "userName": "string",
  "eventId": 1,
  "rating": 5,
  "comment": "string",
  "isApproved": true,
  "createdAt": "2025-12-16T10:00:00"
}
```

## Example Requests

### Get All Events
```bash
curl -X GET "http://localhost:5000/api/events?page=1&pageSize=10&category=Conference"
```

### Create Event
```bash
curl -X POST "http://localhost:5000/api/events" \
  -H "Content-Type: application/json" \
  -H "Cookie: <session-cookie>" \
  -d '{
    "title": "Spring Conference 2026",
    "description": "Annual spring technology conference",
    "startDate": "2026-03-15T09:00:00",
    "endDate": "2026-03-15T17:00:00",
    "location": "San Francisco",
    "maxAttendees": 1000,
    "category": "Conference"
  }'
```

### Create Review
```bash
curl -X POST "http://localhost:5000/api/reviews" \
  -H "Content-Type: application/json" \
  -H "Cookie: <session-cookie>" \
  -d '{
    "eventId": 1,
    "rating": 5,
    "comment": "Excellent event with great speakers!"
  }'
```

### Get Rating Summary
```bash
curl -X GET "http://localhost:5000/api/reviews/summary/1"
```

## Client Integration

### JavaScript/React
```javascript
// Get events
const response = await fetch('/api/events?page=1');
const { data, totalPages } = await response.json();

// Create event (requires auth)
const createResponse = await fetch('/api/events', {
  method: 'POST',
  headers: { 'Content-Type': 'application/json' },
  body: JSON.stringify(eventData),
  credentials: 'include' // Include session cookies
});
```

### Python
```python
import requests

# Get events
response = requests.get('http://localhost:5000/api/events')
events = response.json()

# Create event (requires session)
session = requests.Session()
session.post('http://localhost:5000/Account/Login', data=login_data)
response = session.post('http://localhost:5000/api/events', json=event_data)
```

## Logging

All API requests are logged to `Logs/app_yyyy-MM-dd.log` with:
- Request details
- User ID (if authenticated)
- Operation result
- Errors (if any)

## Security Considerations

1. **Authentication**: Sessions are secure but IP-based. Consider adding JWT tokens.
2. **Authorization**: Ownership checks for create/update/delete operations.
3. **Validation**: Input validation on all endpoints.
4. **CORS**: Currently allows all origins. Restrict in production.
5. **HTTPS**: Enable in production.

## Future Enhancements

- [ ] JWT token-based authentication
- [ ] API key authentication
- [ ] Rate limiting
- [ ] Request validation with Fluent Validation
- [ ] API versioning (v2, v3)
- [ ] Webhooks for event updates
- [ ] GraphQL endpoint
- [ ] OpenAPI/Swagger documentation

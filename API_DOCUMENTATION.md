# Event Horizon API Documentation

## Base URL
```
http://localhost:5000/api
```

## Authentication
Most endpoints require user authentication via session. The user must be logged in and have an active session.

---

## Events API

### 1. Get All Events
**Endpoint:** `GET /api/events`

**Query Parameters:**
- `page` (int, default: 1) - Page number for pagination
- `pageSize` (int, default: 10, max: 50) - Number of events per page
- `category` (string, optional) - Filter by category
- `location` (string, optional) - Filter by location
- `searchTerm` (string, optional) - Search by title or description

**Response:**
```json
{
  "success": true,
  "message": "Success",
  "data": [
    {
      "id": 1,
      "title": "Tech Conference 2025",
      "description": "Annual tech conference",
      "startDate": "2025-12-15T09:00:00",
      "endDate": "2025-12-15T17:00:00",
      "location": "New York",
      "maxAttendees": 500,
      "category": "Conference",
      "imageUrl": "/uploads/events/image.jpg",
      "createdById": 1,
      "createdByName": "John Doe",
      "status": "Active",
      "attendeeCount": 250,
      "averageRating": 4.5,
      "createdAt": "2025-12-01T10:00:00"
    }
  ],
  "pageNumber": 1,
  "pageSize": 10,
  "totalCount": 100,
  "totalPages": 10
}
```

### 2. Get Event by ID
**Endpoint:** `GET /api/events/{id}`

**Response:**
```json
{
  "success": true,
  "message": "Success",
  "data": {
    "id": 1,
    "title": "Tech Conference 2025",
    ...
  }
}
```

### 3. Create Event
**Endpoint:** `POST /api/events`

**Authentication:** Required

**Request Body:**
```json
{
  "title": "New Conference",
  "description": "Detailed description of the conference",
  "startDate": "2025-12-15T09:00:00",
  "endDate": "2025-12-15T17:00:00",
  "location": "New York",
  "maxAttendees": 500,
  "category": "Conference",
  "imageUrl": "/uploads/events/image.jpg"
}
```

**Response:** `201 Created`
```json
{
  "success": true,
  "message": "Event created successfully",
  "data": { ... }
}
```

### 4. Update Event
**Endpoint:** `PUT /api/events/{id}`

**Authentication:** Required (Creator or Admin only)

**Request Body:** (All fields optional)
```json
{
  "title": "Updated Title",
  "description": "Updated description",
  "startDate": "2025-12-15T09:00:00",
  "endDate": "2025-12-15T17:00:00",
  "location": "Updated Location",
  "maxAttendees": 600,
  "category": "Workshop",
  "imageUrl": "/uploads/events/new-image.jpg"
}
```

### 5. Delete Event
**Endpoint:** `DELETE /api/events/{id}`

**Authentication:** Required (Creator or Admin only)

**Response:** `200 OK`
```json
{
  "success": true,
  "message": "Event deleted successfully"
}
```

---

## Reviews/Ratings API

### 1. Get Event Reviews
**Endpoint:** `GET /api/reviews/event/{eventId}`

**Query Parameters:**
- `page` (int, default: 1)
- `pageSize` (int, default: 10, max: 50)

**Response:**
```json
{
  "success": true,
  "message": "Success",
  "data": [
    {
      "id": 1,
      "userId": 1,
      "userName": "Jane Smith",
      "eventId": 1,
      "rating": 5,
      "comment": "Great event! Highly recommended.",
      "isApproved": true,
      "createdAt": "2025-12-16T10:00:00"
    }
  ],
  "pageNumber": 1,
  "pageSize": 10,
  "totalCount": 42,
  "totalPages": 5
}
```

### 2. Get Rating Summary
**Endpoint:** `GET /api/reviews/summary/{eventId}`

**Response:**
```json
{
  "success": true,
  "message": "Success",
  "data": {
    "eventId": 1,
    "eventTitle": "Tech Conference 2025",
    "averageRating": 4.5,
    "totalReviews": 42,
    "ratingDistribution": {
      "1": 2,
      "2": 3,
      "3": 5,
      "4": 15,
      "5": 17
    },
    "topReviews": [
      {
        "id": 1,
        "userId": 1,
        "userName": "Jane Smith",
        "eventId": 1,
        "rating": 5,
        "comment": "Great event!",
        "isApproved": true,
        "createdAt": "2025-12-16T10:00:00"
      }
    ]
  }
}
```

### 3. Get Review by ID
**Endpoint:** `GET /api/reviews/{id}`

**Response:**
```json
{
  "success": true,
  "message": "Success",
  "data": {
    "id": 1,
    "userId": 1,
    "userName": "Jane Smith",
    "eventId": 1,
    "rating": 5,
    "comment": "Great event!",
    "isApproved": true,
    "createdAt": "2025-12-16T10:00:00"
  }
}
```

### 4. Create Review
**Endpoint:** `POST /api/reviews`

**Authentication:** Required

**Requirements:**
- User must have RSVP'd to the event with status "Attending"
- User cannot have already reviewed the event

**Request Body:**
```json
{
  "eventId": 1,
  "rating": 5,
  "comment": "This was an amazing event! Learned so much and met great people."
}
```

**Response:** `201 Created`
```json
{
  "success": true,
  "message": "Review created successfully",
  "data": { ... }
}
```

### 5. Update Review
**Endpoint:** `PUT /api/reviews/{id}`

**Authentication:** Required (Review owner only)

**Request Body:** (All fields optional)
```json
{
  "rating": 4,
  "comment": "Updated comment"
}
```

### 6. Delete Review
**Endpoint:** `DELETE /api/reviews/{id}`

**Authentication:** Required (Review owner only)

**Response:** `200 OK`
```json
{
  "success": true,
  "message": "Review deleted successfully"
}
```

---

## Error Responses

### 400 Bad Request
```json
{
  "success": false,
  "message": "Validation failed",
  "errors": [
    "Title is required",
    "Description must be between 10 and 2000 characters"
  ]
}
```

### 401 Unauthorized
```json
{
  "success": false,
  "message": "User not authenticated"
}
```

### 403 Forbidden
```json
HTTP/1.1 403 Forbidden
```

### 404 Not Found
```json
{
  "success": false,
  "message": "Event not found"
}
```

### 500 Internal Server Error
```json
{
  "success": false,
  "message": "Failed to retrieve events"
}
```

---

## Example Usage

### Using curl
```bash
# Get all events
curl -X GET "http://localhost:5000/api/events?page=1&pageSize=10"

# Get event by ID
curl -X GET "http://localhost:5000/api/events/1"

# Create event (with authentication)
curl -X POST "http://localhost:5000/api/events" \
  -H "Content-Type: application/json" \
  -d '{
    "title": "New Conference",
    "description": "A detailed description",
    "startDate": "2025-12-15T09:00:00",
    "endDate": "2025-12-15T17:00:00",
    "location": "New York",
    "maxAttendees": 500,
    "category": "Conference"
  }'

# Get event reviews
curl -X GET "http://localhost:5000/api/reviews/event/1?page=1&pageSize=10"

# Create review (with authentication)
curl -X POST "http://localhost:5000/api/reviews" \
  -H "Content-Type: application/json" \
  -d '{
    "eventId": 1,
    "rating": 5,
    "comment": "Great event!"
  }'
```

### Using JavaScript/Fetch
```javascript
// Get all events
fetch('http://localhost:5000/api/events?page=1&pageSize=10')
  .then(res => res.json())
  .then(data => console.log(data));

// Create event
fetch('http://localhost:5000/api/events', {
  method: 'POST',
  headers: { 'Content-Type': 'application/json' },
  body: JSON.stringify({
    title: 'New Conference',
    description: 'A detailed description',
    startDate: '2025-12-15T09:00:00',
    endDate: '2025-12-15T17:00:00',
    location: 'New York',
    maxAttendees: 500,
    category: 'Conference'
  })
})
.then(res => res.json())
.then(data => console.log(data));

// Get reviews for event
fetch('http://localhost:5000/api/reviews/event/1?page=1')
  .then(res => res.json())
  .then(data => console.log(data));
```

---

## Rate Limiting
Currently no rate limiting is implemented. Consider adding it in production.

## CORS
CORS is enabled for all origins. Consider restricting to specific domains in production.

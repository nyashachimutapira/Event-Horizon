# Event Horizon - Quick Start Guide

## Prerequisites
- .NET 10.0 SDK installed
- Visual Studio Code or Visual Studio 2022 (optional)

## Running the Application

### Option 1: Command Line (Recommended)
```bash
cd EventManagementSystem
dotnet restore
dotnet build
dotnet run
```

The application will start on:
- **Web App**: http://localhost:5000
- **API**: http://localhost:5000/api

### Option 2: Visual Studio
1. Open `Event-Horizon.sln`
2. Set `EventManagementSystem` as startup project
3. Press `F5` or click "Run"

## Default Test Accounts

The application comes pre-populated with sample data:

**Admin Account:**
- Email: `admin@example.com`
- Password: `Admin@123`
- Role: Administrator (full access)

**Test User Account:**
- Email: `user@example.com`
- Password: `Admin@123`
- Role: Regular User

## Sample Data
The app includes 3 sample events:
1. **Tech Conference 2025** (30 days from now)
2. **Python Workshop** (15 days from now)
3. **Web Development Meetup** (7 days from now)

## Features to Try

### User Features
1. **Browse Events**
   - Visit http://localhost:5000
   - View event list with filtering and search

2. **Create Event**
   - Click "Create Event" button
   - Fill in event details
   - Upload event image (optional)

3. **RSVP to Event**
   - Click on an event
   - Click "RSVP to Event"
   - Automatic email confirmation

4. **Leave Reviews**
   - Click "Reviews" on event page
   - Rate (1-5 stars) and comment
   - Only attendees can review

5. **Notifications**
   - Click your username → Notifications
   - View all notifications with read/unread status
   - Mark as read or delete

### Admin Features
1. **Admin Dashboard**
   - URL: http://localhost:5000/admin/dashboard
   - View statistics and recent activity

2. **User Management**
   - URL: http://localhost:5000/admin/users
   - Activate/deactivate users
   - Promote users to admin
   - Pagination support

3. **Event Management**
   - URL: http://localhost:5000/admin/events
   - View all events
   - See attendee counts

4. **Archived Events**
   - URL: http://localhost:5000/admin/archivedevents
   - Restore deleted events

5. **Analytics**
   - URL: http://localhost:5000/admin/analytics
   - Event statistics by category
   - Top events by attendance

## API Testing

### Get All Events
```bash
curl -X GET "http://localhost:5000/api/events?page=1&pageSize=10"
```

### Get Event by ID
```bash
curl -X GET "http://localhost:5000/api/events/1"
```

### Search Events
```bash
curl -X GET "http://localhost:5000/api/events?searchTerm=tech&category=Conference"
```

### Get Event Reviews
```bash
curl -X GET "http://localhost:5000/api/reviews/event/1"
```

### Get Rating Summary
```bash
curl -X GET "http://localhost:5000/api/reviews/summary/1"
```

## Project Structure

```
EventManagementSystem/
├── Controllers/          # MVC & API controllers
├── Models/              # Database entities
├── Services/            # Business logic
├── Views/               # Razor templates
├── Configuration/       # Settings classes
├── Middleware/          # Custom middleware
├── Program.cs           # App configuration
└── ApplicationDbContext.cs  # EF Core context
```

## Database
- **Type**: In-memory (no file storage needed)
- **Data persistence**: Only during app runtime
- **Reset**: Restart the application

To switch to SQLite file database, edit `Program.cs`:
```csharp
// Uncomment SQLite
options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection"));

// Comment out in-memory
// options.UseInMemoryDatabase("EventHorizonDb");
```

## Email Notifications (Optional)

To enable email sending:

1. Edit `appsettings.json`:
```json
"EmailSettings": {
  "SmtpServer": "smtp.gmail.com",
  "Port": 587,
  "SenderEmail": "your-email@gmail.com",
  "SenderPassword": "your-app-password",
  "UseSsl": true
}
```

2. For Gmail:
   - Enable 2-factor authentication
   - Create an App Password: https://myaccount.google.com/apppasswords
   - Use the generated password in appsettings.json

Without email config, the app still works but email features log to console.

## Logs
Application logs are saved to:
```
EventManagementSystem/Logs/app_YYYY-MM-DD.log
```

## Troubleshooting

### Port Already in Use
```bash
dotnet run --urls "http://localhost:5001"
```

### Build Errors
```bash
dotnet clean
dotnet restore
dotnet build
```

### Missing Dependencies
```bash
dotnet nuget locals all --clear
dotnet restore
```

## Default Port
- Development: **5000** (HTTP)
- HTTPS: **5001**

Change with environment variables:
```bash
set ASPNETCORE_URLS=http://localhost:8080
dotnet run
```

## Features Available

✅ User Authentication & Authorization
✅ Event Management (CRUD)
✅ RSVP System
✅ Waiting List Management
✅ Reviews & Ratings
✅ Email Notifications
✅ Search & Filtering
✅ Pagination
✅ Admin Dashboard
✅ REST API
✅ Background Jobs (Email Reminders)
✅ Error Handling & Logging
✅ Bootstrap UI/Styling

## Next Steps

1. **Explore the app** using the test accounts
2. **Test API endpoints** using curl or Postman
3. **Review code** in Models, Controllers, and Services folders
4. **Customize** configuration in appsettings.json

## Support
For detailed documentation, see:
- `IMPLEMENTATION_SUMMARY.md` - Complete feature list
- `API_DOCUMENTATION.md` - API endpoints reference
- `API_README.md` - API integration guide

## Running Tests
```bash
cd EventManagementSystem.Tests
dotnet test
```

Happy exploring! 🎉

# Event Management System

A comprehensive ASP.NET Core MVC application for managing events, user registrations, and event feedback.

## Features

- User authentication (Login/Register)
- Create, read, update, and delete events
- RSVP to events
- Event feedback and ratings
- Event attendance tracking
- Responsive design

## Project Structure

```
EventManagementSystem/
├── Controllers/
│   ├── EventController.cs
│   └── AccountController.cs
├── Models/
│   ├── User.cs
│   ├── Event.cs
│   ├── Rsvp.cs
│   └── Feedback.cs
├── Views/
│   ├── Events/
│   │   ├── Index.cshtml
│   │   ├── Create.cshtml
│   │   └── Details.cshtml
│   ├── Account/
│   │   ├── Login.cshtml
│   │   └── Register.cshtml
│   └── Shared/
│       └── _Layout.cshtml
├── wwwroot/
│   ├── css/
│   │   └── styles.css
│   ├── js/
│   │   └── scripts.js
│   └── images/
├── ApplicationDbContext.cs
├── Program.cs
├── appsettings.json
└── EventManagementSystem.csproj
```

## Prerequisites

- .NET 6.0 SDK or higher
- SQL Server (LocalDB or Express)
- Visual Studio 2022 or VS Code

## Installation

1. Clone the repository
2. Open the solution in Visual Studio
3. Update the connection string in `appsettings.json` if needed
4. Run migrations:
   ```
   dotnet ef database update
   ```
5. Build and run the project

## Database Setup

The application uses Entity Framework Core with SQL Server. The default connection string is configured for LocalDB:

```json
"DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=EventManagementDB;Trusted_Connection=true;"
```

## Technologies Used

- ASP.NET Core 6.0
- Entity Framework Core
- SQL Server
- HTML5 & CSS3
- JavaScript
- Razor Views

## API Endpoints

### Events
- `GET /Event` - List all events
- `GET /Event/Details/{id}` - Get event details
- `GET /Event/Create` - Create event form
- `POST /Event/Create` - Submit new event
- `GET /Event/Edit/{id}` - Edit event form
- `POST /Event/Edit/{id}` - Submit event changes
- `GET /Event/Delete/{id}` - Delete confirmation
- `POST /Event/Delete/{id}` - Confirm deletion

### Account
- `GET /Account/Login` - Login page
- `POST /Account/Login` - Process login
- `GET /Account/Register` - Registration page
- `POST /Account/Register` - Process registration
- `GET /Account/Logout` - Logout user

## Usage

1. Register a new account
2. Create events
3. View all events
4. RSVP to events
5. Leave feedback and ratings

## License

This project is open source and available under the MIT License.

using Microsoft.EntityFrameworkCore;
using EventManagementSystem;
using EventManagementSystem.Services;
using EventManagementSystem.Configuration;

partial class Program
{
    static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container
        builder.Services.AddControllers();
        builder.Services.AddControllersWithViews().AddRazorRuntimeCompilation();
        builder.Services.AddSession();
        builder.Services.AddScoped<ILoggingService, LoggingService>();
        builder.Services.AddScoped<IEmailService, EmailService>();
        builder.Services.AddScoped<IFileUploadService, FileUploadService>();
        builder.Services.AddScoped<IWaitingListService, WaitingListService>();
        builder.Services.AddScoped<IEmailReminderService, EmailReminderService>();
        builder.Services.AddHostedService<ReminderBackgroundService>();
        builder.Services.AddCors(options =>
        {
            options.AddPolicy("AllowAll", builder =>
            {
                builder.AllowAnyOrigin()
                       .AllowAnyMethod()
                       .AllowAnyHeader();
            });
        });

        // Add configuration
        builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));

        // Add logging
        builder.Logging.ClearProviders();
        builder.Logging.AddConsole();

        // Add Database Context - PostgreSQL
        builder.Services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

        // Uncomment below to use In-Memory Database for testing
        // builder.Services.AddDbContext<ApplicationDbContext>(options =>
        //     options.UseInMemoryDatabase("EventHorizonDb"));

        // Uncomment below to use SQLite file-based database instead
        // builder.Services.AddDbContext<ApplicationDbContext>(options =>
        //     options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

        var app = builder.Build();

        // Configure the HTTP request pipeline
        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Home/Error");
            app.UseHsts();
        }
        else
        {
            app.UseDeveloperExceptionPage();
        }

        // Custom error handling middleware
        app.UseMiddleware<ErrorHandlingMiddleware>();

        app.UseHttpsRedirection();
        app.UseStaticFiles();

        app.UseRouting();
        app.UseCors("AllowAll");
        app.UseSession();

        app.MapControllerRoute(
            name: "default",
            pattern: "{controller=Event}/{action=Index}/{id?}");

        app.MapControllers(); // Add this to enable API controllers

        // Seed database with sample data
        using (var scope = app.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            SeedDatabase(context);
        }

        app.Run();
    }

    static void SeedDatabase(ApplicationDbContext context)
    {
        try
        {
            // Check if data already exists
            if (context.Users.Any())
                return;

            // Create admin user
            var adminUser = new EventManagementSystem.Models.User
            {
                Username = "admin",
                Email = "admin@example.com",
                FirstName = "Admin",
                LastName = "User",
                PasswordHash = "8mDO7EzGJd7+4CaHGvfXZE8AegTJ7L1vUGZdj5BzZ1A=", // Password: Admin@123
                IsAdmin = true,
                IsActive = true
            };

            // Create test user
            var testUser = new EventManagementSystem.Models.User
            {
                Username = "testuser",
                Email = "user@example.com",
                FirstName = "Test",
                LastName = "User",
                PasswordHash = "8mDO7EzGJd7+4CaHGvfXZE8AegTJ7L1vUGZdj5BzZ1A=", // Password: Admin@123
                IsAdmin = false,
                IsActive = true
            };

            context.Users.AddRange(adminUser, testUser);
            context.SaveChanges();

            // Create sample events
            var event1 = new EventManagementSystem.Models.Event
            {
                Title = "Tech Conference 2025",
                Description = "Join us for the annual technology conference featuring keynote speakers, workshops, and networking opportunities.",
                StartDate = DateTime.UtcNow.AddDays(30),
                EndDate = DateTime.UtcNow.AddDays(31),
                Location = "San Francisco, CA",
                MaxAttendees = 500,
                Category = "Conference",
                CreatedById = adminUser.Id,
                Status = "Active"
            };

            var event2 = new EventManagementSystem.Models.Event
            {
                Title = "Python Workshop",
                Description = "Learn Python programming basics and advanced techniques in this hands-on workshop.",
                StartDate = DateTime.UtcNow.AddDays(15),
                EndDate = DateTime.UtcNow.AddDays(15),
                Location = "New York, NY",
                MaxAttendees = 30,
                Category = "Workshop",
                CreatedById = adminUser.Id,
                Status = "Active"
            };

            var event3 = new EventManagementSystem.Models.Event
            {
                Title = "Web Development Meetup",
                Description = "Monthly meetup for web developers to discuss latest trends and share experiences.",
                StartDate = DateTime.UtcNow.AddDays(7),
                EndDate = DateTime.UtcNow.AddDays(7),
                Location = "Austin, TX",
                MaxAttendees = 100,
                Category = "Meetup",
                CreatedById = testUser.Id,
                Status = "Active"
            };

            context.Events.AddRange(event1, event2, event3);
            context.SaveChanges();

            // Create sample RSVPs
            var rsvp1 = new EventManagementSystem.Models.Rsvp
            {
                UserId = testUser.Id,
                EventId = event1.Id,
                Status = "Attending",
                GuestCount = 1
            };

            context.Rsvps.Add(rsvp1);
            context.SaveChanges();

            // Create sample reviews
            var review1 = new EventManagementSystem.Models.Feedback
            {
                UserId = testUser.Id,
                EventId = event1.Id,
                Rating = 5,
                Comment = "Excellent conference! Great speakers and networking opportunities.",
                IsApproved = true
            };

            context.Feedbacks.Add(review1);
            context.SaveChanges();

            Console.WriteLine("✓ Database seeded with sample data");
            Console.WriteLine("  Admin: admin@example.com / Admin@123");
            Console.WriteLine("  User:  user@example.com / Admin@123");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"✗ Error seeding database: {ex.Message}");
        }
    }
}

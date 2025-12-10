using Microsoft.EntityFrameworkCore;
using EventManagementSystem.Models;

namespace EventManagementSystem
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public virtual DbSet<User> Users { get; set; }
        public virtual DbSet<Event> Events { get; set; }
        public virtual DbSet<Rsvp> Rsvps { get; set; }
        public virtual DbSet<Feedback> Feedbacks { get; set; }
        public virtual DbSet<Notification> Notifications { get; set; }
        public virtual DbSet<WaitingList> WaitingLists { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // User relationships
            modelBuilder.Entity<User>()
                .HasMany(u => u.CreatedEvents)
                .WithOne(e => e.CreatedBy)
                .HasForeignKey(e => e.CreatedById);

            modelBuilder.Entity<User>()
                .HasMany(u => u.Rsvps)
                .WithOne(r => r.User)
                .HasForeignKey(r => r.UserId);

            modelBuilder.Entity<User>()
                .HasMany(u => u.Feedbacks)
                .WithOne(f => f.User)
                .HasForeignKey(f => f.UserId);

            // Event relationships
            modelBuilder.Entity<Event>()
                .HasMany(e => e.Rsvps)
                .WithOne(r => r.Event)
                .HasForeignKey(r => r.EventId);

            modelBuilder.Entity<Event>()
                .HasMany(e => e.Feedbacks)
                .WithOne(f => f.Event)
                .HasForeignKey(f => f.EventId);
        }
    }
}

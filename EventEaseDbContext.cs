using Microsoft.EntityFrameworkCore;

namespace EventEase_WebApp.Data; 

public class EventEaseDbContext : DbContext
{
    public EventEaseDbContext(DbContextOptions<EventEaseDbContext> options) : base(options) { }

    public DbSet<Venue> Venues => Set<Venue>();
    public DbSet<Event> Events => Set<Event>();
    public DbSet<Booking> Bookings => Set<Booking>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Event>()
            .Property(e => e.EventDate)
            .HasColumnType("date");

        modelBuilder.Entity<Booking>()
            .Property(b => b.BookingDate)
            .HasColumnType("date");

        modelBuilder.Entity<Event>()
            .HasOne(e => e.Venue)
            .WithMany(v => v.Events)
            .HasForeignKey(e => e.VenueId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<Booking>()
            .HasOne(b => b.Venue)
            .WithMany(v => v.Bookings)
            .HasForeignKey(b => b.VenueId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Booking>()
            .HasOne(b => b.Event)
            .WithMany(e => e.Bookings)
            .HasForeignKey(b => b.EventId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Booking>()
            .HasIndex(b => new { b.VenueId, b.BookingDate })
            .IsUnique();
    }
}
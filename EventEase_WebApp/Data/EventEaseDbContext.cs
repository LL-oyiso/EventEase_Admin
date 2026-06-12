
using Microsoft.EntityFrameworkCore;
using EventEase_WebApp.Models;

namespace EventEase_WebApp.Data;

public class EventEaseDbContext : DbContext
{
    public EventEaseDbContext(DbContextOptions<EventEaseDbContext> options) : base(options) { }

    public DbSet<EventType> EventTypes => Set<EventType>();
    public DbSet<Venue> Venues => Set<Venue>();
    public DbSet<Event> Events => Set<Event>();
    public DbSet<Booking> Bookings => Set<Booking>();
    public DbSet<BookingDetail> BookingDetails => Set<BookingDetail>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Event>()
            .Property(e => e.EventDate)
            .HasColumnType("date");

        modelBuilder.Entity<Booking>()
            .Property(b => b.BookingDate)
            .HasColumnType("date");

        modelBuilder.Entity<Venue>()
            .Property(v => v.IsAvailable)
            .HasDefaultValue(true);

        modelBuilder.Entity<Event>()
            .HasOne(e => e.Venue)
            .WithMany(v => v.Events)
            .HasForeignKey(e => e.VenueId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<Event>()
            .HasOne(e => e.EventType)
            .WithMany(et => et.Events)
            .HasForeignKey(e => e.EventTypeId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<EventType>()
            .HasIndex(et => et.EventTypeName)
            .IsUnique();

        modelBuilder.Entity<EventType>().HasData(
            new EventType { EventTypeId = 1, EventTypeName = "Conference" },
            new EventType { EventTypeId = 2, EventTypeName = "Wedding" },
            new EventType { EventTypeId = 3, EventTypeName = "Concert" },
            new EventType { EventTypeId = 4, EventTypeName = "Festival" },
            new EventType { EventTypeId = 5, EventTypeName = "Corporate" },
            new EventType { EventTypeId = 6, EventTypeName = "Private" });

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

        modelBuilder.Entity<BookingDetail>()
            .HasNoKey()
            .ToView("vwBookingDetails");
    }
}
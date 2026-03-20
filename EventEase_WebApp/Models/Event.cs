using System.ComponentModel.DataAnnotations;

namespace EventEase_WebApp.Models;

public class Event
{
    [Key]
    public int EventId { get; set; }

    [Required, StringLength(120)]
    public string EventName { get; set; } = string.Empty;

    [Required, DataType(DataType.Date)]
    public DateTime EventDate { get; set; }

    [StringLength(2000)]
    public string? Description { get; set; }

    // Optional: events can exist before a venue is assigned
    public int? VenueId { get; set; }
    public Venue? Venue { get; set; }

    // Navigation (needed for EF relationships in DbContext)
    public List<Booking> Bookings { get; set; } = new();
}
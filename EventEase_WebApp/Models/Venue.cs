using System.ComponentModel.DataAnnotations;

namespace EventEase_WebApp.Models;

public class Venue
{
    [Key]
    public int VenueId { get; set; }

    [Required, StringLength(100)]
    public string VenueName { get; set; } = string.Empty;

    [Required, StringLength(200)]
    public string Location { get; set; } = string.Empty;

    [Range(1, 250000)]
    public int Capacity { get; set; }

    [Required, StringLength(500), Url]
    public string ImageUrl { get; set; } = "https://placehold.co/600x400?text=Venue";

    // Navigation (needed for EF relationships in DbContext)
    public List<Event> Events { get; set; } = new();
    public List<Booking> Bookings { get; set; } = new();
}
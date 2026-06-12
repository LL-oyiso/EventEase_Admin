using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Http;
using EventEase_WebApp.Validation;
namespace EventEase_WebApp.Models;

public class Event
{
    [Key]
    public int EventId { get; set; }

    [Required, StringLength(120)]
    public string EventName { get; set; } = string.Empty;

    [Required, DataType(DataType.Date)]
    [DateNotInPast(ErrorMessage = "Event date cannot be in the past.")]
    public DateTime EventDate { get; set; }

    [StringLength(2000)]
    public string? Description { get; set; }

    [Required, StringLength(500), Url]
    public string ImageUrl { get; set; } = "https://placehold.co/600x400?text=Event";

    [NotMapped]
    public IFormFile? ImageFile { get; set; }

    public int? EventTypeId { get; set; }
    public EventType? EventType { get; set; }

    // Optional: events can exist before a venue is assigned
    public int? VenueId { get; set; }
    public Venue? Venue { get; set; }

    // Navigation (needed for EF relationships in DbContext)
    public List<Booking> Bookings { get; set; } = new();
}
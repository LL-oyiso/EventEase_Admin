using System.ComponentModel.DataAnnotations;

namespace EventEase_WebApp.Models;

public class Booking
{
    [Key]
    public Guid BookingId { get; set; } = Guid.NewGuid();

    [Required]
    public int EventId { get; set; }
    public Event? Event { get; set; }

    [Required]
    public int VenueId { get; set; }
    public Venue? Venue { get; set; }

    [Required, DataType(DataType.Date)]
    public DateTime BookingDate { get; set; }
}
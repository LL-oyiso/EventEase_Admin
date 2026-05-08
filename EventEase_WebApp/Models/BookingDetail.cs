namespace EventEase_WebApp.Models;

public class BookingDetail
{
    public Guid BookingId { get; set; }

    public DateTime BookingDate { get; set; }

    public int EventId { get; set; }
    public string EventName { get; set; } = string.Empty;
    public DateTime EventDate { get; set; }

    public int VenueId { get; set; }
    public string VenueName { get; set; } = string.Empty;
    public string VenueLocation { get; set; } = string.Empty;
}

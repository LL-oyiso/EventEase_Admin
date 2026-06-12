using System.ComponentModel.DataAnnotations;

namespace EventEase_WebApp.Models;

public class EventType
{
    [Key]
    public int EventTypeId { get; set; }

    [Required, StringLength(80)]
    public string EventTypeName { get; set; } = string.Empty;

    public List<Event> Events { get; set; } = new();
}

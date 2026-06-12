namespace EventEase_WebApp.ViewModels;

public class DashboardViewModel
{
    public int TotalEvents { get; set; }
    public int UpcomingEventsCount { get; set; }
    public int CompletedEventsCount { get; set; }
    public int TotalBookings { get; set; }
    public int UnavailableVenuesCount { get; set; }
    public int UnclassifiedEventsCount { get; set; }

    public List<MonthlyBookingTrendItem> MonthlyBookingTrend { get; set; } = new();
    public List<EventTypeBreakdownItem> EventTypeBreakdown { get; set; } = new();
    public List<UpcomingEventRow> UpcomingEvents { get; set; } = new();
    public List<TopEventRow> TopEvents { get; set; } = new();
}

public class MonthlyBookingTrendItem
{
    public string Label { get; set; } = string.Empty;
    public int BookingCount { get; set; }
}

public class EventTypeBreakdownItem
{
    public string Name { get; set; } = string.Empty;
    public int Count { get; set; }
    public int Percentage { get; set; }
}

public class UpcomingEventRow
{
    public string EventName { get; set; } = string.Empty;
    public DateTime EventDate { get; set; }
    public string Location { get; set; } = "Unassigned";
    public int BookingCount { get; set; }
}

public class TopEventRow
{
    public string EventName { get; set; } = string.Empty;
    public DateTime EventDate { get; set; }
    public int BookingCount { get; set; }
    public string EventTypeName { get; set; } = "Unclassified";
}

using EventEase_WebApp.Data;
using EventEase_WebApp.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EventEase_WebApp.Controllers;

public class HomeController : Controller
{
    private readonly EventEaseDbContext _db;

    public HomeController(EventEaseDbContext db)
    {
        _db = db;
    }

    public async Task<IActionResult> Index()
    {
        var today = DateTime.Today;
        var firstMonth = new DateTime(today.Year, today.Month, 1).AddMonths(-5);
        var monthWindows = Enumerable.Range(0, 6)
            .Select(offset => firstMonth.AddMonths(offset))
            .ToList();

        var monthlyRaw = await _db.Bookings
            .AsNoTracking()
            .Where(b => b.BookingDate >= firstMonth)
            .GroupBy(b => new { b.BookingDate.Year, b.BookingDate.Month })
            .Select(g => new
            {
                g.Key.Year,
                g.Key.Month,
                Count = g.Count()
            })
            .ToListAsync();

        var monthlyTrend = monthWindows
            .Select(m =>
            {
                var match = monthlyRaw.FirstOrDefault(x => x.Year == m.Year && x.Month == m.Month);
                return new MonthlyBookingTrendItem
                {
                    Label = m.ToString("MMM"),
                    BookingCount = match?.Count ?? 0
                };
            })
            .ToList();

        var totalEvents = await _db.Events.AsNoTracking().CountAsync();
        var typeNameById = await _db.EventTypes
            .AsNoTracking()
            .ToDictionaryAsync(x => x.EventTypeId, x => x.EventTypeName);

        var eventTypeCountsRaw = await _db.Events
            .AsNoTracking()
            .GroupBy(e => e.EventTypeId)
            .Select(g => new
            {
                EventTypeId = g.Key,
                Count = g.Count()
            })
            .ToListAsync();

        var eventTypeBreakdown = eventTypeCountsRaw
            .Select(x =>
            {
                var name = x.EventTypeId.HasValue && typeNameById.TryGetValue(x.EventTypeId.Value, out var eventTypeName)
                    ? eventTypeName
                    : "Unclassified";
                var percentage = totalEvents == 0
                    ? 0
                    : (int)Math.Round((double)x.Count * 100 / totalEvents);

                return new EventTypeBreakdownItem
                {
                    Name = name,
                    Count = x.Count,
                    Percentage = percentage
                };
            })
            .OrderByDescending(x => x.Count)
            .ToList();

        var upcomingEvents = await _db.Events
            .AsNoTracking()
            .Where(e => e.EventDate >= today)
            .OrderBy(e => e.EventDate)
            .ThenBy(e => e.EventName)
            .Select(e => new UpcomingEventRow
            {
                EventName = e.EventName,
                EventDate = e.EventDate,
                Location = e.Venue != null ? e.Venue.Location : "Unassigned",
                BookingCount = e.Bookings.Count
            })
            .Take(6)
            .ToListAsync();

        var topEvents = await _db.Events
            .AsNoTracking()
            .OrderByDescending(e => e.Bookings.Count)
            .ThenByDescending(e => e.EventDate)
            .Select(e => new TopEventRow
            {
                EventName = e.EventName,
                EventDate = e.EventDate,
                BookingCount = e.Bookings.Count,
                EventTypeName = e.EventType != null ? e.EventType.EventTypeName : "Unclassified"
            })
            .Take(5)
            .ToListAsync();

        var model = new DashboardViewModel
        {
            TotalEvents = totalEvents,
            UpcomingEventsCount = await _db.Events.AsNoTracking().CountAsync(e => e.EventDate >= today),
            CompletedEventsCount = await _db.Events.AsNoTracking().CountAsync(e => e.EventDate < today),
            TotalBookings = await _db.Bookings.AsNoTracking().CountAsync(),
            UnavailableVenuesCount = await _db.Venues.AsNoTracking().CountAsync(v => !v.IsAvailable),
            UnclassifiedEventsCount = await _db.Events.AsNoTracking().CountAsync(e => e.EventTypeId == null),
            MonthlyBookingTrend = monthlyTrend,
            EventTypeBreakdown = eventTypeBreakdown,
            UpcomingEvents = upcomingEvents,
            TopEvents = topEvents
        };

        return View(model);
    }
}

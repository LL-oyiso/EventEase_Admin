using EventEase_WebApp.Data;
using EventEase_WebApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace EventEase_WebApp.Controllers;

public class EventsController : Controller
{
    private readonly EventEaseDbContext _db;

    public EventsController(EventEaseDbContext db)
    {
        _db = db;
    }

    
    public async Task<IActionResult> Index()
    {
        var eventsList = await _db.Events
            .AsNoTracking()
            .Include(e => e.Venue)
            .OrderByDescending(e => e.EventDate)
            .ThenBy(e => e.EventName)
            .ToListAsync();

        return View(eventsList);
    }

  
    public async Task<IActionResult> Details(int? id)
    {
        if (id is null) return NotFound();

        var ev = await _db.Events
            .AsNoTracking()
            .Include(e => e.Venue)
            .FirstOrDefaultAsync(e => e.EventId == id.Value);

        if (ev is null) return NotFound();
        return View(ev);
    }

    public async Task<IActionResult> Create()
    {
        await PopulateVenuesDropDownList(null);
        return View(new Event { EventDate = DateTime.Today });
    }

   
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Event ev)
    {
        if (!ModelState.IsValid)
        {
            await PopulateVenuesDropDownList(ev.VenueId);
            return View(ev);
        }

        _db.Events.Add(ev);
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

   
    public async Task<IActionResult> Edit(int? id)
    {
        if (id is null) return NotFound();

        var ev = await _db.Events.FindAsync(id.Value);
        if (ev is null) return NotFound();

        await PopulateVenuesDropDownList(ev.VenueId);
        return View(ev);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Event ev)
    {
        if (id != ev.EventId) return NotFound();

        if (!ModelState.IsValid)
        {
            await PopulateVenuesDropDownList(ev.VenueId);
            return View(ev);
        }

        try
        {
            _db.Update(ev);
            await _db.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            var exists = await _db.Events.AnyAsync(e => e.EventId == ev.EventId);
            if (!exists) return NotFound();
            throw;
        }

        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(int? id)
    {
        if (id is null) return NotFound();

        var ev = await _db.Events
            .AsNoTracking()
            .Include(e => e.Venue)
            .FirstOrDefaultAsync(e => e.EventId == id.Value);

        if (ev is null) return NotFound();

        var hasBookings = await _db.Bookings.AnyAsync(b => b.EventId == id.Value);
        ViewBag.HasBookings = hasBookings;

        return View(ev);
    }


    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var ev = await _db.Events.FindAsync(id);
        if (ev is null) return RedirectToAction(nameof(Index));

        var hasBookings = await _db.Bookings.AnyAsync(b => b.EventId == id);
        if (hasBookings)
        {
            TempData["Error"] = "Cannot delete this event because it is linked to one or more bookings.";
            return RedirectToAction(nameof(Delete), new { id });
        }

        _db.Events.Remove(ev);
        await _db.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }

    private async Task PopulateVenuesDropDownList(int? selectedVenueId)
    {
        var venues = await _db.Venues
            .AsNoTracking()
            .OrderBy(v => v.VenueName)
            .ToListAsync();

        ViewBag.VenueId = new SelectList(venues, nameof(Venue.VenueId), nameof(Venue.VenueName), selectedVenueId);
    }
}
using EventEase_WebApp.Data;
using EventEase_WebApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace EventEase_WebApp.Controllers;

public class BookingsController : Controller
{
    private readonly EventEaseDbContext _db;

    public BookingsController(EventEaseDbContext db)
    {
        _db = db;
    }


    public async Task<IActionResult> Index()
    {
        var bookings = await _db.Bookings
            .AsNoTracking()
            .Include(b => b.Venue)
            .Include(b => b.Event)
            .OrderByDescending(b => b.BookingDate)
            .ToListAsync();

        return View(bookings);
    }

    // Shows one booking reference.
    public async Task<IActionResult> Details(Guid? id)
    {
        if (id is null) return NotFound();

        var booking = await _db.Bookings
            .AsNoTracking()
            .Include(b => b.Venue)
            .Include(b => b.Event)
            .FirstOrDefaultAsync(b => b.BookingId == id.Value);

        if (booking is null) return NotFound();
        return View(booking);
    }

  
    public async Task<IActionResult> Create()
    {
        await PopulateDropDowns(eventId: null, venueId: null);
        return View(new Booking { BookingDate = DateTime.Today });
    }

    
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Booking booking)
    {
        await ValidateBookingRules(booking);

        if (!ModelState.IsValid)
        {
            await PopulateDropDowns(booking.EventId, booking.VenueId);
            return View(booking);
        }

        _db.Bookings.Add(booking);

        try
        {
            await _db.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            // If two admins submit same time, the DB unique constraint may be hit.
            ModelState.AddModelError(nameof(Booking.BookingDate), "This venue is already booked for that date.");
            await PopulateDropDowns(booking.EventId, booking.VenueId);
            return View(booking);
        }

        return RedirectToAction(nameof(Index));
    }

    
    public async Task<IActionResult> Edit(Guid? id)
    {
        if (id is null) return NotFound();

        var booking = await _db.Bookings.FindAsync(id.Value);
        if (booking is null) return NotFound();

        await PopulateDropDowns(booking.EventId, booking.VenueId);
        return View(booking);
    }

    
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, Booking booking)
    {
        if (id != booking.BookingId) return NotFound();

        await ValidateBookingRules(booking);

        if (!ModelState.IsValid)
        {
            await PopulateDropDowns(booking.EventId, booking.VenueId);
            return View(booking);
        }

        try
        {
            _db.Update(booking);
            await _db.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            var exists = await _db.Bookings.AnyAsync(b => b.BookingId == booking.BookingId);
            if (!exists) return NotFound();
            throw;
        }
        catch (DbUpdateException)
        {
            ModelState.AddModelError(nameof(Booking.BookingDate), "This venue is already booked for that date.");
            await PopulateDropDowns(booking.EventId, booking.VenueId);
            return View(booking);
        }

        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(Guid? id)
    {
        if (id is null) return NotFound();

        var booking = await _db.Bookings
            .AsNoTracking()
            .Include(b => b.Venue)
            .Include(b => b.Event)
            .FirstOrDefaultAsync(b => b.BookingId == id.Value);

        if (booking is null) return NotFound();
        return View(booking);
    }

    
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(Guid id)
    {
        var booking = await _db.Bookings.FindAsync(id);
        if (booking is null) return RedirectToAction(nameof(Index));

        _db.Bookings.Remove(booking);
        await _db.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }

    private async Task PopulateDropDowns(int? eventId, int? venueId)
    {
        var eventsList = await _db.Events
            .AsNoTracking()
            .OrderByDescending(e => e.EventDate)
            .ThenBy(e => e.EventName)
            .ToListAsync();

        var venuesList = await _db.Venues
            .AsNoTracking()
            .OrderBy(v => v.VenueName)
            .ToListAsync();

        ViewBag.EventId = new SelectList(eventsList, nameof(Event.EventId), nameof(Event.EventName), eventId);
        ViewBag.VenueId = new SelectList(venuesList, nameof(Venue.VenueId), nameof(Venue.VenueName), venueId);
    }

    private async Task ValidateBookingRules(Booking booking)
    {
        var bookingDate = booking.BookingDate.Date;

        //Requirements says venue cannot be double booked for the same date.
        var exists = await _db.Bookings
            .AsNoTracking()
            .AnyAsync(b =>
                b.VenueId == booking.VenueId &&
                b.BookingDate == bookingDate &&
                b.BookingId != booking.BookingId);

        if (exists)
        {
            ModelState.AddModelError(nameof(Booking.BookingDate), "This venue is already booked for that date.");
        }

        
        var evVenueId = await _db.Events
            .AsNoTracking()
            .Where(e => e.EventId == booking.EventId)
            .Select(e => e.VenueId)
            .FirstOrDefaultAsync();

        if (evVenueId.HasValue && evVenueId.Value != booking.VenueId)
        {
            ModelState.AddModelError(nameof(Booking.VenueId),
                "This event is assigned to a different venue. Update the event or select the matching venue.");
        }
    }
}
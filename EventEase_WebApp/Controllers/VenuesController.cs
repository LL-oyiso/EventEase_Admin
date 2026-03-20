using EventEase_WebApp.Data;
using EventEase_WebApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EventEase_WebApp.Controllers;

public class VenuesController : Controller
{
    private readonly EventEaseDbContext _db;

    public VenuesController(EventEaseDbContext db)
    {
        _db = db;
    }

    
    public async Task<IActionResult> Index()
    {
        var venues = await _db.Venues
            .AsNoTracking()
            .OrderBy(v => v.VenueName)
            .ToListAsync();

        return View(venues);
    }

    // Lets Shows one venue.
    public async Task<IActionResult> Details(int? id)
    {
        if (id is null) return NotFound();

        var venue = await _db.Venues
            .AsNoTracking()
            .FirstOrDefaultAsync(v => v.VenueId == id.Value);

        if (venue is null) return NotFound();
        return View(venue);
    }

    
   
    public IActionResult Create()
    {
        return View(new Venue
        {
            ImageUrl = "https://placehold.co/600x400?text=Venue"
        });
    }

  
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Venue venue)
    {
        if (!ModelState.IsValid) return View(venue);

        _db.Venues.Add(venue);
        await _db.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }

   
    public async Task<IActionResult> Edit(int? id)
    {
        if (id is null) return NotFound();

        var venue = await _db.Venues.FindAsync(id.Value);
        if (venue is null) return NotFound();

        return View(venue);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Venue venue)
    {
        if (id != venue.VenueId) return NotFound();
        if (!ModelState.IsValid) return View(venue);

        try
        {
            _db.Update(venue);
            await _db.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            var exists = await _db.Venues.AnyAsync(v => v.VenueId == venue.VenueId);
            if (!exists) return NotFound();
            throw;
        }

        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(int? id)
    {
        if (id is null) return NotFound();

        var venue = await _db.Venues
            .AsNoTracking()
            .FirstOrDefaultAsync(v => v.VenueId == id.Value);

        if (venue is null) return NotFound();

        var hasBookings = await _db.Bookings.AnyAsync(b => b.VenueId == id.Value);
        ViewBag.HasBookings = hasBookings;

        return View(venue);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var venue = await _db.Venues.FindAsync(id);
        if (venue is null) return RedirectToAction(nameof(Index));

        var hasBookings = await _db.Bookings.AnyAsync(b => b.VenueId == id);
        if (hasBookings)
        {
            TempData["Error"] = "Cannot delete this venue because it is linked to one or more bookings.";
            return RedirectToAction(nameof(Delete), new { id });
        }

        _db.Venues.Remove(venue);
        await _db.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }
}
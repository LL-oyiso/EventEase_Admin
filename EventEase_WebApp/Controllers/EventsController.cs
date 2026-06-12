using EventEase_WebApp.Data;
using EventEase_WebApp.Models;
using EventEase_WebApp.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace EventEase_WebApp.Controllers;

public class EventsController : Controller
{
    private readonly EventEaseDbContext _db;
    private readonly IBlobStorageService _blobStorageService;
    private static readonly string[] AllowedImageExtensions = [".jpg", ".jpeg", ".png", ".gif", ".webp"];
    private const long MaxImageBytes = 5 * 1024 * 1024;

    public EventsController(EventEaseDbContext db, IBlobStorageService blobStorageService)
    {
        _db = db;
        _blobStorageService = blobStorageService;
    }

    
    public async Task<IActionResult> Index()
    {
        var eventsList = await _db.Events
            .AsNoTracking()
            .Include(e => e.Venue)
            .Include(e => e.EventType)
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
            .Include(e => e.EventType)
            .FirstOrDefaultAsync(e => e.EventId == id.Value);

        if (ev is null) return NotFound();
        return View(ev);
    }

    public async Task<IActionResult> Create()
    {
        await PopulateVenuesDropDownList(null);
        await PopulateEventTypesDropDownList(null);
        return View(new Event { EventDate = DateTime.Today });
    }

   
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Event ev)
    {
        ValidateImageFile(ev.ImageFile, isCreate: true);
        if (!ModelState.IsValid)
        {
            await PopulateVenuesDropDownList(ev.VenueId);
            await PopulateEventTypesDropDownList(ev.EventTypeId);
            return View(ev);
        }

        try
        {
            ev.ImageUrl = await _blobStorageService.UploadImageAsync(ev.ImageFile!, "events");
            _db.Events.Add(ev);
            await _db.SaveChangesAsync();
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            await PopulateVenuesDropDownList(ev.VenueId);
            await PopulateEventTypesDropDownList(ev.EventTypeId);
            return View(ev);
        }

        TempData["Success"] = "Event created.";
        return RedirectToAction(nameof(Index));
    }

   
    public async Task<IActionResult> Edit(int? id)
    {
        if (id is null) return NotFound();

        var ev = await _db.Events.FindAsync(id.Value);
        if (ev is null) return NotFound();

        await PopulateVenuesDropDownList(ev.VenueId);
        await PopulateEventTypesDropDownList(ev.EventTypeId);
        return View(ev);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Event ev)
    {
        if (id != ev.EventId) return NotFound();
        ValidateImageFile(ev.ImageFile, isCreate: false);

        if (!ModelState.IsValid)
        {
            await PopulateVenuesDropDownList(ev.VenueId);
            await PopulateEventTypesDropDownList(ev.EventTypeId);

            return View(ev);
        }

        var existingEvent = await _db.Events.FindAsync(id);
        if (existingEvent is null) return NotFound();

        string? oldImageUrl = null;

        try
        {
            if (ev.ImageFile is not null)
            {
                oldImageUrl = existingEvent.ImageUrl;
                existingEvent.ImageUrl = await _blobStorageService.UploadImageAsync(ev.ImageFile, "events");
            }

            existingEvent.EventName = ev.EventName;
            existingEvent.EventDate = ev.EventDate;
            existingEvent.Description = ev.Description;
            existingEvent.VenueId = ev.VenueId;
            existingEvent.EventTypeId = ev.EventTypeId;
            await _db.SaveChangesAsync();

            if (!string.IsNullOrWhiteSpace(oldImageUrl))
            {
                await _blobStorageService.DeleteImageIfExistsAsync(oldImageUrl);
            }
        }
        catch (DbUpdateConcurrencyException)
        {
            var exists = await _db.Events.AnyAsync(e => e.EventId == ev.EventId);
            if (!exists) return NotFound();
            throw;
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            ev.ImageUrl = existingEvent.ImageUrl;
            await PopulateVenuesDropDownList(ev.VenueId);
            await PopulateEventTypesDropDownList(ev.EventTypeId);
            return View(ev);
        }

        TempData["Success"] = "Event updated.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(int? id)
    {
        if (id is null) return NotFound();

        var ev = await _db.Events
            .AsNoTracking()
            .Include(e => e.Venue)
            .Include(e => e.EventType)
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

        var imageUrl = ev.ImageUrl;
        _db.Events.Remove(ev);
        await _db.SaveChangesAsync();
        await _blobStorageService.DeleteImageIfExistsAsync(imageUrl);

        TempData["Success"] = "Event deleted.";
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

    private async Task PopulateEventTypesDropDownList(int? selectedEventTypeId)
    {
        var eventTypes = await _db.EventTypes
            .AsNoTracking()
            .OrderBy(et => et.EventTypeName)
            .ToListAsync();

        ViewBag.EventTypeId = new SelectList(eventTypes, nameof(EventType.EventTypeId), nameof(EventType.EventTypeName), selectedEventTypeId);
    }

    private void ValidateImageFile(IFormFile? imageFile, bool isCreate)
    {
        if (imageFile is null)
        {
            if (isCreate)
            {
                ModelState.AddModelError(nameof(Event.ImageFile), "Please select an image file.");
            }

            return;
        }

        var extension = Path.GetExtension(imageFile.FileName);
        if (string.IsNullOrWhiteSpace(extension) || !AllowedImageExtensions.Contains(extension, StringComparer.OrdinalIgnoreCase))
        {
            ModelState.AddModelError(nameof(Event.ImageFile), "Only JPG, PNG, GIF, or WEBP files are allowed.");
        }

        if (imageFile.Length > MaxImageBytes)
        {
            ModelState.AddModelError(nameof(Event.ImageFile), "Image size must be 5 MB or smaller.");
        }
    }
}
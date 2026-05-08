using EventEase_WebApp.Data;
using EventEase_WebApp.Models;
using EventEase_WebApp.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EventEase_WebApp.Controllers;

public class VenuesController : Controller
{
    private readonly EventEaseDbContext _db;
    private readonly IBlobStorageService _blobStorageService;
    private static readonly string[] AllowedImageExtensions = [".jpg", ".jpeg", ".png", ".gif", ".webp"];
    private const long MaxImageBytes = 5 * 1024 * 1024;

    public VenuesController(EventEaseDbContext db, IBlobStorageService blobStorageService)
    {
        _db = db;
        _blobStorageService = blobStorageService;
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
        ValidateImageFile(venue.ImageFile, isCreate: true);
        if (!ModelState.IsValid) return View(venue);

        try
        {
            venue.ImageUrl = await _blobStorageService.UploadImageAsync(venue.ImageFile!, "venues");
            _db.Venues.Add(venue);
            await _db.SaveChangesAsync();
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(venue);
        }

        TempData["Success"] = "Venue created.";
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
        ValidateImageFile(venue.ImageFile, isCreate: false);
        if (!ModelState.IsValid) return View(venue);

        var existingVenue = await _db.Venues.FindAsync(id);
        if (existingVenue is null) return NotFound();

        string? oldImageUrl = null;

        try
        {
            if (venue.ImageFile is not null)
            {
                oldImageUrl = existingVenue.ImageUrl;
                existingVenue.ImageUrl = await _blobStorageService.UploadImageAsync(venue.ImageFile, "venues");
            }

            existingVenue.VenueName = venue.VenueName;
            existingVenue.Location = venue.Location;
            existingVenue.Capacity = venue.Capacity;
            await _db.SaveChangesAsync();

            if (!string.IsNullOrWhiteSpace(oldImageUrl))
            {
                await _blobStorageService.DeleteImageIfExistsAsync(oldImageUrl);
            }
        }
        catch (DbUpdateConcurrencyException)
        {
            var exists = await _db.Venues.AnyAsync(v => v.VenueId == venue.VenueId);
            if (!exists) return NotFound();
            throw;
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            venue.ImageUrl = existingVenue.ImageUrl;
            return View(venue);
        }

        TempData["Success"] = "Venue updated.";
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

        var imageUrl = venue.ImageUrl;
        _db.Venues.Remove(venue);
        await _db.SaveChangesAsync();
        await _blobStorageService.DeleteImageIfExistsAsync(imageUrl);

        TempData["Success"] = "Venue deleted.";
        return RedirectToAction(nameof(Index));
    }

    private void ValidateImageFile(IFormFile? imageFile, bool isCreate)
    {
        if (imageFile is null)
        {
            if (isCreate)
            {
                ModelState.AddModelError(nameof(Venue.ImageFile), "Please select an image file.");
            }

            return;
        }

        var extension = Path.GetExtension(imageFile.FileName);
        if (string.IsNullOrWhiteSpace(extension) || !AllowedImageExtensions.Contains(extension, StringComparer.OrdinalIgnoreCase))
        {
            ModelState.AddModelError(nameof(Venue.ImageFile), "Only JPG, PNG, GIF, or WEBP files are allowed.");
        }

        if (imageFile.Length > MaxImageBytes)
        {
            ModelState.AddModelError(nameof(Venue.ImageFile), "Image size must be 5 MB or smaller.");
        }
    }
}
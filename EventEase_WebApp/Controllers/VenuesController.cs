using EventEase_WebApp.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EventEase_WebApp.Controllers;

public class VenuesController : Controller
{
    private readonly EventEaseDbContext _db;
    public VenuesController(EventEaseDbContext db) => _db = db;

    // GET: /Venues
    public async Task<IActionResult> Index()
    {
        var venues = await _db.Venues.AsNoTracking().ToListAsync();
        return View(venues);
    }
}
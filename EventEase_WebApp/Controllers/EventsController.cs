using EventEase_WebApp.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EventEase_WebApp.Controllers;

public class EventsController : Controller
{
    private readonly EventEaseDbContext _db;
    public EventsController(EventEaseDbContext db) => _db = db;

    // GET: /Events
    public async Task<IActionResult> Index()
    {
        var eventsList = await _db.Events.AsNoTracking().ToListAsync();
        return View(eventsList);
    }
}
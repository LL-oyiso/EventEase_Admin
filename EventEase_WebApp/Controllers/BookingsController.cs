using EventEase_WebApp.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EventEase_WebApp.Controllers;

public class BookingsController : Controller
{
    private readonly EventEaseDbContext _db;
    public BookingsController(EventEaseDbContext db) => _db = db;

    // GET: /Bookings
    public async Task<IActionResult> Index()
    {
        var bookings = await _db.Bookings.AsNoTracking().ToListAsync();
        return View(bookings);
    }
}
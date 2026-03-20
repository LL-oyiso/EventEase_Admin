
using EventEase_WebApp.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// MVC (Controllers + Views)
builder.Services.AddControllersWithViews();

// EF Core (SQL Server / LocalDB)
builder.Services.AddDbContext<EventEaseDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("EventEaseDb")));

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

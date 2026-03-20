using EventEase_WebApp.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<EventEaseDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("EventEaseDb")));

var app = builder.Build();

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.MapDefaultControllerRoute(); // enables /Venues -> VenuesController.Index()

app.Run();
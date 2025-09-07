using Microsoft.EntityFrameworkCore;
using RollCall.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Add DbContext with PostgreSQL
builder.Services.AddDbContext<RollCallDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// ------------------- Session Configuration -------------------
builder.Services.AddDistributedMemoryCache(); // Stores session in memory
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // Session timeout
    options.Cookie.HttpOnly = true;                 // Security
    options.Cookie.IsEssential = true;              // Required for GDPR compliance
});
// ------------------------------------------------------------

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// ------------------- Enable Session -------------------
app.UseSession(); // Must come before UseAuthorization
// ------------------------------------------------------

app.UseAuthorization();

// Map default route to User controller
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
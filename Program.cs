using FirstBloom.Data;
using FirstBloom.Models.Identity;
using FirstBloom.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// =====================================================
// CONNECTION STRING
// =====================================================

var connectionString = builder.Configuration
    .GetConnectionString("DefaultConnection");

if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException(
        "DefaultConnection was not found in appsettings.json.");
}


// =====================================================
// DATABASE
// =====================================================

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    if (connectionString.Contains(
            "Data Source=",
            StringComparison.OrdinalIgnoreCase) &&
        connectionString.EndsWith(
            ".db",
            StringComparison.OrdinalIgnoreCase))
    {
        // SQLite
        options.UseSqlite(connectionString);
    }
    else
    {
        // SQL Server
        options.UseSqlServer(connectionString);
    }
});


// =====================================================
// IDENTITY
// =====================================================

builder.Services
    .AddIdentity<ApplicationUser, IdentityRole>(options =>
    {
        // Email must be verified before login
        options.SignIn.RequireConfirmedEmail = true;

        // Password rules
        options.Password.RequireDigit = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireUppercase = true;
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequiredLength = 6;

        // User rules
        options.User.RequireUniqueEmail = true;

        // Lockout
        options.Lockout.DefaultLockoutTimeSpan =
            TimeSpan.FromMinutes(15);

        options.Lockout.MaxFailedAccessAttempts = 5;

        options.Lockout.AllowedForNewUsers = true;
    })
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();


// =====================================================
// ADMIN LOGIN / ACCESS DENIED URL
// =====================================================

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Admin/Account/Login";

    options.AccessDeniedPath =
        "/Admin/Account/AccessDenied";

    options.ExpireTimeSpan =
        TimeSpan.FromMinutes(60);

    options.SlidingExpiration = true;
});


// =====================================================
// EMAIL SERVICE
// =====================================================

builder.Services.AddScoped<EmailService>();


// =====================================================
// MVC
// =====================================================

builder.Services.AddControllersWithViews();


// =====================================================
// BUILD APPLICATION
// =====================================================

var app = builder.Build();


// =====================================================
// ERROR HANDLING
// =====================================================

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");

    app.UseHsts();
}


// =====================================================
// MIDDLEWARE
// =====================================================

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();


// IMPORTANT:
// Authentication must come before Authorization

app.UseAuthentication();

app.UseAuthorization();


// =====================================================
// ADMIN AREA ROUTE
// =====================================================

app.MapControllerRoute(
    name: "areas",
    pattern:
        "{area:exists}/{controller=Dashboard}/{action=Index}/{id?}"
);


// =====================================================
// DEFAULT ROUTE
// =====================================================

app.MapControllerRoute(
    name: "default",
    pattern:
        "{controller=Home}/{action=Index}/{id?}"
);


// =====================================================
// RUN APPLICATION
// =====================================================

app.Run();
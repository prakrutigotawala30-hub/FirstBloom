using FirstBloom.Data;
using FirstBloom.Models.Identity;
using FirstBloom.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);


// =====================================================
// CONNECTION STRING
// =====================================================

var connectionString =
    builder.Configuration.GetConnectionString(
        "DefaultConnection");

if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException(
        "DefaultConnection was not found in appsettings.json.");
}


// =====================================================
// DATABASE
// SQL SERVER OR SQLITE
// =====================================================

builder.Services.AddDbContext<ApplicationDbContext>(
    options =>
    {
        if (
            connectionString.Contains(
                "Data Source=",
                StringComparison.OrdinalIgnoreCase)
            &&
            connectionString.EndsWith(
                ".db",
                StringComparison.OrdinalIgnoreCase)
        )
        {
            options.UseSqlite(connectionString);
        }
        else
        {
            options.UseSqlServer(connectionString);
        }
    });


// =====================================================
// IDENTITY
// =====================================================

builder.Services
    .AddIdentity<ApplicationUser, IdentityRole>(
        options =>
        {
            // -------------------------------------------------
            // SIGN IN
            // -------------------------------------------------

            options.SignIn.RequireConfirmedEmail = true;


            // -------------------------------------------------
            // PASSWORD
            // -------------------------------------------------

            options.Password.RequiredLength = 6;

            options.Password.RequireDigit = true;

            options.Password.RequireLowercase = true;

            options.Password.RequireUppercase = true;

            options.Password.RequireNonAlphanumeric = false;


            // -------------------------------------------------
            // USER
            // -------------------------------------------------

            options.User.RequireUniqueEmail = true;


            // -------------------------------------------------
            // LOCKOUT
            // -------------------------------------------------

            options.Lockout.DefaultLockoutTimeSpan =
                TimeSpan.FromMinutes(15);

            options.Lockout.MaxFailedAccessAttempts = 5;

            options.Lockout.AllowedForNewUsers = true;
        })
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();


// =====================================================
// APPLICATION COOKIE
// =====================================================

builder.Services.ConfigureApplicationCookie(
    options =>
    {
        options.LoginPath =
            "/StudentAccount/Login";

        options.AccessDeniedPath =
            "/StudentAccount/AccessDenied";

        options.ExpireTimeSpan =
            TimeSpan.FromMinutes(30);

        options.SlidingExpiration = true;

        options.Cookie.HttpOnly = true;

        options.Cookie.IsEssential = true;
    });


// =====================================================
// SESSION
// =====================================================

builder.Services.AddDistributedMemoryCache();

builder.Services.AddSession(
    options =>
    {
        options.IdleTimeout =
            TimeSpan.FromMinutes(30);

        options.Cookie.HttpOnly = true;

        options.Cookie.IsEssential = true;
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
// BUILD
// =====================================================

var app = builder.Build();


// =====================================================
// CREATE DEFAULT ROLES
// =====================================================

using (var scope = app.Services.CreateScope())
{
    var roleManager =
        scope.ServiceProvider
            .GetRequiredService<
                RoleManager<IdentityRole>>();

    string[] requiredRoles =
    {
        "Admin",
        "Student"
    };


    foreach (var roleName in requiredRoles)
    {
        if (!await roleManager.RoleExistsAsync(roleName))
        {
            var result =
                await roleManager.CreateAsync(
                    new IdentityRole(roleName));

            if (!result.Succeeded)
            {
                var errors =
                    string.Join(
                        ", ",
                        result.Errors.Select(
                            error =>
                                error.Description));

                throw new InvalidOperationException(
                    $"Could not create role '{roleName}'. {errors}");
            }
        }
    }
}


// =====================================================
// ERROR HANDLING
// =====================================================

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler(
        "/Home/Error");

    app.UseHsts();
}


// =====================================================
// HTTPS
// =====================================================

app.UseHttpsRedirection();


// =====================================================
// STATIC FILES
// =====================================================

app.UseStaticFiles();


// =====================================================
// ROUTING
// =====================================================

app.UseRouting();


// =====================================================
// SESSION
// =====================================================

app.UseSession();


// =====================================================
// AUTHENTICATION
// =====================================================

app.UseAuthentication();


// =====================================================
// AUTHORIZATION
// =====================================================

app.UseAuthorization();


// =====================================================
// ADMIN AREA ROUTE
// =====================================================

app.MapControllerRoute(
    name: "areas",
    pattern:
        "{area:exists}/" +
        "{controller=Dashboard}/" +
        "{action=Index}/" +
        "{id?}");


// =====================================================
// DEFAULT WEBSITE ROUTE
// =====================================================

app.MapControllerRoute(
    name: "default",
    pattern:
        "{controller=Home}/" +
        "{action=Index}/" +
        "{id?}");


// =====================================================
// RUN
// =====================================================

app.Run();
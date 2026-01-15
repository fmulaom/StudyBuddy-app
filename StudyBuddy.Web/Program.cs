using Microsoft.EntityFrameworkCore;
using StudyBuddy.Web.Data;
using StudyBuddy.Web.Extensions;
using StudyBuddy.Web.Models;
using StudyBuddy.Web.Services.Interfaces;
using StudyBuddy.Web.Services.LearningGoalConfig;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddScoped<ILearningGoalProgressStrategy, DeadlineAwareProgressStrategy>();
builder.Services.AddScoped<ILearningGoalService, LearningGoalService>();
builder.Services.AddScoped<ILearningGoalFacade, LearningGoalFacade>();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sqlOptions => sqlOptions.EnableRetryOnFailure(
            maxRetryCount: 3,
            maxRetryDelay: TimeSpan.FromSeconds(5),
            errorNumbersToAdd: null)
    ));

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services
    .AddDefaultIdentity<ApplicationUser>(options =>
    {
        options.SignIn.RequireConfirmedAccount = false;
    })
    .AddEntityFrameworkStores<ApplicationDbContext>();

// ZAP FIX: Cookie security flags
builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.Cookie.SameSite = SameSiteMode.Strict;
});

builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();
builder.Services.AddStudyPartnerServices();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

// ZAP FIX: Security headers
app.Use(async (context, next) =>
{
    context.Response.Headers["Content-Security-Policy"] =
        "default-src 'self'; " +
        "script-src 'self'; " +
        "style-src 'self'; " +
        "img-src 'self' data:; " +
        "object-src 'none'; " +
        "base-uri 'self'; " +
        "frame-ancestors 'none';";

    context.Response.Headers["X-Frame-Options"] = "DENY";
    context.Response.Headers["X-Content-Type-Options"] = "nosniff";
    context.Response.Headers["Referrer-Policy"] = "no-referrer";
    context.Response.Headers["Permissions-Policy"] = "geolocation=(), camera=()";
    context.Response.Headers["Cache-Control"] =
        "no-store, no-cache, must-revalidate";

    await next();
});

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();

app.Run();

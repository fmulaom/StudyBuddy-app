using Microsoft.EntityFrameworkCore;
using StudyBuddy.Web.Data;
using StudyBuddy.Web.Extensions;
using StudyBuddy.Web.Models;
using StudyBuddy.Web.Services.Interfaces;
using StudyBuddy.Web.Services.LearningGoalConfig;


var builder = WebApplication.CreateBuilder(args);

// ---------------- DB & Identity ----------------


builder.Services.AddScoped<ILearningGoalProgressStrategy, DeadlineAwareProgressStrategy>();
builder.Services.AddScoped<ILearningGoalService, LearningGoalService>();
builder.Services.AddScoped<ILearningGoalFacade, LearningGoalFacade>();
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
                       ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services
    .AddDefaultIdentity<ApplicationUser>(options =>
    {
        options.SignIn.RequireConfirmedAccount = false;
    })
    .AddEntityFrameworkStores<ApplicationDbContext>();

// ---------------- MVC / Razor ----------------
builder.Services.AddScoped<ILearningGoalService, LearningGoalService>();
builder.Services.AddScoped<ILearningGoalProgressStrategy, DeadlineAwareProgressStrategy>();
builder.Services.AddControllersWithViews();
builder.Services.AddScoped<ILearningGoalProgressStrategy, DeadlineAwareProgressStrategy>();
builder.Services.AddRazorPages();

// ---------------- StudyBuddy modul (SOLID) ----------------
builder.Services.AddStudyPartnerServices(); 

var app = builder.Build();

// ---------------- Middleware pipeline ----------------

if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

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

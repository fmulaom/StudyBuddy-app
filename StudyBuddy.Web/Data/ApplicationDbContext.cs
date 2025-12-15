using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using StudyBuddy.Web.Models;

namespace StudyBuddy.Web.Data;

public class ApplicationDbContext : IdentityDbContext
{
    public DbSet<StudyTasks> StudyTasks { get; set; }

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }
}

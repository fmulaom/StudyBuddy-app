using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using StudyBuddy.Web.Models;

namespace StudyBuddy.Web.Data;

public class ApplicationDbContext : IdentityDbContext
{
    public DbSet<StudyTasks> StudyTasks { get; set; }
    public DbSet<StudyPartner> StudyPartners { get; set; }
    public DbSet<StudyGroup> StudyGroups { get; set; }
    public DbSet<StudyGroupMember> StudyGroupMembers { get; set; }
    public DbSet<StudySession> StudySessions { get; set; }
    public DbSet<StudyResource> StudyResources { get; set; }

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }
    public DbSet<LearningGoal> LearningGoals { get; set; }

    

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<StudyGroup>()
            .HasMany(g => g.Members)
            .WithOne(m => m.Group)
            .HasForeignKey(m => m.GroupId)
            .OnDelete(DeleteBehavior.Cascade); // ovo nie dobro

        builder.Entity<StudyGroup>()
            .HasMany(g => g.Sessions)
            .WithOne(s => s.Group)
            .HasForeignKey(s => s.GroupId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<StudyGroup>()
            .HasMany(g => g.Resources)
            .WithOne(r => r.Group)
            .HasForeignKey(r => r.GroupId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

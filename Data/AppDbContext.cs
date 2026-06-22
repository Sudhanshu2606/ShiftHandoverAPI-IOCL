using Microsoft.EntityFrameworkCore;
using ShiftHandoverAPI.Models;

namespace ShiftHandoverAPI.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<HandoverReport> HandoverReports { get; set; }
        public DbSet<TaskItem> Tasks { get; set; }
        public DbSet<Issue> Issues { get; set; }
        public DbSet<TaskHistory> TaskHistories { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure relationships
            modelBuilder.Entity<HandoverReport>()
                .HasMany(h => h.Tasks)
                .WithOne(t => t.Handover)
                .HasForeignKey(t => t.HandoverId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<HandoverReport>()
                .HasMany(h => h.Issues)
                .WithOne(i => i.Handover)
                .HasForeignKey(i => i.HandoverId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<TaskItem>()
                .HasOne(t => t.AssignedUser)
                .WithMany()
                .HasForeignKey(t => t.AssignedTo)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<TaskItem>()
                .HasOne(t => t.CreatedByUser)
                .WithMany()
                .HasForeignKey(t => t.CreatedBy)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
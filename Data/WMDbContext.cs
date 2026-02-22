using Microsoft.EntityFrameworkCore;
using WorkTicketManager.Models;


namespace WorkTicketManager.Data
{
    public class WMDbContext : DbContext
    {
        public WMDbContext(DbContextOptions<WMDbContext> options) : base(options) { }

        public DbSet<Department> Departments => Set<Department>();
        public DbSet<User> Users => Set<User>();
        public DbSet<Status> Statuses => Set<Status>();
        public DbSet<Priority> Priorities => Set<Priority>();
        public DbSet<Ticket> Tickets => Set<Ticket>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // =====================
            // SEED: STATUSES
            // =====================
            modelBuilder.Entity<Status>().HasData(
                new Status { Id = 1, Code = "NEW", Name = "New" },
                new Status { Id = 2, Code = "IN_PROGRESS", Name = "In Progress" },
                new Status { Id = 3, Code = "CLOSED", Name = "Closed" }
            );

            // =====================
            // RELATIONSHIPS
            // =====================

            modelBuilder.Entity<Department>()
                .HasMany(d => d.Users)
                .WithOne(u => u.Department!)
                .HasForeignKey(u => u.DepartmentId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<User>()
                .HasMany(u => u.Tickets)
                .WithOne(t => t.User!)
                .HasForeignKey(t => t.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Priority>()
                .HasMany(p => p.Tickets)
                .WithOne(t => t.Priority!)
                .HasForeignKey(t => t.PriorityId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Status>()
                .HasMany(s => s.Tickets)
                .WithOne(t => t.Status!)
                .HasForeignKey(t => t.StatusId)
                .OnDelete(DeleteBehavior.Restrict);

            // =====================
            // INDEXES (IMPORTANT)
            // =====================

            // Быстрые отчёты и фильтры
            modelBuilder.Entity<Ticket>()
                .HasIndex(t => t.CreatedAt);

            modelBuilder.Entity<Ticket>()
                .HasIndex(t => t.CompletedAt);

            modelBuilder.Entity<Ticket>()
                .HasIndex(t => t.StatusId);

            modelBuilder.Entity<Ticket>()
                .HasIndex(t => t.UserId);

            modelBuilder.Entity<User>()
                .HasIndex(u => u.DepartmentId);
        }
    }

}

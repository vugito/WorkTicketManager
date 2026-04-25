using Microsoft.EntityFrameworkCore;
using WorkTicketManager.Models;

namespace WorkTicketManager.Data
{
    public class WMDbContext : DbContext
    {
        public WMDbContext(DbContextOptions<WMDbContext> options) : base(options) { }

        public DbSet<Company> Companies => Set<Company>();
        public DbSet<Department> Departments => Set<Department>();
        public DbSet<Employee> Employees => Set<Employee>();
        public DbSet<AppUser> AppUsers => Set<AppUser>();
        public DbSet<Role> Roles => Set<Role>();
        public DbSet<Status> Statuses => Set<Status>();
        public DbSet<Priority> Priorities => Set<Priority>();
        public DbSet<Ticket> Tickets => Set<Ticket>();
        public DbSet<TicketComment> TicketComments => Set<TicketComment>();

        public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

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

            // Company → Departments
            modelBuilder.Entity<Department>()
                .HasOne(d => d.Company)
                .WithMany(c => c.Departments)
                .HasForeignKey(d => d.CompanyId)
                .OnDelete(DeleteBehavior.Restrict);

            // Department → Employees
            modelBuilder.Entity<Employee>()
                .HasOne(e => e.Department)
                .WithMany(d => d.Employees)
                .HasForeignKey(e => e.DepartmentId)
                .OnDelete(DeleteBehavior.Restrict);

            // Employee → AppUser (один к одному)
            modelBuilder.Entity<AppUser>()
                .HasOne(u => u.Employee)
                .WithOne(e => e.AppUser)
                .HasForeignKey<AppUser>(u => u.EmployeeId)
                .OnDelete(DeleteBehavior.SetNull);

            // AppUser → Company
            modelBuilder.Entity<AppUser>()
                .HasOne(u => u.Company)
                .WithMany(c => c.AppUsers)
                .HasForeignKey(u => u.CompanyId)
                .OnDelete(DeleteBehavior.Restrict);

            // AppUser → Role
            modelBuilder.Entity<AppUser>()
                .HasOne(u => u.Role)
                .WithMany(r => r.AppUsers)
                .HasForeignKey(u => u.RoleId)
                .OnDelete(DeleteBehavior.SetNull);

            // Role → Company
            modelBuilder.Entity<Role>()
                .HasOne(r => r.Company)
                .WithMany(c => c.Roles)
                .HasForeignKey(r => r.CompanyId)
                .OnDelete(DeleteBehavior.Cascade);

            // Ticket → Employee
            modelBuilder.Entity<Ticket>()
                .HasOne(t => t.Employee)
                .WithMany(e => e.Tickets)
                .HasForeignKey(t => t.EmployeeId)
                .OnDelete(DeleteBehavior.Restrict);

            // Ticket → Company
            modelBuilder.Entity<Ticket>()
                .HasOne(t => t.Company)
                .WithMany()
                .HasForeignKey(t => t.CompanyId)
                .OnDelete(DeleteBehavior.Restrict);

            // Ticket → Priority
            modelBuilder.Entity<Ticket>()
                .HasOne(t => t.Priority)
                .WithMany(p => p.Tickets)
                .HasForeignKey(t => t.PriorityId)
                .OnDelete(DeleteBehavior.SetNull);

            // Ticket → Status
            modelBuilder.Entity<Ticket>()
                .HasOne(t => t.Status)
                .WithMany(s => s.Tickets)
                .HasForeignKey(t => t.StatusId)
                .OnDelete(DeleteBehavior.Restrict);

            // TicketComment → Ticket
            modelBuilder.Entity<TicketComment>()
                .HasOne(c => c.Ticket)
                .WithMany(t => t.Comments)
                .HasForeignKey(c => c.TicketId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<RefreshToken>()
                .HasOne(r => r.AppUser)
                .WithMany(u => u.RefreshTokens)
                .HasForeignKey(r => r.AppUserId)
                .OnDelete(DeleteBehavior.Cascade);

            // =====================
            // INDEXES
            // =====================
            modelBuilder.Entity<Ticket>()
                .HasIndex(t => t.CreatedAt);

            modelBuilder.Entity<Ticket>()
                .HasIndex(t => t.CompletedAt);

            modelBuilder.Entity<Ticket>()
                .HasIndex(t => t.StatusId);

            modelBuilder.Entity<Ticket>()
                .HasIndex(t => t.EmployeeId);

            modelBuilder.Entity<Ticket>()
                .HasIndex(t => t.CompanyId);

            modelBuilder.Entity<Employee>()
                .HasIndex(e => e.DepartmentId);

            modelBuilder.Entity<AppUser>()
                .HasIndex(u => u.Username)
                .IsUnique();

            modelBuilder.Entity<AppUser>()
                .HasIndex(u => u.CompanyId);
        }
    }
}
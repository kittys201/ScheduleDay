
using BCrypt.Net;
using Microsoft.EntityFrameworkCore;
using ScheduleDay.Shared.Models;

namespace ScheduleDay.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users => Set<User>();
        public DbSet<TaskItem> TaskItems => Set<TaskItem>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Set table names
            modelBuilder.Entity<User>().ToTable("Users");
            modelBuilder.Entity<TaskItem>().ToTable("Tasks");

            // Sample data with static values to avoid migration errors
            modelBuilder.Entity<User>().HasData(
                new User
                {
                    ID = 1,
                    Name = "Usuario Admin",
                    Email = "admin@demo.com",
                    Password = "$2b$12$WTVyWIrDLsk3NYn.BjlGTezKmiAuIwsIBqPTrBAtnm1ie/rV5eGnO" // Static hash for "Admin123"
                },
                new User
                {
                    ID = 2,
                    Name = "Usuario Demo",
                    Email = "demo@demo.com",
                    Password = "$2b$12$vzx.bnx6QyIYtpRqlf2D6.ogzJ2FTwBweEeK9aLg.Hh3IPUiKDXci" // Static hash for "Demo123"
                }
            );

            modelBuilder.Entity<TaskItem>().HasData(
    new TaskItem
    {
        ID = 1,
        Name = "Tarea 1",
        Description = "Descripción de la tarea 1",
        Date = DateTime.SpecifyKind(new DateTime(2024, 2, 7), DateTimeKind.Utc), // Date in UTC format
        Status = "Pending",
        UserID = 1
    },
    new TaskItem
    {
        ID = 2,
        Name = "Tarea 2",
        Description = "Descripción de la tarea 2",
        Date = DateTime.SpecifyKind(new DateTime(2024, 2, 7), DateTimeKind.Utc), // Date in UTC format
        Status = "Pending",
        UserID = 2
    }
);

        }
    }
}

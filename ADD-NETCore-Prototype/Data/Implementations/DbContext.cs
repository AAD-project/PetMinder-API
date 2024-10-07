using Api.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Api.Data.Implementations
{
    public class PetMinderDbContext : IdentityDbContext<User>
    {
        public PetMinderDbContext(DbContextOptions<PetMinderDbContext> options)
            : base(options) { }

        public DbSet<Pet>? Pets { get; set; }
        public DbSet<TodoTask>? Tasks { get; set; }
        public DbSet<Reminder>? Reminders { get; set; }
        public DbSet<HealthData>? HealthDatas { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder); // Call base method to configure Identity entities

            // Configure User entity
            modelBuilder.Entity<User>().HasKey(u => u.Id); // Define primary key for User
            modelBuilder.Entity<User>().Property(u => u.Id).ValueGeneratedNever(); // Disable auto-generation for Id

            // User has many Pets, each Pet has one User (Owner), but no navigation in Pet
            modelBuilder
                .Entity<User>()
                .HasMany(u => u.Pets) // User has many Pets
                .WithOne() // No navigation property in Pet
                .HasForeignKey(p => p.OwnerId) // Pet points to User through OwnerId
                .IsRequired(true); // OwnerId is required

            // User has many Tasks, each Task has one User (Owner), but no navigation in Task
            modelBuilder
                .Entity<User>()
                .HasMany(u => u.Tasks) // User has many Tasks
                .WithOne() // No navigation property in Task
                .HasForeignKey(t => t.UserId) // Task points to User through UserId
                .IsRequired(true); // UserId is required

            // Configure Pet entity
            modelBuilder.Entity<Pet>().HasKey(p => p.Id); // Define primary key for Pet
            modelBuilder.Entity<Pet>().Property(p => p.Id).ValueGeneratedNever(); // Disable auto-generation for Id

            // Configure one-to-one relationship between Pet and HealthData
            modelBuilder
                .Entity<Pet>()
                .HasOne(p => p.HealthData) // Pet has one HealthData
                .WithOne() // HealthData has no navigation property to Pet
                .HasForeignKey<HealthData>(h => h.PetId) // Set the foreign key explicitly
                .OnDelete(DeleteBehavior.Cascade);

            // Configure TodoTask entity
            modelBuilder.Entity<TodoTask>().HasKey(t => t.Id); // Define primary key for Task
            modelBuilder.Entity<TodoTask>().Property(t => t.Id).ValueGeneratedNever(); // Disable auto-generation for Id

            // Configure Reminder entity
            modelBuilder.Entity<Reminder>().HasKey(r => r.Id); // Define primary key for Reminder
            modelBuilder.Entity<Reminder>().Property(r => r.Id).ValueGeneratedNever(); // Disable auto-generation for Id
            modelBuilder.Entity<Reminder>().Property(r => r.UserId).IsRequired(); // UserId is required
            modelBuilder.Entity<Reminder>().Property(r => r.ReminderDateTime).IsRequired(); // ReminderDateTime is required
            modelBuilder.Entity<Reminder>().Property(r => r.Title).IsRequired(); // Title is required
        }
    }
}

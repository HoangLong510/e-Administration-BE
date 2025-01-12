using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Server.Models;
using Server.Models.Enums;
using Server.Utils;

namespace Server.Data
{
    public class DatabaseContext : DbContext
    {
        public DatabaseContext(DbContextOptions options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Device> Devices { get; set; }
        public DbSet<Software> Softwares { get; set; }
        public DbSet<Lab> Labs { get; set; }
        public DbSet<Class> Classes { get; set; }
        public DbSet<Department> Departments { get; set; }
        public DbSet<Schedule> Schedules { get; set; }
        public DbSet<Document> Documents { get; set; }
        public DbSet<Report> Reports { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<Tasks> Tasks { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<EmailModel> Emails { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Tasks>()
                .HasOne<Report>()
                .WithMany(c => c.Tasks)
                .HasForeignKey(u => u.ReportId)
                .OnDelete(DeleteBehavior.SetNull)
                .IsRequired(false);

            modelBuilder.Entity<User>()
                .HasOne<Class>()
                .WithMany(c => c.Users)
                .HasForeignKey(u => u.ClassId)
                .OnDelete(DeleteBehavior.SetNull)
                .IsRequired(false);

            modelBuilder.Entity<User>()
                .HasOne<Department>()
                .WithMany(c => c.Users)
                .HasForeignKey(u => u.DepartmentId)
                .OnDelete(DeleteBehavior.SetNull)
                .IsRequired(false);

            modelBuilder.Entity<Comment>()
                .HasOne(c => c.Report)
                .WithMany(r => r.Comments)
                .HasForeignKey(c => c.ReportId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Comment>()
                .HasOne(c => c.User)
                .WithMany()
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Tasks>()
                .HasOne(t => t.Assignees)
                .WithMany()
                .HasForeignKey(t => t.AssigneesId);

            // Configures the "Role" property of the "User" entity similarly, ensuring that the UserRole enum is stored as a string in the database.
            modelBuilder.Entity<User>()
                .Property(u => u.Role)
                .HasConversion(new EnumToStringConverter<UserRole>());

            // Configures the "Gender" property of the "User" entity similarly, ensuring that the UserGender enum is stored as a string in the database.
            modelBuilder.Entity<User>()
                .Property(u => u.Gender)
                .HasConversion(new EnumToStringConverter<UserGender>());
                
            // Configures the "Status" property of the "Task" entity similarly, ensuring that the TaskStatusEnum enum is stored as a string in the database.
            modelBuilder.Entity<Tasks>()
                .Property(t => t.Status)
                .HasConversion(new EnumToStringConverter<TaskStatusEnum>());
                
            // Thiết lập khóa ngoại cho Device và Software liên kết với Lab
            modelBuilder.Entity<Device>()
                .HasOne<Lab>(d => d.Lab)
                .WithMany(l => l.Devices)
                .HasForeignKey(d => d.LabId);

            modelBuilder.Entity<Software>()
                .HasOne<Lab>(s => s.Lab)
                .WithMany(l => l.Softwares)
                .HasForeignKey(s => s.LabId);

            // Seeding data for User table
            modelBuilder.Entity<User>().HasData(
               new User
               {
                   Id = 1,
                   Username = "admin",
                   Password = PasswordHasher.HashPassword("123456"),
                   FullName = "Administrator",
                   Role = UserRole.Admin
               }
            );

        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.ConfigureWarnings(warnings =>
                   warnings.Ignore(RelationalEventId.PendingModelChangesWarning));
        }
    }
}

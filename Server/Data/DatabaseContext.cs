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

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configures the "Role" property of the "User" entity similarly, ensuring that the UserRole enum is stored as a string in the database.
            modelBuilder.Entity<User>()
                .Property(u => u.Role)
                .HasConversion(new EnumToStringConverter<UserRole>());

            // Configures the "Gender" property of the "User" entity similarly, ensuring that the UserGender enum is stored as a string in the database.
            modelBuilder.Entity<User>()
                .Property(u => u.Gender)
                .HasConversion(new EnumToStringConverter<UserGender>());

            // Thiết lập khóa ngoại cho Device và Software liên kết với Lab
            modelBuilder.Entity<Device>()
                .HasOne<Lab>(d => d.Lab)
                .WithMany(l => l.Devices)
                .HasForeignKey(d => d.LabId); // Thay đổi từ RoomId thành LabID

            modelBuilder.Entity<Software>()
                .HasOne<Lab>(s => s.Lab)
                .WithMany(l => l.Softwares)
                .HasForeignKey(s => s.LabId); // Thay đổi từ RoomId thành LabID

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

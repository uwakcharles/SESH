using System.Data.Common;
using Microsoft.EntityFrameworkCore;
using SESH.Models;
using SESH.Models.Enums;

namespace SESH.Data
{
    public class ApplicationDbContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Student> Students { get; set; }
        public DbSet<PersonalSupervisor> PersonalSupervisors { get; set; }
        public DbSet<SeniorTutor> SeniorTutors { get; set; }
        public DbSet<WellBeingReport> WellBeingReports { get; set; }
        public DbSet<Meeting> Meetings { get; set; }
        public DbSet<AvailabilitySlot> AvailabilitySlots { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Data Source=SESHDatabase.db");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>()
                .HasDiscriminator<UserRole>("Role")
                .HasValue<Student>(UserRole.Student)
                .HasValue<PersonalSupervisor>(UserRole.PersonalSupervisor)
                .HasValue<SeniorTutor>(UserRole.SeniorTutor);

            modelBuilder.Entity<Meeting>()
                .HasOne(m => m.BookedBy)
                .WithMany(u => u.MeetingsBooked)
                .HasForeignKey(m => m.BookedById)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Meeting>()
                .HasOne(m => m.BookedWith)
                .WithMany(u => u.MeetinngsWith)
                .HasForeignKey(m => m.BookedWithId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Student>()
                .HasOne(s => s.PersonalSupervisor)
                .WithMany(ps => ps.AssignedStudents)
                .HasForeignKey(s => s.PersonalSupervisorId);

            modelBuilder.Entity<WellBeingReport>()
                .HasIndex(r => r.SubmittedAt);

            modelBuilder.Entity<Meeting>()
                .HasIndex(m => m.ScheduledAt);
        }
    }
}
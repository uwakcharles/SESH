using System;
using Microsoft.EntityFrameworkCore;
using SESH.Data;
using SESH.Models;
using SESH.Models.Enums;

namespace SESH.Tests
{
    public abstract class TestBase : IDisposable
    {
        protected readonly ApplicationDbContext _context;

        protected TestBase()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new ApplicationDbContext(options);
            SeedTestData();
        }

        private void SeedTestData()
        {
            var supervisor = new PersonalSupervisor
            {
                Id = 1,
                Name = "Dr. Test Supervisor",
                Email = "test.supervisor@hud.ac.uk",
                StaffId = "TEST001",
                Role = UserRole.PersonalSupervisor
            };
            supervisor.SetPassword("password123");
            _context.PersonalSupervisors.Add(supervisor);

            var student = new Student
            {
                Id = 2,
                Name = "Test Student",
                Email = "test.student@edu.hud.ac.uk",
                StudentId = "S12345",
                PersonalSupervisorId = 1,
                Role = UserRole.Student
            };
            student.SetPassword("student123");
            _context.Students.Add(student);

            var seniorTutor = new SeniorTutor
            {
                Id = 3,
                Name = "Prof. Test Tutor",
                Email = "test.tutor@hud.ac.uk",
                StaffId = "ST001",
                Role = UserRole.SeniorTutor
            };
            seniorTutor.SetPassword("tutor123");
            _context.SeniorTutors.Add(seniorTutor);

            var availabilitySlot = new AvailabilitySlot
            {
                Id = 1,
                PersonalSupervisorId = 1,
                StartTime = DateTime.UtcNow.AddDays(1),
                EndTime = DateTime.UtcNow.AddDays(1).AddHours(1),
                IsBooked = false
            };
            _context.AvailabilitySlots.Add(availabilitySlot);

            var report = new WellBeingReport
            {
                Id = 1,
                StudentId = 2,
                Status = ReportStatus.Okay,
                Notes = "Doing well",
                SubmittedAt = DateTime.UtcNow.AddDays(-1)
            };
            _context.WellBeingReports.Add(report);

            _context.SaveChanges();
        }

        public void Dispose()
        {
            _context?.Dispose();
        }
    }
}

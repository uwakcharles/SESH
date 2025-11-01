using SESH.Data;
using Microsoft.EntityFrameworkCore;
using SESH.Models;
using SESH.Models.Enums;
using SESH.Services;
using SESH.Services.Interfaces;
using SESH.Data;


namespace SESH.UI
{
    public class SeniorTutorMenu : MenuSystem
    {
        private readonly SeniorTutor _tutor;
        private readonly IReportService _reportService;
        private readonly IMeetingService _meetingService;
        private readonly AnalyticsService _analyticsService;
        private readonly IUserRegistrationService _registrationService;

        public SeniorTutorMenu(ApplicationDbContext context, IAuthService authService, SeniorTutor tutor,
                              IReportService reportService, IMeetingService meetingService,
                              AnalyticsService analyticsService, IUserRegistrationService registrationService)
            : base(context, authService)
        {
            _tutor = tutor;
            _reportService = reportService;
            _meetingService = meetingService;
            _analyticsService = analyticsService;
            _registrationService = registrationService;
        }

        public override async Task ShowMenuAsync()
        {
            while (true)
            {
                DisplayHeader($"Senior Tutor Portal - Welcome {_tutor.Name}");

                var options = new[]
                {
                    "View Cohort Analytics",
                    "View Supervisor Engagement",
                    "Manage Users",
                    "Change Password",
                    "Logout"
                };

                var choice = GetMenuChoice(options);

                switch (choice)
                {
                    case 1:
                        await ViewCohortAnalyticsAsync();
                        break;
                    case 2:
                        await ViewSupervisorEngagementAsync();
                        break;
                    case 3:
                        await ManageUsersAsync();
                        break;
                    case 4:
                        await ChangePasswordAsync();
                        break;
                    case 5:
                        return;
                }
            }
        }

        private async Task ViewCohortAnalyticsAsync()
        {
            DisplayHeader("Cohort Analytics Dashboard");

            var analytics = await _analyticsService.GetCohortAnalyticsAsync();

            Console.WriteLine($"Total Students: {analytics.TotalStudents}");
            Console.WriteLine($"Total Reports (Last 30 days): {analytics.TotalReports}");
            Console.WriteLine($"Completed Meetings (Last 30 days): {analytics.TotalMeetings}");
            Console.WriteLine($"Average Response Time: {analytics.AverageResponseTime} days\n");

            Console.WriteLine("Well-being Status Distribution:");
            foreach (var status in Enum.GetValues<ReportStatus>())
            {
                var count = analytics.StatusDistribution.GetValueOrDefault(status, 0);
                var percentage = analytics.TotalReports > 0 ? (count * 100.0 / analytics.TotalReports) : 0;
                Console.WriteLine($"  {status}: {count} ({percentage:F1}%)");
            }

            PressAnyKeyToContinue();
        }

        private async Task ViewSupervisorEngagementAsync()
        {
            DisplayHeader("Supervisor Engagement");

            var engagement = await _analyticsService.GetSupervisorEngagementAsync();

            if (!engagement.Any())
            {
                Console.WriteLine("No supervisor data available.");
                PressAnyKeyToContinue();
                return;
            }

            foreach (var supervisor in engagement)
            {
                Console.WriteLine($"• {supervisor.SupervisorName}");
                Console.WriteLine($"  Students: {supervisor.StudentCount}");
                Console.WriteLine($"  Meetings (30 days): {supervisor.MeetingsCount}");
                Console.WriteLine($"  Avg Response Time: {supervisor.AverageResponseTime} days");
                Console.WriteLine();
            }

            PressAnyKeyToContinue();
        }

        private async Task ManageUsersAsync()
        {
            while (true)
            {
                DisplayHeader("User Management");

                var options = new[]
                {
                    "Register New Student",
                    "Register New Personal Supervisor",
                    "Register New Senior Tutor",
                    "View All Users",
                    "Back to Main Menu"
                };

                var choice = GetMenuChoice(options);

                switch (choice)
                {
                    case 1:
                        await RegisterNewStudentAsync();
                        break;
                    case 2:
                        await RegisterNewPersonalSupervisorAsync();
                        break;
                    case 3:
                        await RegisterNewSeniorTutorAsync();
                        break;
                    case 4:
                        await ViewAllUsersAsync();
                        break;
                    case 5:
                        return;
                }
            }
        }

        private async Task RegisterNewStudentAsync()
        {
            DisplayHeader("Register New Student");

            var supervisors = await _registrationService.GetAvailableSupervisorsAsync();
            if (!supervisors.Any())
            {
                DisplayError("No Personal Supervisors available. Please register a supervisor first.");
                PressAnyKeyToContinue();
                return;
            }

            Console.Write("Student Full Name: ");
            var name = Console.ReadLine();

            Console.Write("Student Email: ");
            var email = Console.ReadLine();

            Console.Write("Student ID: ");
            var studentId = Console.ReadLine();

            Console.Write("Password: ");
            var password = Console.ReadLine();

            Console.WriteLine("\nAvailable Personal Supervisors:");
            for (int i = 0; i < supervisors.Count; i++)
            {
                Console.WriteLine($"{i + 1}. {supervisors[i].Name} ({supervisors[i].StaffId})");
            }

            Console.Write("Select Supervisor (number): ");
            if (!int.TryParse(Console.ReadLine(), out int supervisorChoice) || supervisorChoice < 1 || supervisorChoice > supervisors.Count)
            {
                DisplayError("Invalid supervisor selection.");
                PressAnyKeyToContinue();
                return;
            }

            var selectedSupervisor = supervisors[supervisorChoice - 1];

            var result = await _registrationService.RegisterStudentAsync(
                name ?? "", email ?? "", studentId ?? "", password ?? "", selectedSupervisor.Id);

            if (result.Success)
            {
                DisplaySuccess($"Student {name} registered successfully and assigned to {selectedSupervisor.Name}!");
            }
            else
            {
                DisplayError($"Registration failed: {result.ErrorMessage}");
            }

            PressAnyKeyToContinue();
        }

        private async Task RegisterNewPersonalSupervisorAsync()
        {
            DisplayHeader("Register New Personal Supervisor");

            Console.Write("Full Name: ");
            var name = Console.ReadLine();

            Console.Write("Email: ");
            var email = Console.ReadLine();

            Console.Write("Staff ID: ");
            var staffId = Console.ReadLine();

            Console.Write("Password: ");
            var password = Console.ReadLine();

            var result = await _registrationService.RegisterPersonalSupervisorAsync(
                name ?? "", email ?? "", staffId ?? "", password ?? "");

            if (result.Success)
            {
                DisplaySuccess($"Personal Supervisor {name} registered successfully!");
            }
            else
            {
                DisplayError($"Registration failed: {result.ErrorMessage}");
            }

            PressAnyKeyToContinue();
        }

        private async Task RegisterNewSeniorTutorAsync()
        {
            DisplayHeader("Register New Senior Tutor");

            Console.Write("Full Name: ");
            var name = Console.ReadLine();

            Console.Write("Email: ");
            var email = Console.ReadLine();

            Console.Write("Staff ID: ");
            var staffId = Console.ReadLine();

            Console.Write("Password: ");
            var password = Console.ReadLine();

            var result = await _registrationService.RegisterSeniorTutorAsync(
                name ?? "", email ?? "", staffId ?? "", password ?? "");

            if (result.Success)
            {
                DisplaySuccess($"Senior Tutor {name} registered successfully!");
            }
            else
            {
                DisplayError($"Registration failed: {result.ErrorMessage}");
            }

            PressAnyKeyToContinue();
        }

        private async Task ViewAllUsersAsync()
        {
            DisplayHeader("All System Users");

            var students = await _context.Students.Include(s => s.PersonalSupervisor).ToListAsync();
            var supervisors = await _context.PersonalSupervisors.ToListAsync();
            var tutors = await _context.SeniorTutors.ToListAsync();

            Console.WriteLine("=== STUDENTS ===");
            foreach (var student in students)
            {
                Console.WriteLine($"• {student.Name} ({student.StudentId}) - {student.Email} - Supervisor: {student.PersonalSupervisor.Name}");
            }

            Console.WriteLine("\n=== PERSONAL SUPERVISORS ===");
            foreach (var supervisor in supervisors)
            {
                Console.WriteLine($"• {supervisor.Name} ({supervisor.StaffId}) - {supervisor.Email} - Students: {supervisor.AssignedStudents.Count}");
            }

            Console.WriteLine("\n=== SENIOR TUTORS ===");
            foreach (var tutor in tutors)
            {
                Console.WriteLine($"• {tutor.Name} ({tutor.StaffId}) - {tutor.Email}");
            }

            PressAnyKeyToContinue();
        }

        private async Task ChangePasswordAsync()
        {
            DisplayHeader("Change Password");

            var currentPassword = GetUserInput("Current Password");
            var newPassword = GetUserInput("New Password");
            var confirmPassword = GetUserInput("Confirm New Password");

            if (newPassword != confirmPassword)
            {
                DisplayError("New passwords do not match.");
                PressAnyKeyToContinue();
                return;
            }

            if (await _authService.ChangePasswordAsync(_tutor.Id, currentPassword, newPassword))
            {
                DisplaySuccess("Password changed successfully!");
            }
            else
            {
                DisplayError("Failed to change password. Please check your current password.");
            }

            PressAnyKeyToContinue();
        }
    }
}
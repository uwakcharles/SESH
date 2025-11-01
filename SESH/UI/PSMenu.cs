using SESH.Data;
using Microsoft.EntityFrameworkCore;
using SESH.Models;
using SESH.Models.Enums;
using SESH.Services;
using SESH.Services.Interfaces;




namespace SESH.UI
{
    public class PSMenu : MenuSystem
    {
        private readonly PersonalSupervisor _supervisor;
        private readonly IReportService _reportService;
        private readonly IMeetingService _meetingService;
        private readonly IUserRegistrationService _registrationService;

        public PSMenu(ApplicationDbContext context, IAuthService authService, PersonalSupervisor supervisor,
                     IReportService reportService, IMeetingService meetingService,
                     IUserRegistrationService registrationService)
            : base(context, authService)
        {
            _supervisor = supervisor;
            _reportService = reportService;
            _meetingService = meetingService;
            _registrationService = registrationService;
        }

        public override async Task ShowMenuAsync()
        {
            while (true)
            {
                DisplayHeader($"Personal Supervisor Portal - Welcome {_supervisor.Name}");

                var options = new[]
                {
                    "View Student Dashboard",
                    "Book Meeting with Student",
                    "Manage Availability Slots",
                    "View My Meetings",
                    "Register New Student",
                    "Change Password",
                    "Logout"
                };

                var choice = GetMenuChoice(options);

                switch (choice)
                {
                    case 1:
                        await ViewStudentDashboardAsync();
                        break;
                    case 2:
                        await BookMeetingWithStudentAsync();
                        break;
                    case 3:
                        await ManageAvailabilitySlotsAsync();
                        break;
                    case 4:
                        await ViewMyMeetingsAsync();
                        break;
                    case 5:
                        await RegisterNewStudentAsync();
                        break;
                    case 6:
                        await ChangePasswordAsync();
                        break;
                    case 7:
                        return;
                }
            }
        }

        private async Task ViewStudentDashboardAsync()
        {
            DisplayHeader("Student Dashboard");

            var reports = await _reportService.GetReportsForSupervisorAsync(_supervisor.Id);
            var students = await _context.Students
                .Where(s => s.PersonalSupervisorId == _supervisor.Id)
                .Include(s => s.Reports)
                .ToListAsync();

            Console.WriteLine($"You have {students.Count} assigned students:\n");

            foreach (var student in students)
            {
                var latestReport = student.Reports.OrderByDescending(r => r.SubmittedAt).FirstOrDefault();
                var status = latestReport?.Status.ToString() ?? "No reports";
                var lastReportDate = latestReport?.SubmittedAt.ToString("MMM dd, yyyy") ?? "Never";

                Console.WriteLine($"• {student.Name} ({student.StudentId})");
                Console.WriteLine($"  Current Status: {status}");
                Console.WriteLine($"  Last Report: {lastReportDate}");
                Console.WriteLine($"  Total Reports: {student.Reports.Count}");
                Console.WriteLine();
            }

            var strugglingStudents = reports
                .Where(r => r.Status == ReportStatus.Struggling || r.Status == ReportStatus.InCrisis)
                .Select(r => r.Student)
                .Distinct()
                .ToList();

            if (strugglingStudents.Any())
            {
                DisplayWarning("Students needing attention:");
                foreach (var student in strugglingStudents)
                {
                    Console.WriteLine($"  ⚠ {student.Name} - {student.Reports.OrderByDescending(r => r.SubmittedAt).First()?.Status}");
                }
            }

            PressAnyKeyToContinue();
        }

        private async Task BookMeetingWithStudentAsync()
        {
            DisplayHeader("Book Meeting with Student");

            var students = await _context.Students
                .Where(s => s.PersonalSupervisorId == _supervisor.Id)
                .ToListAsync();

            if (!students.Any())
            {
                DisplayWarning("No students assigned to you.");
                PressAnyKeyToContinue();
                return;
            }

            Console.WriteLine("Select a student:\n");
            for (int i = 0; i < students.Count; i++)
            {
                Console.WriteLine($"{i + 1}. {students[i].Name} ({students[i].StudentId})");
            }

            Console.Write($"\nSelect student (1-{students.Count}): ");
            if (!int.TryParse(Console.ReadLine(), out int studentChoice) || studentChoice < 1 || studentChoice > students.Count)
            {
                DisplayError("Invalid student selection.");
                PressAnyKeyToContinue();
                return;
            }

            var selectedStudent = students[studentChoice - 1];
            var availableSlots = await _meetingService.GetSupervisorAvailabilityAsync(_supervisor.Id);

            if (!availableSlots.Any(s => !s.IsBooked))
            {
                DisplayWarning("No available time slots. Please add availability first.");
                PressAnyKeyToContinue();
                return;
            }

            Console.WriteLine("\nYour available slots:");
            var freeSlots = availableSlots.Where(s => !s.IsBooked).ToList();
            for (int i = 0; i < freeSlots.Count; i++)
            {
                var slot = freeSlots[i];
                Console.WriteLine($"{i + 1}. {slot.StartTime:ddd, MMM dd, yyyy 'at' hh:mm tt}");
            }

            Console.Write($"\nSelect slot (1-{freeSlots.Count}): ");
            if (!int.TryParse(Console.ReadLine(), out int slotChoice) || slotChoice < 1 || slotChoice > freeSlots.Count)
            {
                DisplayError("Invalid slot selection.");
                PressAnyKeyToContinue();
                return;
            }

            var selectedSlot = freeSlots[slotChoice - 1];

            Console.Write("Meeting title: ");
            var title = Console.ReadLine() ?? $"Meeting with {selectedStudent.Name}";

            Console.Write("Meeting description (optional): ");
            var description = Console.ReadLine() ?? string.Empty;

            var result = await _meetingService.BookMeetingAsync(
                _supervisor.Id, selectedStudent.Id, selectedSlot.Id, title, description);

            if (result.Success)
            {
                DisplaySuccess($"Meeting booked with {selectedStudent.Name} for {selectedSlot.StartTime:MMM dd, yyyy 'at' hh:mm tt}!");
            }
            else
            {
                DisplayError(result.ErrorMessage!);
            }

            PressAnyKeyToContinue();
        }

        private async Task ManageAvailabilitySlotsAsync()
        {
            DisplayHeader("Manage Availability Slots");

            while (true)
            {
                var slots = await _meetingService.GetSupervisorAvailabilityAsync(_supervisor.Id);

                Console.WriteLine("Your current availability:\n");
                if (!slots.Any())
                {
                    Console.WriteLine("No availability slots set.");
                }
                else
                {
                    foreach (var slot in slots.OrderBy(s => s.StartTime))
                    {
                        var status = slot.IsBooked ? "BOOKED" : "AVAILABLE";
                        Console.WriteLine($"• {slot.StartTime:ddd, MMM dd, yyyy 'at' hh:mm tt} - {status}");
                    }
                }

                Console.WriteLine("\n1. Add new availability slot");
                Console.WriteLine("2. Back to main menu");
                Console.Write("\nEnter choice: ");

                var choice = Console.ReadLine();
                if (choice == "1")
                {
                    await AddAvailabilitySlotAsync();
                }
                else if (choice == "2")
                {
                    break;
                }
                else
                {
                    DisplayError("Invalid choice.");
                }
            }
        }

        private async Task AddAvailabilitySlotAsync()
        {
            Console.WriteLine("\n--- Add New Availability Slot ---");

            Console.Write("Enter date (yyyy-mm-dd): ");
            if (!DateTime.TryParse(Console.ReadLine(), out DateTime date))
            {
                DisplayError("Invalid date format.");
                return;
            }

            Console.Write("Enter start time (hh:mm): ");
            if (!TimeSpan.TryParse(Console.ReadLine(), out TimeSpan startTime))
            {
                DisplayError("Invalid time format.");
                return;
            }

            Console.Write("Enter end time (hh:mm): ");
            if (!TimeSpan.TryParse(Console.ReadLine(), out TimeSpan endTime))
            {
                DisplayError("Invalid time format.");
                return;
            }

            var startDateTime = date.Add(startTime);
            var endDateTime = date.Add(endTime);

            if (startDateTime <= DateTime.Now)
            {
                DisplayError("Cannot add availability in the past.");
                return;
            }

            if (endDateTime <= startDateTime)
            {
                DisplayError("End time must be after start time.");
                return;
            }

            await _meetingService.AddAvailabilitySlotAsync(_supervisor.Id, startDateTime, endDateTime);
            DisplaySuccess("Availability slot added successfully!");
        }

        private async Task ViewMyMeetingsAsync()
        {
            DisplayHeader("My Scheduled Meetings");

            var meetings = await _meetingService.GetUserMeetingsAsync(_supervisor.Id);

            if (!meetings.Any())
            {
                Console.WriteLine("No scheduled meetings.");
            }
            else
            {
                foreach (var meeting in meetings)
                {
                    var otherPerson = meeting.BookedById == _supervisor.Id ? meeting.BookedWith : meeting.BookedBy;
                    Console.WriteLine($"• {meeting.Title}");
                    Console.WriteLine($"  With: {otherPerson.Name}");
                    Console.WriteLine($"  When: {meeting.ScheduledAt:MMM dd, yyyy 'at' hh:mm tt}");
                    Console.WriteLine($"  Status: {meeting.Status}");
                    Console.WriteLine();
                }
            }

            PressAnyKeyToContinue();
        }

        private async Task RegisterNewStudentAsync()
        {
            DisplayHeader("Register New Student");

            Console.Write("Student Full Name: ");
            var name = Console.ReadLine();

            Console.Write("Student Email: ");
            var email = Console.ReadLine();

            Console.Write("Student ID: ");
            var studentId = Console.ReadLine();

            Console.Write("Password: ");
            var password = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(email) ||
                string.IsNullOrWhiteSpace(studentId) || string.IsNullOrWhiteSpace(password))
            {
                DisplayError("All fields are required.");
                PressAnyKeyToContinue();
                return;
            }

            var result = await _registrationService.RegisterStudentAsync(
                name, email, studentId, password, _supervisor.Id);

            if (result.Success)
            {
                DisplaySuccess($"Student {name} registered successfully and assigned to you!");
            }
            else
            {
                DisplayError($"Registration failed: {result.ErrorMessage}");
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

            if (await _authService.ChangePasswordAsync(_supervisor.Id, currentPassword, newPassword))
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
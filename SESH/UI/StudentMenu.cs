using SESH.Data;
using SESH.Models;
using SESH.Models.Enums;
using SESH.Services;
using SESH.Services.Interfaces;

namespace SESH.UI
{
    public class StudentMenu : MenuSystem
    {
        private readonly Student _student;
        private readonly IReportService _reportService;
        private readonly IMeetingService _meetingService;
        private readonly IUserRegistrationService _registrationService;

        public StudentMenu(ApplicationDbContext context, IAuthService authService, Student student,
                         IReportService reportService, IMeetingService meetingService,
                         IUserRegistrationService registrationService)
            : base(context, authService)
        {
            _student = student;
            _reportService = reportService;
            _meetingService = meetingService;
            _registrationService = registrationService;
        }

        public override async Task ShowMenuAsync()
        {
            while (true)
            {
                DisplayHeader($"Student Portal - Welcome {_student.Name}");

                var options = new[]
                {
                    "Submit Well-being Report",
                    "Book Meeting with Personal Supervisor",
                    "View My Meetings",
                    "View My Reports History",
                    "Change Password",
                    "Logout"
                };

                var choice = GetMenuChoice(options);

                switch (choice)
                {
                    case 1:
                        await SubmitWellBeingReportAsync();
                        break;
                    case 2:
                        await BookMeetingWithPSAsync();
                        break;
                    case 3:
                        await ViewMyMeetingsAsync();
                        break;
                    case 4:
                        await ViewMyReportsAsync();
                        break;
                    case 5:
                        await ChangePasswordAsync();
                        break;
                    case 6:
                        return;
                }
            }
        }

        private async Task SubmitWellBeingReportAsync()
        {
            DisplayHeader("Submit Well-being Report");

            if (!await _reportService.CanSubmitReportAsync(_student.Id))
            {
                DisplayWarning("You have already submitted a report this week. You can submit again in 7 days.");
                PressAnyKeyToContinue();
                return;
            }

            Console.WriteLine("How are you feeling this week?");
            var statusOptions = new[] { "Thriving", "Okay", "Struggling", "In Crisis" };
            var statusChoice = GetMenuChoice(statusOptions);

            var status = (ReportStatus)(statusChoice - 1);

            Console.WriteLine("\nAdd any additional notes (optional, max 500 characters):");
            var notes = Console.ReadLine() ?? string.Empty;

            var result = await _reportService.SubmitReportAsync(_student.Id, status, notes);

            if (result.Success)
            {
                DisplaySuccess("Well-being report submitted successfully!");
                if (status == ReportStatus.Struggling || status == ReportStatus.InCrisis)
                {
                    DisplayWarning("Your Personal Supervisor has been notified of your status.");
                }
            }
            else
            {
                DisplayError(result.ErrorMessage!);
            }

            PressAnyKeyToContinue();
        }

        private async Task BookMeetingWithPSAsync()
        {
            DisplayHeader("Book Meeting with Personal Supervisor");

            var availableSlots = await _meetingService.GetAvailableSlotsAsync(_student.PersonalSupervisorId);

            if (!availableSlots.Any())
            {
                DisplayWarning("No available meeting slots at the moment. Please check back later.");
                PressAnyKeyToContinue();
                return;
            }

            Console.WriteLine("Available meeting slots:\n");
            for (int i = 0; i < availableSlots.Count; i++)
            {
                var slot = availableSlots[i];
                Console.WriteLine($"{i + 1}. {slot.StartTime:ddd, MMM dd, yyyy 'at' hh:mm tt}");
            }

            Console.Write($"\nSelect a slot (1-{availableSlots.Count}): ");
            if (int.TryParse(Console.ReadLine(), out int slotChoice) && slotChoice >= 1 && slotChoice <= availableSlots.Count)
            {
                var selectedSlot = availableSlots[slotChoice - 1];

                Console.Write("Meeting title: ");
                var title = Console.ReadLine() ?? "Meeting with Student";

                Console.Write("Meeting description (optional): ");
                var description = Console.ReadLine() ?? string.Empty;

                var result = await _meetingService.BookMeetingAsync(
                    _student.Id, _student.PersonalSupervisorId, selectedSlot.Id, title, description);

                if (result.Success)
                {
                    DisplaySuccess($"Meeting booked successfully for {selectedSlot.StartTime:MMM dd, yyyy 'at' hh:mm tt}!");
                }
                else
                {
                    DisplayError(result.ErrorMessage!);
                }
            }
            else
            {
                DisplayError("Invalid slot selection.");
            }

            PressAnyKeyToContinue();
        }

        private async Task ViewMyMeetingsAsync()
        {
            DisplayHeader("My Scheduled Meetings");

            var meetings = await _meetingService.GetUserMeetingsAsync(_student.Id);

            if (!meetings.Any())
            {
                Console.WriteLine("No scheduled meetings.");
            }
            else
            {
                foreach (var meeting in meetings)
                {
                    Console.WriteLine($"• {meeting.Title}");
                    Console.WriteLine($"  With: {meeting.BookedWith.Name}");
                    Console.WriteLine($"  When: {meeting.ScheduledAt:MMM dd, yyyy 'at' hh:mm tt}");
                    Console.WriteLine($"  Status: {meeting.Status}");
                    Console.WriteLine();
                }
            }

            PressAnyKeyToContinue();
        }

        private async Task ViewMyReportsAsync()
        {
            DisplayHeader("My Well-being Reports History");

            var reports = await _reportService.GetStudentReportsAsync(_student.Id);

            if (!reports.Any())
            {
                Console.WriteLine("No reports submitted yet.");
            }
            else
            {
                foreach (var report in reports.OrderByDescending(r => r.SubmittedAt))
                {
                    Console.WriteLine($"• {report.SubmittedAt:MMM dd, yyyy}: {report.Status}");
                    if (!string.IsNullOrEmpty(report.Notes))
                    {
                        Console.WriteLine($"  Notes: {report.Notes}");
                    }
                    Console.WriteLine();
                }
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

            if (await _authService.ChangePasswordAsync(_student.Id, currentPassword, newPassword))
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
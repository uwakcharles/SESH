using Microsoft.EntityFrameworkCore;
using SESH.Data;
using SESH.Models;
using SESH.Services;
using SESH.Services.Interfaces;
using SESH.UI;

namespace SESH
{
    class Program
    {
        static async Task Main(string[] args)
        {
            using var context = new ApplicationDbContext();
            context.Database.EnsureCreated();
            SeedData.Initialize(context);

            var authService = new AuthService(context);
            var reportService = new ReportService(context);
            var meetingService = new MeetingService(context);
            var registrationService = new UserRegistrationService(context);
            var analyticsService = new AnalyticsService(context);

            Console.WriteLine("====================================");
            Console.WriteLine("    SESH - Student Engagement Support Hub");
            Console.WriteLine("====================================");

            while (true)
            {
                Console.WriteLine("\n1. Login");
                Console.WriteLine("2. Register New User");
                Console.WriteLine("3. Exit");
                Console.Write("\nEnter your choice: ");

                var choice = Console.ReadLine();
                switch (choice)
                {
                    case "1":
                        await HandleLogin(context, authService, reportService, meetingService, analyticsService, registrationService);
                        break;
                    case "2":
                        await HandleRegistration(registrationService, context);
                        break;
                    case "3":
                        Console.WriteLine("Goodbye!");
                        return;
                    default:
                        Console.WriteLine("Invalid choice. Please try again.");
                        break;
                }
            }
        }

        private static async Task HandleLogin(ApplicationDbContext context, IAuthService authService,
            IReportService reportService, IMeetingService meetingService, AnalyticsService analyticsService,
            IUserRegistrationService registrationService)
        {
            Console.Write("Email: ");
            var email = Console.ReadLine();
            Console.Write("Password: ");
            var password = Console.ReadLine();

            var user = await authService.LoginAsync(email ?? "", password ?? "");
            if (user == null)
            {
                Console.WriteLine("Invalid email or password.");
                return;
            }

            Console.WriteLine($"\nWelcome, {user.Name}!");

            switch (user)
            {
                case Student student:
                    var studentMenu = new StudentMenu(context, authService, student, reportService, meetingService, registrationService);
                    await studentMenu.ShowMenuAsync();
                    break;
                case PersonalSupervisor supervisor:
                    var psMenu = new PSMenu(context, authService, supervisor, reportService, meetingService, registrationService);
                    await psMenu.ShowMenuAsync();
                    break;
                case SeniorTutor tutor:
                    var stMenu = new SeniorTutorMenu(context, authService, tutor, reportService, meetingService, analyticsService, registrationService);
                    await stMenu.ShowMenuAsync();
                    break;
            }
        }

        private static async Task HandleRegistration(IUserRegistrationService registrationService, ApplicationDbContext context)
        {
            Console.WriteLine("\n=== User Registration ===");
            Console.WriteLine("1. Register as Student");
            Console.WriteLine("2. Register as Personal Supervisor");
            Console.WriteLine("3. Register as Senior Tutor");
            Console.WriteLine("4. Back to Main Menu");
            Console.Write("\nSelect user type: ");

            var choice = Console.ReadLine();
            switch (choice)
            {
                case "1":
                    await RegisterStudent(registrationService, context);
                    break;
                case "2":
                    await RegisterPersonalSupervisor(registrationService);
                    break;
                case "3":
                    await RegisterSeniorTutor(registrationService);
                    break;
                case "4":
                    return;
                default:
                    Console.WriteLine("Invalid choice.");
                    break;
            }
        }

        private static async Task RegisterStudent(IUserRegistrationService registrationService, ApplicationDbContext context)
        {
            Console.WriteLine("\n--- Student Registration ---");

            Console.Write("Full Name: ");
            var name = Console.ReadLine();

            Console.Write("Email: ");
            var email = Console.ReadLine();

            Console.Write("Student ID: ");
            var studentId = Console.ReadLine();

            Console.Write("Password: ");
            var password = Console.ReadLine();

            var supervisors = await registrationService.GetAvailableSupervisorsAsync();
            if (!supervisors.Any())
            {
                Console.WriteLine("No Personal Supervisors available. Please contact administrator.");
                return;
            }

            Console.WriteLine("\nAvailable Personal Supervisors:");
            for (int i = 0; i < supervisors.Count; i++)
            {
                Console.WriteLine($"{i + 1}. {supervisors[i].Name} ({supervisors[i].StaffId})");
            }

            Console.Write("Select Supervisor (number): ");
            if (!int.TryParse(Console.ReadLine(), out int supervisorChoice) || supervisorChoice < 1 || supervisorChoice > supervisors.Count)
            {
                Console.WriteLine("Invalid supervisor selection.");
                return;
            }

            var selectedSupervisor = supervisors[supervisorChoice - 1];

            var result = await registrationService.RegisterStudentAsync(
                name ?? "", email ?? "", studentId ?? "", password ?? "", selectedSupervisor.Id);

            if (result.Success)
            {
                Console.WriteLine($"✅ Student registered successfully! Assigned to {selectedSupervisor.Name}.");
            }
            else
            {
                Console.WriteLine($"❌ Registration failed: {result.ErrorMessage}");
            }
        }

        private static async Task RegisterPersonalSupervisor(IUserRegistrationService registrationService)
        {
            Console.WriteLine("\n--- Personal Supervisor Registration ---");

            Console.Write("Full Name: ");
            var name = Console.ReadLine();

            Console.Write("Email: ");
            var email = Console.ReadLine();

            Console.Write("Staff ID: ");
            var staffId = Console.ReadLine();

            Console.Write("Password: ");
            var password = Console.ReadLine();

            var result = await registrationService.RegisterPersonalSupervisorAsync(
                name ?? "", email ?? "", staffId ?? "", password ?? "");

            if (result.Success)
            {
                Console.WriteLine("✅ Personal Supervisor registered successfully!");
            }
            else
            {
                Console.WriteLine($"❌ Registration failed: {result.ErrorMessage}");
            }
        }

        private static async Task RegisterSeniorTutor(IUserRegistrationService registrationService)
        {
            Console.WriteLine("\n--- Senior Tutor Registration ---");

            Console.Write("Full Name: ");
            var name = Console.ReadLine();

            Console.Write("Email: ");
            var email = Console.ReadLine();

            Console.Write("Staff ID: ");
            var staffId = Console.ReadLine();

            Console.Write("Password: ");
            var password = Console.ReadLine();

            var result = await registrationService.RegisterSeniorTutorAsync(
                name ?? "", email ?? "", staffId ?? "", password ?? "");

            if (result.Success)
            {
                Console.WriteLine("✅ Senior Tutor registered successfully!");
            }
            else
            {
                Console.WriteLine($"❌ Registration failed: {result.ErrorMessage}");
            }
        }
    }
}
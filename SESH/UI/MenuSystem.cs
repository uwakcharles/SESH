using SESH.Data;
using SESH.Models;
using SESH.Services.Interfaces;
using SESH.Services;

namespace SESH.UI
{
    public abstract class MenuSystem
    {
        protected readonly ApplicationDbContext _context;
        protected readonly IAuthService _authService;
        protected User? _currentUser;

        protected MenuSystem(ApplicationDbContext context, IAuthService authService)
        {
            _context = context;
            _authService = authService;
        }

        public abstract Task ShowMenuAsync();

        protected void DisplayHeader(string title)
        {
            Console.Clear();
            Console.WriteLine("====================================");
            Console.WriteLine($"    SESH - {title}");
            Console.WriteLine("====================================");
            Console.WriteLine();
        }

        protected void DisplaySuccess(string message)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"✓ {message}");
            Console.ResetColor();
        }

        protected void DisplayError(string message)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"✗ {message}");
            Console.ResetColor();
        }

        protected void DisplayWarning(string message)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"⚠ {message}");
            Console.ResetColor();
        }

        protected string GetUserInput(string prompt)
        {
            Console.Write($"{prompt}: ");
            return Console.ReadLine() ?? string.Empty;
        }

        protected void PressAnyKeyToContinue()
        {
            Console.WriteLine("\nPress any key to continue...");
            Console.ReadKey();
        }

        protected int GetMenuChoice(string[] options)
        {
            for (int i = 0; i < options.Length; i++)
            {
                Console.WriteLine($"{i + 1}. {options[i]}");
            }

            while (true)
            {
                Console.Write("\nEnter your choice: ");
                if (int.TryParse(Console.ReadLine(), out int choice) && choice >= 1 && choice <= options.Length)
                {
                    return choice;
                }
                DisplayError("Invalid choice. Please try again.");
            }
        }
    }
}
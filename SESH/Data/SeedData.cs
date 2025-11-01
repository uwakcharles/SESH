using SESH.Models;  
using SESH.Models.Enums;

namespace SESH.Data
{
    public static class SeedData
    {
        public static void Initialize(ApplicationDbContext context)
        {
            context.Database.EnsureCreated();

            if (!context.Users.Any())
            {
                var adminTutor = new SeniorTutor()
                {
                    Name = "Administrator",
                    Email = "admin@hull.ac.uk",
                    StaffId = "ST001",
                    Role = UserRole.SeniorTutor
                };
                adminTutor.SetPassword("admin123");
                context.SeniorTutors.Add(adminTutor);

                context.SaveChanges();

                Console.WriteLine("Default Senior Tutor created:");
                Console.WriteLine($"Email: {adminTutor.Email}");
                Console.WriteLine($"Password: uwak123");
            }
        }
    }
}
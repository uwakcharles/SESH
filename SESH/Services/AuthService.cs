using Microsoft.EntityFrameworkCore;
using SESH.Data;
using SESH.Models;
using SESH.Services.Interfaces;

namespace SESH.Services
{
    public class AuthService : IAuthService
    {
        private readonly ApplicationDbContext _context;

        public AuthService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<User?> LoginAsync(string email, string password)
        {
            var user = await _context.Users
                .Include(u => u is Student ? (u as Student)!.PersonalSupervisor : null)
                .FirstOrDefaultAsync(u => u.Email == email);

            if (user == null || !user.Authenticate(password))
                return null;

            return user;
        }

        public async Task<bool> ChangePasswordAsync(int userId, string currentPassword, string newPassword)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null || !user.Authenticate(currentPassword))
                return false;

            user.SetPassword(newPassword);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
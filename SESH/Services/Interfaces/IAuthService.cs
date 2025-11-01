using SESH.Models;

namespace SESH.Services.Interfaces
{
    public interface IAuthService
    {
        Task<User?> LoginAsync(string email, string password);
        Task<bool> ChangePasswordAsync(int userId, string currentPassword, string newPassword);
    }
}
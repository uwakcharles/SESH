using SESH.Models;

namespace SESH.Services.Interfaces
{
    public class RegistrationResult
    {
        public bool Success { get; set; }
        public string? ErrorMessage { get; set; }
        public User? User { get; set; }

        public static RegistrationResult SuccessResult(User user)
            => new() { Success = true, User = user };

        public static RegistrationResult FailureResult(string error)
            => new() { Success = false, ErrorMessage = error };
    }

    public interface IUserRegistrationService
    {
        Task<RegistrationResult> RegisterStudentAsync(string name, string email, string studentId, string password, int supervisorId);
        Task<RegistrationResult> RegisterPersonalSupervisorAsync(string name, string email, string staffId, string password);
        Task<RegistrationResult> RegisterSeniorTutorAsync(string name, string email, string staffId, string password);
        Task<bool> EmailExistsAsync(string email);
        Task<bool> StudentIdExistsAsync(string studentId);
        Task<bool> StaffIdExistsAsync(string staffId);
        Task<List<PersonalSupervisor>> GetAvailableSupervisorsAsync();
    }
}
using Microsoft.EntityFrameworkCore;
using SESH.Data;
using SESH.Models;
using SESH.Models.Enums;
using SESH.Services.Interfaces;

namespace SESH.Services
{
    public class UserRegistrationService : IUserRegistrationService
    {
        private readonly ApplicationDbContext _context;

        public UserRegistrationService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<RegistrationResult> RegisterStudentAsync(string name, string email, string studentId, string password, int supervisorId)
        {
            if (await EmailExistsAsync(email))
                return RegistrationResult.FailureResult("Email already exists in system.");

            if (await StudentIdExistsAsync(studentId))
                return RegistrationResult.FailureResult("Student ID already exists in system.");

            var supervisor = await _context.PersonalSupervisors.FindAsync(supervisorId);
            if (supervisor == null)
                return RegistrationResult.FailureResult("Selected Personal Supervisor does not exist.");

            var student = new Student
            {
                Name = name.Trim(),
                Email = email.ToLower().Trim(),
                StudentId = studentId.ToUpper().Trim(),
                PersonalSupervisorId = supervisorId,
                Role = UserRole.Student
            };
            student.SetPassword(password);

            _context.Students.Add(student);
            await _context.SaveChangesAsync();

            return RegistrationResult.SuccessResult(student);
        }

        public async Task<RegistrationResult> RegisterPersonalSupervisorAsync(string name, string email, string staffId, string password)
        {
            if (await EmailExistsAsync(email))
                return RegistrationResult.FailureResult("Email already exists in system.");

            if (await StaffIdExistsAsync(staffId))
                return RegistrationResult.FailureResult("Staff ID already exists in system.");

            var supervisor = new PersonalSupervisor
            {
                Name = name.Trim(),
                Email = email.ToLower().Trim(),
                StaffId = staffId.ToUpper().Trim(),
                Role = UserRole.PersonalSupervisor
            };
            supervisor.SetPassword(password);

            _context.PersonalSupervisors.Add(supervisor);
            await _context.SaveChangesAsync();

            return RegistrationResult.SuccessResult(supervisor);
        }

        public async Task<RegistrationResult> RegisterSeniorTutorAsync(string name, string email, string staffId, string password)
        {
            if (await EmailExistsAsync(email))
                return RegistrationResult.FailureResult("Email already exists in system.");

            if (await StaffIdExistsAsync(staffId))
                return RegistrationResult.FailureResult("Staff ID already exists in system.");

            var tutor = new SeniorTutor
            {
                Name = name.Trim(),
                Email = email.ToLower().Trim(),
                StaffId = staffId.ToUpper().Trim(),
                Role = UserRole.SeniorTutor
            };
            tutor.SetPassword(password);

            _context.SeniorTutors.Add(tutor);
            await _context.SaveChangesAsync();

            return RegistrationResult.SuccessResult(tutor);
        }

        public async Task<bool> EmailExistsAsync(string email)
        {
            return await _context.Users.AnyAsync(u => u.Email.ToLower() == email.ToLower().Trim());
        }

        public async Task<bool> StudentIdExistsAsync(string studentId)
        {
            return await _context.Students.AnyAsync(s => s.StudentId.ToUpper() == studentId.ToUpper().Trim());
        }

        public async Task<bool> StaffIdExistsAsync(string staffId)
        {
            return await _context.Users.OfType<PersonalSupervisor>()
                .AnyAsync(ps => ps.StaffId.ToUpper() == staffId.ToUpper().Trim()) ||
                   await _context.Users.OfType<SeniorTutor>()
                .AnyAsync(st => st.StaffId.ToUpper() == staffId.ToUpper().Trim());
        }

        public async Task<List<PersonalSupervisor>> GetAvailableSupervisorsAsync()
        {
            return await _context.PersonalSupervisors
                .OrderBy(ps => ps.Name)
                .ToListAsync();
        }
    }
}
using SESH.Models;
using SESH.Services;

namespace SESH.Tests
{
    public class UserRegistrationServiceTests : TestBase
    {
        private readonly UserRegistrationService _registrationService;

        public UserRegistrationServiceTests()
        {
            _registrationService = new UserRegistrationService(_context);
        }

        [Fact]
        public async Task RegisterStudent_ValidData_ReturnsSuccess()
        {
            // Arrange
            var name = "New Student";
            var email = "new.student@edu.hud.ac.uk";
            var studentId = "S99999";
            var password = "password123";
            var supervisorId = 1;

            // Act
            var result = await _registrationService.RegisterStudentAsync(name, email, studentId, password, supervisorId);

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.User);
            Assert.Equal(name, result.User.Name);
            Assert.Equal(email, result.User.Email);
            Assert.IsType<Student>(result.User);

            var student = result.User as Student;
            Assert.Equal(supervisorId, student?.PersonalSupervisorId);
        }

        [Fact]
        public async Task RegisterStudent_DuplicateEmail_ReturnsFailure()
        {
            // Arrange
            var existingEmail = "test.student@edu.hud.ac.uk";

            // Act
            var result = await _registrationService.RegisterStudentAsync("Another Student", existingEmail, "S88888", "pass123", 1);

            // Assert
            Assert.False(result.Success);
            Assert.Contains("Email already exists", result.ErrorMessage);
        }

        [Fact]
        public async Task RegisterStudent_DuplicateStudentId_ReturnsFailure()
        {
            // Arrange
            var existingStudentId = "S12345";

            // Act
            var result = await _registrationService.RegisterStudentAsync("Another Student", "new@edu.hud.ac.uk", existingStudentId, "pass123", 1);

            // Assert
            Assert.False(result.Success);
            Assert.Contains("Student ID already exists", result.ErrorMessage);
        }

        [Fact]
        public async Task RegisterStudent_InvalidSupervisor_ReturnsFailure()
        {
            // Arrange
            var invalidSupervisorId = 999;

            // Act
            var result = await _registrationService.RegisterStudentAsync("New Student", "new@edu.hud.ac.uk", "S77777", "pass123", invalidSupervisorId);

            // Assert
            Assert.False(result.Success);
            Assert.Contains("does not exist", result.ErrorMessage);
        }

        [Fact]
        public async Task RegisterPersonalSupervisor_ValidData_ReturnsSuccess()
        {
            // Arrange
            var name = "New Supervisor";
            var email = "new.supervisor@hud.ac.uk";
            var staffId = "PS999";
            var password = "password123";

            // Act
            var result = await _registrationService.RegisterPersonalSupervisorAsync(name, email, staffId, password);

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.User);
            Assert.Equal(name, result.User.Name);
            Assert.IsType<PersonalSupervisor>(result.User);
        }

        [Fact]
        public async Task RegisterSeniorTutor_ValidData_ReturnsSuccess()
        {
            // Arrange
            var name = "New Senior Tutor";
            var email = "new.tutor@hud.ac.uk";
            var staffId = "ST999";
            var password = "password123";

            // Act
            var result = await _registrationService.RegisterSeniorTutorAsync(name, email, staffId, password);

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.User);
            Assert.Equal(name, result.User.Name);
            Assert.IsType<SeniorTutor>(result.User);
        }

        [Fact]
        public async Task GetAvailableSupervisors_ReturnsSupervisors()
        {
            // Act
            var supervisors = await _registrationService.GetAvailableSupervisorsAsync();

            // Assert
            Assert.NotEmpty(supervisors);
            Assert.All(supervisors, s => Assert.IsType<PersonalSupervisor>(s));
        }

        [Fact]
        public async Task EmailExists_ExistingEmail_ReturnsTrue()
        {
            // Arrange
            var existingEmail = "test.student@edu.hud.ac.uk";

            // Act
            var exists = await _registrationService.EmailExistsAsync(existingEmail);

            // Assert
            Assert.True(exists);
        }

        [Fact]
        public async Task EmailExists_NonExistingEmail_ReturnsFalse()
        {
            // Arrange
            var nonExistingEmail = "nonexisting@test.com";

            // Act
            var exists = await _registrationService.EmailExistsAsync(nonExistingEmail);

            // Assert
            Assert.False(exists);
        }
    }
}
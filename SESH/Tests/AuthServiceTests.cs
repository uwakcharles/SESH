using SESH.Models;
using SESH.Services;

namespace SESH.Tests
{
    public class AuthServiceTests : TestBase
    {
        private readonly AuthService _authService;

        public AuthServiceTests()
        {
            _authService = new AuthService(_context);
        }

        [Fact]
        public async Task Login_ValidCredentials_ReturnsUser()
        {
            // Arrange
            var email = "test.student@edu.hud.ac.uk";
            var password = "student123";

            // Act
            var user = await _authService.LoginAsync(email, password);

            // Assert
            Assert.NotNull(user);
            Assert.Equal(email, user.Email);
            Assert.IsType<Student>(user);
        }

        [Fact]
        public async Task Login_ValidSupervisorCredentials_ReturnsSupervisor()
        {
            // Arrange
            var email = "test.supervisor@hud.ac.uk";
            var password = "password123";

            // Act
            var user = await _authService.LoginAsync(email, password);

            // Assert
            Assert.NotNull(user);
            Assert.Equal(email, user.Email);
            Assert.IsType<PersonalSupervisor>(user);
        }

        [Fact]
        public async Task Login_InvalidPassword_ReturnsNull()
        {
            // Arrange
            var email = "test.student@edu.hud.ac.uk";
            var wrongPassword = "wrongpassword";

            // Act
            var user = await _authService.LoginAsync(email, wrongPassword);

            // Assert
            Assert.Null(user);
        }

        [Fact]
        public async Task Login_InvalidEmail_ReturnsNull()
        {
            // Arrange
            var nonExistentEmail = "nonexistent@test.com";
            var password = "anypassword";

            // Act
            var user = await _authService.LoginAsync(nonExistentEmail, password);

            // Assert
            Assert.Null(user);
        }

        [Fact]
        public async Task ChangePassword_ValidCurrentPassword_ChangesSuccessfully()
        {
            // Arrange
            var userId = 2; // Student
            var currentPassword = "student123";
            var newPassword = "newsecurepassword123";

            // Act
            var result = await _authService.ChangePasswordAsync(userId, currentPassword, newPassword);

            // Assert
            Assert.True(result);

            // Verify new password works
            var user = await _context.Users.FindAsync(userId);
            Assert.True(user?.Authenticate(newPassword));
        }

        [Fact]
        public async Task ChangePassword_InvalidCurrentPassword_ReturnsFalse()
        {
            // Arrange
            var userId = 2;
            var wrongCurrentPassword = "wrongpassword";
            var newPassword = "newpassword123";

            // Act
            var result = await _authService.ChangePasswordAsync(userId, wrongCurrentPassword, newPassword);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task ChangePassword_InvalidUser_ReturnsFalse()
        {
            // Arrange
            var invalidUserId = 999;
            var currentPassword = "anypassword";
            var newPassword = "newpassword123";

            // Act
            var result = await _authService.ChangePasswordAsync(invalidUserId, currentPassword, newPassword);

            // Assert
            Assert.False(result);
        }
    }
}
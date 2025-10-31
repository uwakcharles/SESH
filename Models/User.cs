using System.ComponentModel.DataAnnotations;
using SESH.Models.Enums;

namespace SESH.Models
{
    /// <summary>
    /// defines a user within the SESH application.
    /// store user information such as name, email, password hash, and role.
    /// then manage relationships with meetings.
    /// and provide methods for authentication.
    /// <summary/>
    public class User : BaseModel
    {
        [Required, StringLength(100)]]
        public string Name { get; set; } = string.Empty;

        [Required, EmailAddress, StringLength(255)]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string PasswordHash { get; set; } = string.Empty;

        [Required]
        public UserRole Role { get; set; }

        // Navigation properties
        public ICollection<Meeting> MeetingsBooked { get; set; } = new List<Meeting>();
        public ICollection<Meeting> MeetingsWith { get; set; } = new List<Meeting>();


        /// Authenticates the user by verifying the provided password against the stored password hash.
        /// </summary>
        /// <param name="password">The plaintext password to verify.</param>
        /// <returns>True if the password is correct; otherwise, false.</returns>
        public bool Authenticate(string password)
        {
            return BCrypt.Net.BCrypt.Verify(password, PasswordHash);
        }

        /// Sets the user's password by hashing the provided plaintext password    
        public void SetPassword(string password)
        {
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password);
        }

    }
}
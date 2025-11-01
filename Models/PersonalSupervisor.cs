using System.ComponentModel.DataAnnotations;

namespace SESH.Models
{
    /// <summary>
    /// Represents a personal supervisor within the SESH application.
    /// Inherits from the User class and adds specific properties for supervisors.
    /// </summary>
    public class PersonalSupervisor : User
    {
        [Required, StringLength(20)]
        public string StaffId { get; set; } = string.Empty;

        // Collection of students supervised by this personal supervisor
        public virtual ICollection<Student> AssignedStudents { get; set; } = new List<Student>();

        public virtual ICollection<AvailabilitySlot> Availability { get; set; } = new List<AvailabilitySlot>();

        public PersonalSupervisor()
        {
            Role = Enums.UserRole.PersonalSupervisor;
        }
    }
}
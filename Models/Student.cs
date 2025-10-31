using System.ComponentModel.DataAnnotations;

namespace SESH.Models
{
    public class Student : User
{
        [Required, StringLength(20)]
        public string StudentId { get; set; } = string.Empty;

        // Foreign key and relationship to PersonalSupervisor
        public int PersonalSupervisorId { get; set; }
        public PersonalSupervisor PersonalSupervisor { get; set; } = null!;

        //Collection of reports submiited by the student
        public virtual ICollection<WellBeingReport> Reports { get; set; } = new List<WellBeingReport>();

        public Student()
        {
            Role = Enums.UserRole.Student;
        }
    }
}
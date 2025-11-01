using System.ComponentModel.DataAnnotations;

namespace SESH.Models
{
    public class SeniorTutor : User
    {
        [Required, StringLength(20)]
        public string StaffId { get; set; } = string.Empty;

        public SeniorTutor()
        {
            Role = Enums.UserRole.SeniorTutor;
        }
    }
}
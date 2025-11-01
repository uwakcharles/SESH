using System;
using System.ComponentModel.DataAnnotations;

namespace SESH.Models
{
    public class AvailabilitySlot : BaseModel
    {
        [Required]
        public int PersonalSupervisorId { get; set; }
        public virtual PersonalSupervisor PersonalSupervisor { get; set; } = null!;

        [Required]
        public DateTime StartTime { get; set; }

        [Required]
        public DateTime EndTime { get; set; }

        public bool IsBooked { get; set; } = false;
    }
}
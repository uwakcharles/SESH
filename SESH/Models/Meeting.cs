using System;
using System.ComponentModel.DataAnnotations;
using SESH.Models.Enums;

namespace SESH.Models
{
    public class Meeting : BaseModel
    {
        [Required, StringLength(100)]
        public string Title { get; set; } = string.Empty;

        [StringLength(500)]
        public string Description { get; set; } = string.Empty;

        [Required]
        public DateTime ScheduledAt { get; set; }

        [Required]
        public MeetingStatus Status { get; set; } = MeetingStatus.Scheduled;

        // Foreign keys for the two participants
        public int BookedById { get; set; }
        public int BookedWithId { get; set; }

        // Navigation properties
        public virtual User BookedBy { get; set; } = null!;
        public virtual User BookedWith { get; set; } = null!;
    }
}
using System.ComponentModel.DataAnnotations;
using SESH.Models.Enums;

namespace SESH.Models
{
    public class WellBeingReport : BaseModel
    {
        [Required]
        public int StudentId { get; set; }
        public virtual Student Student { get; set; } = null!;

        [Required]
        public ReportStatus Status { get; set; }

        [StringLength(500, ErrorMessage = "Notes cannot exceed 500 characters")]
        public string Notes { get; set; } = string.Empty;

        public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;
    }
}
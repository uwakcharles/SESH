using System.ComponentModel.DataAnnotations;

namespace SESH.Models
{
    /// <summary>
    /// Represents the base model for all entities in the SESH application.
    /// </summary>
    public abstract class BaseModel
    {
        /// <summary>
        /// Gets or sets the unique identifier for the entity.
        /// </summary>
        [Key]
        public int Id { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; }
    }
}
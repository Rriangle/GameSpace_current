using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GameSpace.Models
{
    [Table("ManagerData")]
    public class ManagerData
    {
        [Key]
        public int ManagerId { get; set; }

        [Required]
        [StringLength(50)]
        public string ManagerAccount { get; set; } = string.Empty;

        [Required]
        [StringLength(255)]
        public string ManagerPassword { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string Email { get; set; } = string.Empty;

        public bool EmailConfirmed { get; set; } = false;

        public int AccessFailedCount { get; set; } = 0;

        public bool LockoutEnabled { get; set; } = false;

        public DateTime? LockoutEnd { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Required]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
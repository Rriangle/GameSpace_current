using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GameSpace.Models
{
    [Table("UserSalesInformation")]
    public class UserSalesInformation
    {
        [Key]
        public int SalesInfoId { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        [StringLength(100)]
        public string SalesName { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        public string SalesPhone { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string SalesEmail { get; set; } = string.Empty;

        [Required]
        [StringLength(200)]
        public string SalesAddress { get; set; } = string.Empty;

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Required]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // 導航屬性
        [ForeignKey("UserId")]
        public virtual User User { get; set; } = null!;
    }
}
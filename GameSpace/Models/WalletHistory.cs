using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GameSpace.Models
{
    [Table("WalletHistory")]
    public class WalletHistory
    {
        [Key]
        [Column("LogID")]
        public int LogID { get; set; }

        [Required]
        [Column("UserID")]
        public int UserID { get; set; }

        [Required]
        [StringLength(20)]
        public string ChangeType { get; set; } = string.Empty; // "Point", "Coupon", "EVoucher"

        [Required]
        public int PointsChanged { get; set; } = 0;

        [StringLength(100)]
        public string? ItemCode { get; set; }

        [Required]
        [StringLength(200)]
        public string Description { get; set; } = string.Empty;

        [Required]
        public DateTime ChangeTime { get; set; } = DateTime.UtcNow;

        // 導航屬性
        [ForeignKey("UserID")]
        public virtual User User { get; set; } = null!;
    }
}
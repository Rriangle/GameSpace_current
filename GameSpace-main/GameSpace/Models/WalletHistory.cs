using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GameSpace.Models
{
    [Table("WalletHistory")]
    public class WalletHistory
    {
        [Key]
        public int HistoryId { get; set; }

        [Required]
        public int UserId { get; set; }

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
        [ForeignKey("UserId")]
        public virtual User User { get; set; } = null!;
    }
}
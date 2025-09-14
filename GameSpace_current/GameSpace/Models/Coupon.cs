using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GameSpace.Models
{
    [Table("Coupon")]
    public class Coupon
    {
        [Key]
        public int CouponId { get; set; }

        [Required]
        [StringLength(50)]
        public string CouponCode { get; set; } = string.Empty;

        [Required]
        public int CouponTypeId { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        public bool IsUsed { get; set; } = false;

        [Required]
        public DateTime AcquiredTime { get; set; } = DateTime.UtcNow;

        public DateTime? UsedTime { get; set; }

        public int? UsedInOrderId { get; set; }

        // 導航屬性
        [ForeignKey("CouponTypeId")]
        public virtual CouponType CouponType { get; set; } = null!;
        [ForeignKey("UserId")]
        public virtual User User { get; set; } = null!;
    }
}
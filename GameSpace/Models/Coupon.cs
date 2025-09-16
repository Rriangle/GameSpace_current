using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GameSpace.Models
{
    [Table("Coupon")]
    public class Coupon
    {
        [Key]
        [Column("CouponID")]
        public int CouponID { get; set; }

        [Required]
        [StringLength(20)]
        [Column("CouponCode", TypeName = "varchar(20)")]
        public string CouponCode { get; set; } = string.Empty;

        [Required]
        [Column("CouponTypeID")]
        public int CouponTypeID { get; set; }

        [Required]
        [Column("UserID")]
        public int UserID { get; set; }

        [Required]
        [Column("IsUsed", TypeName = "bit")]
        public bool IsUsed { get; set; } = false;

        [Required]
        [Column("AcquiredTime", TypeName = "datetime2")]
        public DateTime AcquiredTime { get; set; } = DateTime.UtcNow;

        [Column("UsedTime", TypeName = "datetime2")]
        public DateTime? UsedTime { get; set; }

        public int? UsedInOrderId { get; set; }

        // 導航屬性
        [ForeignKey("CouponTypeID")]
        public virtual CouponType CouponType { get; set; } = null!;
        [ForeignKey("UserID")]
        public virtual User User { get; set; } = null!;
    }
}
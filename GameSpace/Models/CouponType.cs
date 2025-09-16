using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GameSpace.Models
{
    [Table("CouponType")]
    public class CouponType
    {
        [Key]
        [Column("CouponTypeID")]
        public int CouponTypeID { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        public string DiscountType { get; set; } = string.Empty; // "Amount" or "Percent"

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal DiscountValue { get; set; } = 0;

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal MinSpend { get; set; } = 0;

        [Required]
        public DateTime ValidFrom { get; set; } = DateTime.UtcNow;

        [Required]
        public DateTime ValidTo { get; set; } = DateTime.UtcNow.AddDays(30);

        [Required]
        public int PointsCost { get; set; } = 0;

        [StringLength(500)]
        public string? Description { get; set; }

        // 導航屬性
        public virtual ICollection<Coupon> Coupons { get; set; } = new List<Coupon>();
    }
}
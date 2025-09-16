using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GameSpace.Models
{
    [Table("UserSignInStats")]
    public class UserSignInStats
    {
        [Key]
        [Column("LogID")]
        public int LogID { get; set; }

        [Required]
        [Column("SignTime")]
        public DateTime SignTime { get; set; } = DateTime.UtcNow;

        [Required]
        [Column("UserID")]
        public int UserID { get; set; }

        [Required]
        [Column("PointsGained")]
        public int PointsGained { get; set; } = 0;

        [Required]
        [Column("PointsGainedTime")]
        public DateTime PointsGainedTime { get; set; } = DateTime.UtcNow;

        [Required]
        [Column("ExpGained")]
        public int ExpGained { get; set; } = 0;

        [Required]
        [Column("ExpGainedTime")]
        public DateTime ExpGainedTime { get; set; } = DateTime.UtcNow;

        [Required]
        [Column("CouponGained")]
        [StringLength(50)]
        public string CouponGained { get; set; } = "0";

        [Required]
        [Column("CouponGainedTime")]
        public DateTime CouponGainedTime { get; set; } = DateTime.UtcNow;

        // 導航屬性
        [ForeignKey("UserID")]
        public virtual User User { get; set; } = null!;
    }
}
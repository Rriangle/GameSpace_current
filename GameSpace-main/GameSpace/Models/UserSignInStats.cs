using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GameSpace.Models
{
    [Table("UserSignInStats")]
    public class UserSignInStats
    {
        [Key]
        public int LogId { get; set; }

        [Required]
        public DateTime SignTime { get; set; } = DateTime.UtcNow;

        [Required]
        public int UserId { get; set; }

        [Required]
        public int PointsChanged { get; set; } = 0;

        [Required]
        public DateTime PointsChangedTime { get; set; } = DateTime.UtcNow;

        [Required]
        public int ExpGained { get; set; } = 0;

        [Required]
        public DateTime ExpGainedTime { get; set; } = DateTime.UtcNow;

        [Required]
        public int CouponGained { get; set; } = 0;

        [Required]
        public DateTime CouponGainedTime { get; set; } = DateTime.UtcNow;

        // 導航屬性
        [ForeignKey("UserId")]
        public virtual User User { get; set; } = null!;
    }
}
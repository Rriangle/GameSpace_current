using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GameSpace.Models
{
    [Table("MiniGame")]
    public class MiniGame
    {
        [Key]
        [Column("PlayID")]
        public int PlayID { get; set; }

        [Required]
        [Column("UserID")]
        public int UserID { get; set; }

        [Required]
        [Column("PetID")]
        public int PetID { get; set; }

        [Required]
        public int Level { get; set; } = 0;

        [Required]
        public int MonsterCount { get; set; } = 0;

        [Required]
        [Column(TypeName = "decimal(5,2)")]
        public decimal SpeedMultiplier { get; set; } = 1.00m;

        [Required]
        [StringLength(10)]
        public string Result { get; set; } = "Unknown";

        [Required]
        [Column("ExpGained")]
        public int ExpGained { get; set; } = 0;

        [Required]
        [Column("ExpGainedTime")]
        public DateTime ExpGainedTime { get; set; } = DateTime.UtcNow;

        [Required]
        [Column("PointsGained")]
        public int PointsGained { get; set; } = 0;

        [Required]
        [Column("PointsGainedTime")]
        public DateTime PointsGainedTime { get; set; } = DateTime.UtcNow;

        [Required]
        [Column("CouponGained")]
        [StringLength(50)]
        public string CouponGained { get; set; } = "0";

        [Required]
        [Column("CouponGainedTime")]
        public DateTime CouponGainedTime { get; set; } = DateTime.UtcNow;

        [Required]
        [Column("HungerDelta")]
        public int HungerDelta { get; set; } = 0;

        [Required]
        [Column("MoodDelta")]
        public int MoodDelta { get; set; } = 0;

        [Required]
        [Column("StaminaDelta")]
        public int StaminaDelta { get; set; } = 0;

        [Required]
        [Column("CleanlinessDelta")]
        public int CleanlinessDelta { get; set; } = 0;

        [Required]
        [Column("StartTime")]
        public DateTime StartTime { get; set; } = DateTime.UtcNow;

        [Column("EndTime")]
        public DateTime? EndTime { get; set; }

        [Required]
        [Column("Aborted")]
        public bool Aborted { get; set; } = false;

        // 導航屬性
        [ForeignKey("UserId")]
        public virtual User User { get; set; } = null!;
        [ForeignKey("PetId")]
        public virtual Pet Pet { get; set; } = null!;
    }
}
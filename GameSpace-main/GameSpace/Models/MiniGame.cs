using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GameSpace.Models
{
    [Table("MiniGame")]
    public class MiniGame
    {
        [Key]
        public int PlayId { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        public int PetId { get; set; }

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
        public int ExpGained { get; set; } = 0;

        public DateTime? ExpGainedTime { get; set; }

        [Required]
        public int PointsChanged { get; set; } = 0;

        public DateTime? PointsChangedTime { get; set; }

        [Required]
        public int CouponGained { get; set; } = 0;

        [Required]
        public DateTime CouponGainedTime { get; set; } = DateTime.UtcNow;

        [Required]
        public int HungerDelta { get; set; } = 0;

        [Required]
        public int MoodDelta { get; set; } = 0;

        [Required]
        public int StaminaDelta { get; set; } = 0;

        [Required]
        public int CleanlinessDelta { get; set; } = 0;

        [Required]
        public DateTime StartTime { get; set; } = DateTime.UtcNow;

        public DateTime? EndTime { get; set; }

        [Required]
        public bool Aborted { get; set; } = false;

        // 導航屬性
        [ForeignKey("UserId")]
        public virtual User User { get; set; } = null!;
        [ForeignKey("PetId")]
        public virtual Pet Pet { get; set; } = null!;
    }
}
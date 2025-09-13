using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GameSpace.Models
{
    [Table("Pet")]
    public class Pet
    {
        [Key]
        [Column("PetID")]
        public int PetId { get; set; }

        [Required]
        [Column("UserID")]
        public int UserId { get; set; }

        [Required]
        [StringLength(50)]
        public string PetName { get; set; } = "小可愛";

        [Required]
        public int Level { get; set; } = 0;

        [Required]
        public DateTime LevelUpTime { get; set; } = DateTime.UtcNow;

        [Required]
        public int Experience { get; set; } = 0;

        [Required]
        [Range(0, 100)]
        public int Hunger { get; set; } = 0;

        [Required]
        [Range(0, 100)]
        public int Mood { get; set; } = 0;

        [Required]
        [Range(0, 100)]
        public int Stamina { get; set; } = 0;

        [Required]
        [Range(0, 100)]
        public int Cleanliness { get; set; } = 0;

        [Required]
        [Range(0, 100)]
        public int Health { get; set; } = 0;

        [Required]
        [StringLength(50)]
        public string SkinColor { get; set; } = "#ADD8E6";

        [Required]
        [Column("SkinColorChangedTime")]
        public DateTime ColorChangedTime { get; set; } = DateTime.UtcNow;

        [Required]
        [StringLength(50)]
        public string BackgroundColor { get; set; } = "粉藍";

        [Required]
        public DateTime BackgroundColorChangedTime { get; set; } = DateTime.UtcNow;

        [Required]
        [Column("PointsChanged_SkinColor")]
        public int PointsChangedColor { get; set; } = 0;

        [Required]
        public DateTime PointsChangedTimeColor { get; set; } = DateTime.UtcNow;

        [Required]
        [Column("PointsGained_LevelUp")]
        public int PointsGainedLevelUp { get; set; } = 0;

        [Required]
        public DateTime PointsGainedTimeLevelUp { get; set; } = DateTime.UtcNow;

        // 導航屬性
        [ForeignKey("UserId")]
        public virtual User User { get; set; } = null!;
        public virtual ICollection<MiniGame> MiniGames { get; set; } = new List<MiniGame>();
    }
}
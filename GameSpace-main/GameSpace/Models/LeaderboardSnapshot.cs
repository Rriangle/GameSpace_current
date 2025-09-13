using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GameSpace.Models
{
    [Table("LeaderboardSnapshots")]
    public class LeaderboardSnapshot
    {
        [Key]
        public int SnapshotId { get; set; }

        [Required]
        [StringLength(20)]
        public string Period { get; set; } = string.Empty; // "daily", "weekly", "monthly"

        [Required]
        public DateTime Ts { get; set; } = DateTime.UtcNow;

        [Required]
        public int Rank { get; set; } = 0;

        [Required]
        public int GameId { get; set; }

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal IndexValue { get; set; } = 0;

        // 導航屬性
        [ForeignKey("GameId")]
        public virtual Game Game { get; set; } = null!;
    }
}
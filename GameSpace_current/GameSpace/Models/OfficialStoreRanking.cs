using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GameSpace.Models
{
    [Table("Official_Store_Ranking")]
    public class OfficialStoreRanking
    {
        [Key]
        public int RankingId { get; set; }

        [Required]
        [StringLength(20)]
        public string PeriodType { get; set; } = string.Empty; // "日", "月", "季", "年"

        [Required]
        public DateTime RankingDate { get; set; } = DateTime.UtcNow;

        [Required]
        public int ProductId { get; set; }

        [Required]
        [StringLength(50)]
        public string RankingMetric { get; set; } = string.Empty; // "交易額", "交易量"

        [Required]
        public int RankingPosition { get; set; } = 0;

        [Required]
        [Column(TypeName = "decimal(15,2)")]
        public decimal TradingAmount { get; set; } = 0;

        [Required]
        public int TradingVolume { get; set; } = 0;

        [Required]
        public DateTime RankingUpdatedAt { get; set; } = DateTime.UtcNow;

        // 導航屬性
        [ForeignKey("ProductId")]
        public virtual ProductInfo ProductInfo { get; set; } = null!;
    }
}
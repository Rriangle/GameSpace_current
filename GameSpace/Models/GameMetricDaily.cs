using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GameSpace.Models
{
    [Table("GameMetricDaily")]
    public class GameMetricDaily
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int GameId { get; set; }

        [Required]
        public int MetricId { get; set; }

        [Required]
        public DateTime Date { get; set; } = DateTime.UtcNow.Date;

        [Required]
        [Column(TypeName = "decimal(15,2)")]
        public decimal Value { get; set; } = 0;

        [Required]
        [StringLength(20)]
        public string AggMethod { get; set; } = string.Empty; // "max", "avg", "sum"

        // 導航屬性
        [ForeignKey("GameId")]
        public virtual Game Game { get; set; } = null!;
        [ForeignKey("MetricId")]
        public virtual Metric Metric { get; set; } = null!;
    }
}
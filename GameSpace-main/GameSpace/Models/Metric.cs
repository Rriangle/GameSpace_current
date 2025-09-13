using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GameSpace.Models
{
    [Table("Metrics")]
    public class Metric
    {
        [Key]
        public int MetricId { get; set; }

        [Required]
        public int SourceId { get; set; }

        [Required]
        [StringLength(50)]
        public string Code { get; set; } = string.Empty;

        [StringLength(20)]
        public string? Unit { get; set; }

        [StringLength(200)]
        public string? Description { get; set; }

        // 導航屬性
        [ForeignKey("SourceId")]
        public virtual MetricSource MetricSource { get; set; } = null!;
        public virtual ICollection<GameMetricDaily> GameMetricDailies { get; set; } = new List<GameMetricDaily>();
    }
}
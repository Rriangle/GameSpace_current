using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GameSpace.Models
{
    [Table("MetricSources")]
    public class MetricSource
    {
        [Key]
        public int SourceId { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Note { get; set; }

        // 導航屬性
        public virtual ICollection<Metric> Metrics { get; set; } = new List<Metric>();
    }
}
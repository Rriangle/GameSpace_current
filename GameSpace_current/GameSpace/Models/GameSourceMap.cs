using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GameSpace.Models
{
    [Table("GameSourceMap")]
    public class GameSourceMap
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int GameId { get; set; }

        [Required]
        public int SourceId { get; set; }

        [Required]
        [StringLength(100)]
        public string ExternalKey { get; set; } = string.Empty;

        // 導航屬性
        [ForeignKey("GameId")]
        public virtual Game Game { get; set; } = null!;
        [ForeignKey("SourceId")]
        public virtual MetricSource MetricSource { get; set; } = null!;
    }
}
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GameSpace.Models
{
    [Table("Games")]
    public class Game
    {
        [Key]
        public int GameId { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [StringLength(100)]
        public string? NameZh { get; set; }

        [StringLength(50)]
        public string? Genre { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // 導航屬性
        public virtual ICollection<Forum> Forums { get; set; } = new List<Forum>();
    }
}
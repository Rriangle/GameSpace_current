using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GameSpace.Models
{
    [Table("Forums")]
    public class Forum
    {
        [Key]
        public int ForumId { get; set; }

        [Required]
        public int GameId { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // 導航屬性
        public virtual ICollection<Thread> Threads { get; set; } = new List<Thread>();
    }
}
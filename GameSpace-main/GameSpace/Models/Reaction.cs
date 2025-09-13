using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GameSpace.Models
{
    [Table("Reactions")]
    public class Reaction
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        [StringLength(20)]
        public string TargetType { get; set; } = string.Empty; // "post", "thread", "thread_post"

        [Required]
        public int TargetId { get; set; }

        [Required]
        [StringLength(20)]
        public string Kind { get; set; } = "like";

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // 導航屬性
        [ForeignKey("UserId")]
        public virtual User User { get; set; } = null!;
    }
}
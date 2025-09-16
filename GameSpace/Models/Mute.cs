using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GameSpace.Models
{
    [Table("Mutes")]
    public class Mute
    {
        [Key]
        public int MuteId { get; set; }

        [Required]
        [StringLength(50)]
        public string MuteName { get; set; } = string.Empty;

        [Required]
        public int DurationMinutes { get; set; } = 0;

        [StringLength(200)]
        public string? Description { get; set; }
    }
}
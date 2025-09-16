using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GameSpace.Models
{
    [Table("banned_words")]
    public class BannedWord
    {
        [Key]
        public int WordId { get; set; }

        [StringLength(50)]
        public string? Word { get; set; }

        public DateTime? CreatedAt { get; set; }
    }
}
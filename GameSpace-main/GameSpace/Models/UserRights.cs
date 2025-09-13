using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GameSpace.Models
{
    [Table("User_Rights")]
    public class UserRights
    {
        [Key]
        public int UserId { get; set; }

        [Required]
        public bool UserStatus { get; set; } = true;

        [Required]
        public bool ShoppingPermission { get; set; } = true;

        [Required]
        public bool MessagePermission { get; set; } = true;

        [Required]
        public bool SalesAuthority { get; set; } = false;

        // 導航屬性
        [ForeignKey("UserId")]
        public virtual User User { get; set; } = null!;
    }
}
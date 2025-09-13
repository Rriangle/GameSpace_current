using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GameSpace.Models
{
    [Table("User_Wallet")]
    public class UserWallet
    {
        [Key]
        public int UserId { get; set; }

        [Required]
        public int UserPoint { get; set; } = 0;

        // 導航屬性
        [ForeignKey("UserId")]
        public virtual User User { get; set; } = null!;
    }
}
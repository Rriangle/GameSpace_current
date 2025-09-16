using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GameSpace.Models
{
    [Table("User_Wallet")]
    public class UserWallet
    {
        [Key]
        [Column("User_Id")]
        public int UserID { get; set; }

        [Required]
        [Column("User_Point")]
        public int UserPoint { get; set; } = 0;

        // 導航屬性
        [ForeignKey("UserID")]
        public virtual User User { get; set; } = null!;
    }
}
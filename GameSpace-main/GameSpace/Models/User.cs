using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GameSpace.Models
{
    [Table("Users")]
    public class User
    {
        [Key]
        [Column("User_ID")]
        public int UserId { get; set; }

        [Required]
        [StringLength(30)]
        [Column("User_name")]
        public string UserName { get; set; } = string.Empty;

        [Required]
        [StringLength(30)]
        [Column("User_Account")]
        public string UserAccount { get; set; } = string.Empty;

        [Required]
        [StringLength(30)]
        [Column("User_Password")]
        public string UserPassword { get; set; } = string.Empty;

        [StringLength(100)]
        public string? UserEmail { get; set; }

        [StringLength(20)]
        public string? UserPhone { get; set; }

        [Column("User_EmailConfirmed")]
        public bool UserEmailConfirmed { get; set; } = false;

        [Column("User_PhoneNumberConfirmed")]
        public bool UserPhoneConfirmed { get; set; } = false;

        [Column("User_AccessFailedCount")]
        public int UserAccessFailedCount { get; set; } = 0;

        [Column("User_LockoutEnabled")]
        public bool UserLockoutEnabled { get; set; } = false;

        [Column("User_LockoutEnd")]
        public DateTime? UserLockoutEnd { get; set; }

        [Column("User_TwoFactorEnabled")]
        public bool UserTwoFactorEnabled { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // 導航屬性
        public virtual UserIntroduce? UserIntroduce { get; set; }
        public virtual UserRights? UserRights { get; set; }
        public virtual UserWallet? UserWallet { get; set; }
        public virtual ICollection<Pet> Pets { get; set; } = new List<Pet>();
        public virtual ICollection<MiniGame> MiniGames { get; set; } = new List<MiniGame>();
        public virtual ICollection<Coupon> Coupons { get; set; } = new List<Coupon>();
        public virtual ICollection<EVoucher> EVouchers { get; set; } = new List<EVoucher>();
        public virtual ICollection<WalletHistory> WalletHistories { get; set; } = new List<WalletHistory>();
        public virtual ICollection<UserSignInStats> UserSignInStats { get; set; } = new List<UserSignInStats>();
    }
}
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GameSpace.Models
{
    [Table("PlayerMarketProductInfo")]
    public class PlayerMarketProductInfo
    {
        [Key]
        public int PProductId { get; set; }

        [Required]
        [StringLength(50)]
        public string PProductType { get; set; } = string.Empty;

        [Required]
        [StringLength(200)]
        public string PProductTitle { get; set; } = string.Empty;

        [Required]
        [StringLength(200)]
        public string PProductName { get; set; } = string.Empty;

        [Required]
        public string PProductDescription { get; set; } = string.Empty;

        public int? ProductId { get; set; }

        [Required]
        public int SellerId { get; set; }

        [Required]
        [StringLength(20)]
        public string PStatus { get; set; } = "上架中";

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal Price { get; set; } = 0;

        public int? PProductImgId { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Required]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // 導航屬性
        [ForeignKey("SellerId")]
        public virtual User Seller { get; set; } = null!;
        [ForeignKey("ProductId")]
        public virtual ProductInfo? ProductInfo { get; set; }
        public virtual ICollection<PlayerMarketProductImg> PlayerMarketProductImgs { get; set; } = new List<PlayerMarketProductImg>();
    }
}
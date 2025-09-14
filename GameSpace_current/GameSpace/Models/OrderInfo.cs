using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GameSpace.Models
{
    [Table("OrderInfo")]
    public class OrderInfo
    {
        [Key]
        public int OrderId { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        public DateTime OrderDate { get; set; } = DateTime.UtcNow;

        [Required]
        [StringLength(20)]
        public string OrderStatus { get; set; } = "待付款";

        [Required]
        [StringLength(20)]
        public string PaymentStatus { get; set; } = "待付款";

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal OrderTotal { get; set; } = 0;

        public DateTime? PaymentAt { get; set; }

        public DateTime? ShippedAt { get; set; }

        public DateTime? CompletedAt { get; set; }

        // 導航屬性
        [ForeignKey("UserId")]
        public virtual User User { get; set; } = null!;
        public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    }
}
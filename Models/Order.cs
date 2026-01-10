using System.ComponentModel.DataAnnotations;

namespace OnlineShop.Models
{
    public class Order
    {
        [Key]
        public int Id { get; set; }

        public string UserId { get; set; }
        public virtual ApplicationUser User { get; set; }

        public DateTime Date { get; set; } = DateTime.UtcNow;
        
        public string Status { get; set; } = "Inregistrata"; // Inregistrata, Expediata, Livrata

        public decimal TotalAmount { get; set; }

        public virtual ICollection<OrderDetail> OrderDetails { get; set; }
    }
}

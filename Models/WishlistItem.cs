using System.ComponentModel.DataAnnotations;

namespace OnlineShop.Models
{
    public class WishlistItem
    {
        [Key]
        public int Id { get; set; }

        public int WishlistId { get; set; }
        public virtual Wishlist Wishlist { get; set; }

        public int ProductId { get; set; }
        public virtual Product Product { get; set; }

        public DateTime DateAdded { get; set; } = DateTime.UtcNow;
    }
}

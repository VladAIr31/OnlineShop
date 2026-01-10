using System.ComponentModel.DataAnnotations;

namespace OnlineShop.Models
{
    public class Wishlist
    {
        [Key]
        public int Id { get; set; }

        public string UserId { get; set; }
        public virtual ApplicationUser User { get; set; }

        public virtual ICollection<WishlistItem> WishlistItems { get; set; }
    }
}

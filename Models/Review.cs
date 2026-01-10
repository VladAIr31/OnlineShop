using System.ComponentModel.DataAnnotations;

namespace OnlineShop.Models
{
    public class Review
    {
        [Key]
        public int Id { get; set; }

        public string? Content { get; set; } // Textul este opțional

        [Range(1, 5, ErrorMessage = "Nota trebuie să fie între 1 și 5")]
        public int? Rating { get; set; } // Rating opțional (nullable)

        public DateTime Date { get; set; } = DateTime.UtcNow;

        // --- Relații ---

        public int ProductId { get; set; }
        public virtual Product? Product { get; set; }

        public string? UserId { get; set; } // Cine a lăsat review-ul
        public virtual ApplicationUser? User { get; set; }
    }
}
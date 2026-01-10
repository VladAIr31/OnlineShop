using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OnlineShop.Models
{
    public class ProductFaq
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Question { get; set; } = string.Empty;

        [Required]
        public string Answer { get; set; } = string.Empty;

        public DateTime Date { get; set; } = DateTime.UtcNow;

        // Relation to Product
        public int ProductId { get; set; }
        public virtual Product? Product { get; set; }
    }
}

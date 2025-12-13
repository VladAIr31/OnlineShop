using System.ComponentModel.DataAnnotations;

namespace OnlineShop.Models
{
    public class Category
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Numele categoriei este obligatoriu")]
        public string Name { get; set; } = string.Empty; // Initializare pentru fix CS8618

        // Relatie: O categorie are mai multe produse [cite: 15]
        public virtual ICollection<Product>? Products { get; set; }
    }
}
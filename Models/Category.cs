using System.ComponentModel.DataAnnotations;

namespace OnlineShop.Models
{
    public class Category
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Numele categoriei este obligatoriu")]
        public string Name { get; set; } = string.Empty;

        // Relatie: O categorie are mai multe produse
        public virtual ICollection<Product>? Products { get; set; }
    }
}
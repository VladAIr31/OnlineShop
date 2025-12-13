using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OnlineShop.Models
{
    public enum ProductStatus
    {
        Pending,  // În așteptare (propus de colaborator)
        Approved, // Aprobat (vizibil pe site)
        Rejected  // Respins
    }
    public class Product
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Titlul este obligatoriu")]
        public string Title { get; set; }=  string.Empty;

        [Required(ErrorMessage = "Descrierea este obligatorie")]
        public string Description { get; set; }= string.Empty;  

        [Required(ErrorMessage = "Imaginea este obligatorie")]
        public string ImagePath { get; set; } = string.Empty; // Se salvează calea către fișier

        [Required(ErrorMessage = "Prețul este obligatoriu")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Prețul trebuie să fie mai mare ca 0")]
        public decimal Price { get; set; }

        [Required(ErrorMessage = "Stocul este obligatoriu")]
        [Range(0, int.MaxValue, ErrorMessage = "Stocul trebuie să fie mai mare sau egal cu 0")]
        public int Stock { get; set; }

        //Rating calculat automat din media review-urilor 
        [NotMapped]
        public double Rating
        {
            get
            {
                if (Reviews == null || !Reviews.Any()) return 0;

                // Selectam doar rating-urile care nu sunt null
                var validRatings = Reviews
                                    .Where(r => r.Rating != null)
                                    .Select(r => (double)r.Rating!.Value); // Cast la double

                if (!validRatings.Any()) return 0;

                return Math.Round(validRatings.Average(), 1);
            }
        }

     // Starea produsului pentru fluxul de aprobare 
        public ProductStatus Status { get; set; } = ProductStatus.Pending;
// ID-ul utilizatorului (Colaborator) care a propus produsul [cite: 36]
        public string? UserId { get; set; }

        // Relație cu Categoria (FK) [cite: 15]
        [Required(ErrorMessage = "Selectarea categoriei este obligatorie")]
        public int CategoryId { get; set; }
        public virtual Category? Category { get; set; }

        // Relație cu Review-urile 
        public virtual ICollection<Review>? Reviews { get; set; }
    }
}
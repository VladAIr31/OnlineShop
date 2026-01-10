using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OnlineShop.Models
{
    public enum RequestStatus
    {
        Pending,
        Approved,
        Rejected,
        Revoked
    }

    public class CollaboratorRequest
    {
        [Key]
        public int Id { get; set; }

        public string UserId { get; set; }
        public virtual ApplicationUser User { get; set; }

        public DateTime RequestDate { get; set; } = DateTime.UtcNow;

        public RequestStatus Status { get; set; } = RequestStatus.Pending;

        public bool Seen { get; set; } = false;

        [Required(ErrorMessage = "Te rugam sa ne spui motivul pentru care doresti sa devii colaborator.")]
        [Display(Name = "Motiv")]
        public string Reason { get; set; }
    }
}

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Consilium.Domain.Models
{
    [Table("lawyer", Schema = "core")]
    public class Lawyer
    {
        [Key]
        [Column("lawyer_id")]
        public Guid ID { get; set; } // This is both PK and FK

        [Column("lawyer_professional_register")]
        [Required]
        [StringLength(20)]
        public string ProfessionalRegister { get; set; } = string.Empty;

        // Navigation property to link back to the User
        [ForeignKey("ID")]
        public User User { get; set; } = null!;
    }
}

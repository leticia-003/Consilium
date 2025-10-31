using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Consilium.Domain.Models
{
    [Table("Client")]
    public class Client
    {
        [Key]
        [Column("ID")]
        public Guid ID { get; set; } // This is both PK and FK

        [Column("nif")]
        public int NIF { get; set; }
        
        [Column("address")]
        public string? Address { get; set; }

        // Navigation property to link back to the User
        [ForeignKey("ID")]
        public User User { get; set; } = null!;
    }
}
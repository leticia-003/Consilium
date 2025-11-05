using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Consilium.Domain.Models
{
    [Table("client", Schema = "core")]
    public class Client
    {
        [Key]
        [Column("client_id")]
        public Guid ID { get; set; } // This is both PK and FK

        [Column("client_address")]
        public string Address { get; set; } = string.Empty;

        // Navigation property to link back to the User
        [ForeignKey("ID")]
        public User User { get; set; } = null!;
    }
}
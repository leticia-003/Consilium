using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Consilium.Domain.Models
{
    [Table("admin", Schema = "core")]
    public class Admin
    {
        [Key]
        [Column("admin_id")]
        public Guid ID { get; set; } // This is both PK and FK

        [Column("admin_started_at")]
        public DateTime StartedAt { get; set; } = DateTime.UtcNow;

        // Navigation property to link back to the User
        [ForeignKey("ID")]
        public User User { get; set; } = null!;
    }
}

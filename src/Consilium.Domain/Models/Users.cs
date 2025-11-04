using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Consilium.Domain.Models
{
    [Table("user", Schema = "core")]
    public class User
    {
        [Key]
        [Column("user_id")]
        public Guid ID { get; set; }

        [Column("user_name")]
        public string Name { get; set; } = string.Empty;

        [Column("user_nif")]
        public string NIF { get; set; } = string.Empty;

        [Column("user_email")]
        public string Email { get; set; } = string.Empty;

        [Column("user_password_hash")]
        public string PasswordHash { get; set; } = string.Empty;

        [Column("user_is_active")]
        public bool IsActive { get; set; } = true;

        // Navigation properties
        public Client? Client { get; set; }
        public ICollection<Phone> Phones { get; set; } = new List<Phone>();
    }
}

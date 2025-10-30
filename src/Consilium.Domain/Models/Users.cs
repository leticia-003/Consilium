using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Consilium.Domain.Enums; // We'll add this soon

namespace Consilium.Domain.Models
{
    [Table("User")] // Matches your SQL table name
    public class User
    {
        [Key]
        public Guid ID { get; set; }
        
        [Column("email")]
        public string Email { get; set; } = string.Empty;
        
        [Column("password_hash")]
        public string PasswordHash { get; set; } = string.Empty;
        
        [Column("Name")]
        public string Name { get; set; } = string.Empty;
        
        [Column("phone")]
        public int? Phone { get; set; }
        
        [Column("status")]
        public UserStatus Status { get; set; }
    }
}
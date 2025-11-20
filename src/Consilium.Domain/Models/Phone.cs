using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Consilium.Domain.Models
{
    [Table("phone", Schema = "core")]
    public class Phone
    {
        [Key]
        [Column("phone_id")]
        public Guid ID { get; set; }

        [Column("fk_user_id")]
        [Required]
        public Guid UserID { get; set; }

        [Column("phone_country_code")]
        [Required]
        public short CountryCode { get; set; } = 351;

        [Column("phone_number")]
        [Required]
        [StringLength(20)]
        public string Number { get; set; } = string.Empty;

        [Column("phone_is_main")]
        [Required]
        public bool IsMain { get; set; } = false;

        // Navigation property
        [ForeignKey("UserID")]
        public User User { get; set; } = null!;
    }
}

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Consilium.Domain.Models
{
    [Table("process_status", Schema = "legal")]
    public class ProcessStatus
    {
        [Column("process_status_id")]
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }


        [Column("process_status_name")]
        [Required]
        [StringLength(50)]
        public string Name { get; set; } = null!;


        [Column("process_status_is_final")]
        [Required]
        public bool IsFinal { get; set; } = false;


        [Column("process_status_is_default")]
        [Required]
        public bool IsDefault { get; set; } = false;


        [Column("process_status_is_active")]
        [Required]
        public bool IsActive { get; set; } = true;
    }
}
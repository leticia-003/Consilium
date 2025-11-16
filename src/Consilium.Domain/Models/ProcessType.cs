using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Consilium.Domain.Models
{
    [Table("PROCESS_TYPE", Schema = "LEGAL")]
    public class ProcessType
    {
        // PROCESS_TYPE_ID SERIAL NOT NULL
        [Column("process_type_id")]
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        // PROCESS_TYPE_NAME VARCHAR(255) NOT NULL
        [Column("process_type_name")]
        [StringLength(255)]
        [Required]
        public string Name { get; set; } = null!;

        // PROCESS_TYPE_IS_ACTIVE BOOLEAN NOT NULL DEFAULT TRUE
        [Column("process_type_is_active")]
        public bool IsActive { get; set; } = true;
    }
}
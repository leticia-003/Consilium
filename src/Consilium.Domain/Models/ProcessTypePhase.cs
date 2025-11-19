using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Consilium.Domain.Models
{
    [Table("process_type_phase", Schema = "legal")]
    public class ProcessTypePhase
    {
        [Column("process_type_phase_id")]
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Column("process_phase_id")]
        [Required]
        public int ProcessPhaseId { get; set; }

        [Column("process_type_id")]
        [Required]
        public int ProcessTypeId { get; set; }

        [Column("process_type_phase_order")]
        [Required]
        public short TypePhaseOrder { get; set; }

        [Column("process_type_phase_is_optional")]
        [Required]
        public bool IsOptional { get; set; } = false;

        [Column("process_type_phase_is_active")]
        [Required]
        public bool IsActive { get; set; } = true;

        // Navigation properties
        [ForeignKey("ProcessPhaseId")]
        public ProcessPhase ProcessPhase { get; set; } = null!;

        [ForeignKey("ProcessTypeId")]
        public ProcessType ProcessType { get; set; } = null!;

    }

}
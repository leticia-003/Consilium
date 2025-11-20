using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Consilium.Domain.Models
{
    [Table("process_phase", Schema = "legal")]
    public class ProcessPhase
    {
        [Column("process_phase_id")]
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Column("process_phase_name")]
        [StringLength(255)]
        [Required]
        public string Name { get; set; } = null!;

        [Column("process_phase_description")]
        public string? Description { get; set; }

        [Column("process_phase_is_active")]
        public bool IsActive { get; set; } = true;

        public ICollection<ProcessTypePhase> ProcessTypePhases { get; set; } = new List<ProcessTypePhase>();
    }
}
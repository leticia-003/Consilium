using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;


namespace Consilium.Domain.Models
{
    [Table("document_log", Schema = "legal")]
    public class DocumentLog
    {
        [Column("document_log_id")]
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public Guid Id { get; set; }

        [Column("document_id")]
        [Required]
        public Guid DocumentId { get; set; }

        [Column("updated_by")]
        [Required]
        public Guid UpdatedBy { get; set; }

        [Column("action_log_type_id")]
        [Required]
        public int ActionLogTypeId { get; set; }

        [Column("updated_at")]
        [Required]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        [Column("old_value")]
        public JsonElement OldValue { get; set; } = JsonDocument.Parse("{}").RootElement;

        [Column("new_value")]
        public JsonElement NewValue { get; set; } = JsonDocument.Parse("{}").RootElement;

        public Document? Document { get; set; }
        public User? UpdatedByUser { get; set; }
        public ActionLogType? ActionLogType { get; set; }

    }

}
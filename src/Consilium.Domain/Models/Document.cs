using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Consilium.Domain.Models
{
    [Table("document", Schema = "legal")]
    public class Document
    {
    [Column("document_id")]
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        [Column("process_id")]
        [Required]
        public Guid ProcessId { get; set; }

        [Column("file_name")]
        [StringLength(100)]
        [Required]
        public string FileName { get; set; } = null!;

        [Column("file")]
        [Required]
        public byte[] File { get; set; } = null!;

        [Column("file_mimetype")]
        [StringLength(50)]
        [Required]
        public string FileMimeType { get; set; } = null!;

        [Column("file_size")]
        [Required]
        public long FileSize { get; set; }

        [Column("created_at")]
        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // [Column("created_by")]
        // [Required]
        // public Guid CreatedBy { get; set; }

         public Process? Process { get; set; }
        //  public User? CreatedByUser { get; set; }

    }
}
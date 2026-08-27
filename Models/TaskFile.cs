using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProjectManagementSystem.Models
{
    public class TaskFile
    {
        public int TaskFileId { get; set; }

        public int TaskItemId { get; set; }

        [Required]
        public string UploadedByUserId { get; set; }

        [Required]
        [StringLength(255)]
        public string OriginalFileName { get; set; }

        [Required]
        [StringLength(255)]
        public string StoredFileName { get; set; }

        [Required]
        [StringLength(500)]
        public string FilePath { get; set; }

        public long FileSize { get; set; }

        [StringLength(100)]
        public string ContentType { get; set; }

        public DateTime UploadedDate { get; set; }

        [ForeignKey("TaskItemId")]
        public virtual TaskItem TaskItem { get; set; }

        [ForeignKey("UploadedByUserId")]
        public virtual ApplicationUser UploadedByUser { get; set; }
    }
}
using System.ComponentModel.DataAnnotations;

namespace FirstBloom.Models
{
    public class Notice
    {
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string Message { get; set; } = string.Empty;

        // Public or Personal
        [Required]
        [StringLength(20)]
        public string NoticeType { get; set; } = "Public";

        // Null for Public notices
        // Student UserId for Personal notices
        public string? UserId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime? PublishedAt { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
using System.ComponentModel.DataAnnotations;

namespace FirstBloom.Models
{
    public class Gallery
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Image title is required.")]
        public string Title { get; set; } = string.Empty;

        public string? ImageUrl { get; set; }

        public string? Category { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
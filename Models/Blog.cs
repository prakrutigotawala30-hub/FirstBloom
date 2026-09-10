using System.ComponentModel.DataAnnotations;

namespace FirstBloom.Models
{
    public class Blog
    {
        public int Id { get; set; }

        [Required]
        public string Title { get; set; }

        public string ShortDescription { get; set; }

        public string Description { get; set; }

        public string ImageUrl { get; set; }

        public string Author { get; set; } = "FirstBloom Team";

        public DateTime PublishedDate { get; set; } = DateTime.Now;

        public bool IsPublished { get; set; } = true;

        public string Category { get; set; } = "General";

        public string ReadTime { get; set; } = "3 min read";
    }
}
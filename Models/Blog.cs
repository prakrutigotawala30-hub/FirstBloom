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

        public string Author { get; set; }

        public DateTime PublishedDate { get; set; } = DateTime.Now;

        public bool IsPublished { get; set; } = true;
    }
}
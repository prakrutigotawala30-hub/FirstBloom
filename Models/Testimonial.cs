using System.ComponentModel.DataAnnotations;

namespace FirstBloom.Models
{
    public class Testimonial
    {
        public int Id { get; set; }

        [Required]
        public string ParentName { get; set; }

        public string ChildName { get; set; }

        public string Message { get; set; }

        public int Rating { get; set; }

        public string ParentImage { get; set; }
    }
}
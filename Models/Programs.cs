using System.ComponentModel.DataAnnotations;

namespace FirstBloom.Models
{
    public class Programs
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string ProgramName { get; set; }

        [Required]
        public string AgeGroup { get; set; }

        [Required]
        public string Duration { get; set; }

        public decimal Fee { get; set; }

        public string Description { get; set; }

        public string ImageUrl { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
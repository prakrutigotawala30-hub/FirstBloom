using System.ComponentModel.DataAnnotations;

namespace FirstBloom.Models
{
    public class FAQ
    {
        public int Id { get; set; }

        [Required]
        [StringLength(250)]
        public string Question { get; set; } = string.Empty;

        [Required]
        public string Answer { get; set; } = string.Empty;

        [StringLength(50)]
        public string Icon { get; set; } = "question-circle";

        [StringLength(20)]
        public string Color { get; set; } = "#7c3aed";

        public bool IsActive { get; set; } = true;
    }
}
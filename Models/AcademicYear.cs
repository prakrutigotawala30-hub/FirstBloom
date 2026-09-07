using System.ComponentModel.DataAnnotations;

namespace FirstBloom.Models
{
    public class AcademicYear
    {
        public int Id { get; set; }

        [Required]
        [StringLength(20)]
        [Display(Name = "Academic Year")]
        public string Name { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Start Date")]
        public DateTime StartDate { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "End Date")]
        public DateTime EndDate { get; set; }

        [Display(Name = "Current Year")]
        public bool IsCurrent { get; set; }

        [Display(Name = "Active")]
        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
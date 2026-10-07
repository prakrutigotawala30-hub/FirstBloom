using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FirstBloom.Models
{
    public class Programs
    {
        public int Id { get; set; }

        // =====================================================
        // PROGRAM INFORMATION
        // =====================================================

        [Required]
        [StringLength(100)]
        public string ProgramName { get; set; } = string.Empty;

        [Required]
        public string AgeGroup { get; set; } = string.Empty;

        [Required]
        public string Duration { get; set; } = string.Empty;

        public decimal Fee { get; set; }

        public string? Description { get; set; }

        public string? ImageUrl { get; set; }


        // =====================================================
        // ADMISSION START DATE
        // =====================================================

        [DataType(DataType.Date)]
        public DateTime? AdmissionStartDate { get; set; }


       

        [NotMapped]
        public DateTime StartDate
        {
            get => AdmissionStartDate ?? DateTime.Now;

            set => AdmissionStartDate = value;
        }


        // =====================================================
        // STATUS
        // =====================================================

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
using System.ComponentModel.DataAnnotations;

namespace FirstBloom.Models
{
    public class About
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Academy Name")]
        public string AcademyName { get; set; }

        [Required]
        [Display(Name = "About Title")]
        public string Title { get; set; }

        [Required]
        [Display(Name = "About Description")]
        public string Description { get; set; }

        [Display(Name = "Mission")]
        public string Mission { get; set; }

        [Display(Name = "Vision")]
        public string Vision { get; set; }

        [Display(Name = "Our Story")]
        public string Story { get; set; }

        [Display(Name = "Principal Message")]
        public string PrincipalMessage { get; set; }

        [Display(Name = "Principal Name")]
        public string PrincipalName { get; set; }

        [Display(Name = "About Image")]
        public string AboutImage { get; set; }

        [Display(Name = "Principal Image")]
        public string PrincipalImage { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
using System.ComponentModel.DataAnnotations;

namespace FirstBloom.Models.Student
{
    public class StudentLoginModel
    {
        [Required(ErrorMessage = "Please enter your email.")]
        [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
        [Display(Name = "Email Address")]
        public string Email { get; set; } = string.Empty;


        [Required(ErrorMessage = "Please enter your password.")]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; } = string.Empty;


        [Display(Name = "Remember Me")]
        public bool RememberMe { get; set; }
    }
}
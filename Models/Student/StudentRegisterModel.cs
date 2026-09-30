using System.ComponentModel.DataAnnotations;

namespace FirstBloom.Models.Student
{
    public class StudentRegisterModel
    {
        [Required(ErrorMessage = "Please enter your full name.")]
        [Display(Name = "Full Name")]
        [StringLength(150)]
        public string FullName { get; set; } = string.Empty;


        [Required(ErrorMessage = "Please enter your email.")]
        [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
        [Display(Name = "Email Address")]
        public string Email { get; set; } = string.Empty;


        [Required(ErrorMessage = "Please enter your mobile number.")]
        [Phone(ErrorMessage = "Please enter a valid mobile number.")]
        [Display(Name = "Mobile Number")]
        public string Mobile { get; set; } = string.Empty;


        [Required(ErrorMessage = "Please enter a password.")]
        [DataType(DataType.Password)]
        [StringLength(
            100,
            MinimumLength = 6,
            ErrorMessage = "Password must be at least 6 characters."
        )]
        [Display(Name = "Password")]
        public string Password { get; set; } = string.Empty;


        [Required(ErrorMessage = "Please confirm your password.")]
        [DataType(DataType.Password)]
        [Compare(
            "Password",
            ErrorMessage = "Password and confirm password do not match."
        )]
        [Display(Name = "Confirm Password")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
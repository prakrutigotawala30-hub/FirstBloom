
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace FirstBloom.ViewModels
{
    // ==========================================
    // STEP 1
    // ==========================================

    public class AdmissionStep1ViewModel
    {
        [Required]
        public string ChildFirstName { get; set; } = string.Empty;

        [Required]
        public string ChildLastName { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Date)]
        public DateTime DateOfBirth { get; set; }

        [Required]
        public string Gender { get; set; } = string.Empty;

        public string? BloodGroup { get; set; }

        public string? PreviousSchool { get; set; }
    }


    // ==========================================
    // STEP 2
    // ==========================================

    public class AdmissionStep2ViewModel
    {
        [Required]
        public string FatherName { get; set; } = string.Empty;

        public string? FatherOccupation { get; set; }

        public string? FatherPhone { get; set; }

        [Required]
        public string MotherName { get; set; } = string.Empty;

        public string? MotherOccupation { get; set; }

        public string? MotherPhone { get; set; }

        [EmailAddress]
        public string? ParentEmail { get; set; }
    }


    // ==========================================
    // STEP 3
    // ==========================================

    public class AdmissionStep3ViewModel
    {
        [Required]
        public string Address { get; set; } = string.Empty;

        [Required]
        public string City { get; set; } = string.Empty;

        [Required]
        public string State { get; set; } = string.Empty;

        [Required]
        public string Pincode { get; set; } = string.Empty;
    }


    // ==========================================
    // STEP 4 - PROGRAM
    // ==========================================

    public class AdmissionStep4ViewModel
    {
        [Required(ErrorMessage = "Please select a program.")]
        [Display(Name = "Program")]
        public string Program { get; set; } = string.Empty;


        // ---------------------------------------------------------
        // Automatically generated.
        // Student cannot edit.
        // ---------------------------------------------------------

        [Display(Name = "Academic Year")]
        public string AcademicYear { get; set; } = string.Empty;


        // ---------------------------------------------------------
        // Comes from selected program.
        // Student cannot edit.
        // ---------------------------------------------------------

        [Display(Name = "Program Start Date")]
        public string PreferredStartDate { get; set; } = string.Empty;
    }


    // ==========================================
    // STEP 5
    // ==========================================

    public class AdmissionStep5ViewModel
    {
        [Required]
        public IFormFile? BirthCertificate { get; set; }

        [Required]
        public IFormFile? ChildPhoto { get; set; }

        [Required]
        public IFormFile? AddressProof { get; set; }
    }


    // ==========================================
    // STEP 6
    // ==========================================

    public class AdmissionStep6ViewModel
    {
        public bool DeclarationAccepted { get; set; }

        public string? ParentSignature { get; set; }
    }
}
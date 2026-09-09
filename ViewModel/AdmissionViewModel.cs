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
    // STEP 4
    // ==========================================

    public class AdmissionStep4ViewModel
    {
        [Required]
        public string Program { get; set; } = string.Empty;

        public string? AcademicYear { get; set; }

        public string? PreferredStartDate { get; set; }

        public string? TransportRequired { get; set; }

        public string? DayCareRequired { get; set; }
    }


    // ==========================================
    // STEP 5
    // ==========================================

    public class AdmissionStep5ViewModel
    {
        public IFormFile? BirthCertificate { get; set; }

        public IFormFile? ChildPhoto { get; set; }

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
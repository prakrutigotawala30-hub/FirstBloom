using System.ComponentModel.DataAnnotations;

namespace FirstBloom.Models
{
    public enum AdmissionStatus
    {
        Draft = 0,
        Waiting = 1,
        Approved = 2,
        Rejected = 3
    }

    public class AdmissionApplication
    {
        public int Id { get; set; }

        [Required]
        public string ApplicationNumber { get; set; } = string.Empty;

        public AdmissionStatus Status { get; set; } = AdmissionStatus.Draft;

        public int CurrentStep { get; set; } = 1;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime? SubmittedAt { get; set; }

        public DateTime? ApprovedAt { get; set; }

        public string? RejectionReason { get; set; }


        public DateTime? RejectedAt { get; set; }


        // ==========================================
        // APPLICANT ACCOUNT
        // ==========================================

        public string? UserId { get; set; }

        public string? ApplicantEmail { get; set; }


        // ==========================================
        // STEP 1 - CHILD INFORMATION
        // ==========================================

        [Required]
        public string ChildFirstName { get; set; } = string.Empty;

        [Required]
        public string ChildLastName { get; set; } = string.Empty;

        [Required]
        public DateTime DateOfBirth { get; set; }

        [Required]
        public string Gender { get; set; } = string.Empty;

        public string? BloodGroup { get; set; }

        public string? PreviousSchool { get; set; }


        // ==========================================
        // STEP 2 - PARENT INFORMATION
        // ==========================================

        [Required]
        public string FatherName { get; set; } = string.Empty;

        public string? FatherOccupation { get; set; }

        public string? FatherPhone { get; set; }

        [Required]
        public string MotherName { get; set; } = string.Empty;

        public string? MotherOccupation { get; set; }

        public string? MotherPhone { get; set; }

        public string? ParentEmail { get; set; }


        // ==========================================
        // STEP 3 - ADDRESS
        // ==========================================

        [Required]
        public string Address { get; set; } = string.Empty;

        [Required]
        public string City { get; set; } = string.Empty;

        [Required]
        public string State { get; set; } = string.Empty;

        [Required]
        public string Pincode { get; set; } = string.Empty;


        // ==========================================
        // STEP 4 - PROGRAM
        // ==========================================

        [Required]
        public string Program { get; set; } = string.Empty;

        public string? AcademicYear { get; set; }

        public string? PreferredStartDate { get; set; }

        public string? TransportRequired { get; set; }

        public string? DayCareRequired { get; set; }


        // ==========================================
        // STEP 5 - DOCUMENTS
        // ==========================================

        public string? BirthCertificatePath { get; set; }

        public string? ChildPhotoPath { get; set; }

        public string? AddressProofPath { get; set; }


        // ==========================================
        // STEP 6 - DECLARATION
        // ==========================================

        public bool DeclarationAccepted { get; set; }

        public string? ParentSignature { get; set; }
    }
}
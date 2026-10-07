using System.ComponentModel.DataAnnotations;

namespace FirstBloom.Models.Student
{
    public class StudentProfile
    {
        public int Id { get; set; }


        // =====================================================
        // USER ACCOUNT
        // =====================================================

        [Required]
        public string UserId { get; set; } = string.Empty;


        // =====================================================
        // STUDENT INFORMATION
        // =====================================================

        [Required]
        public string FullName { get; set; } = string.Empty;

        public string? Email { get; set; }

        public string? Mobile { get; set; }

        public string? StudentId { get; set; }


        // =====================================================
        // ADMISSION
        // =====================================================

        public string AdmissionStatus { get; set; }
            = "NotStarted";

        public string? RejectionReason { get; set; }

        public int? AdmissionApplicationId { get; set; }

        public string? ApplicationNumber { get; set; }


        // =====================================================
        // STUDENT INFORMATION FROM ADMISSION
        // =====================================================

        public string? ChildFirstName { get; set; }

        public string? ChildLastName { get; set; }

        public DateTime? DateOfBirth { get; set; }

        public string? Gender { get; set; }

        public string? BloodGroup { get; set; }

        public string? PreviousSchool { get; set; }


        // =====================================================
        // PARENT INFORMATION
        // =====================================================

        public string? FatherName { get; set; }

        public string? FatherOccupation { get; set; }

        public string? FatherPhone { get; set; }

        public string? MotherName { get; set; }

        public string? MotherOccupation { get; set; }

        public string? MotherPhone { get; set; }

        public string? ParentEmail { get; set; }


        // =====================================================
        // ADDRESS
        // =====================================================

        public string? Address { get; set; }

        public string? City { get; set; }

        public string? State { get; set; }

        public string? Pincode { get; set; }


        // =====================================================
        // PROGRAM INFORMATION
        // =====================================================

        public string? Program { get; set; }

        public string? AcademicYear { get; set; }

        public string? PreferredStartDate { get; set; }

        public string? TransportRequired { get; set; }

        public string? DayCareRequired { get; set; }


        // =====================================================
        // DATES
        // =====================================================

        public DateTime CreatedAt { get; set; }
            = DateTime.Now;

        public DateTime? ApprovedAt { get; set; }
    }
}
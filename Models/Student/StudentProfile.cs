using System;
using System.ComponentModel.DataAnnotations;

namespace FirstBloom.Models.Student
{
    public class StudentProfile
    {
        public int Id { get; set; }

        // Identity user's Id
        [Required]
        public string UserId { get; set; } = string.Empty;

        // Generated only after admission approval
        [MaxLength(30)]
        public string? StudentId { get; set; }

        // Student/applicant name
        [Required]
        [MaxLength(150)]
        public string FullName { get; set; } = string.Empty;

        // Registered email
        [Required]
        [EmailAddress]
        [MaxLength(150)]
        public string Email { get; set; } = string.Empty;

        // Registered mobile
        [MaxLength(20)]
        public string? Mobile { get; set; }

        // Admission status
        [Required]
        [MaxLength(30)]
        public string AdmissionStatus { get; set; } = "NotStarted";

        // Rejection reason
        [MaxLength(1000)]
        public string? RejectionReason { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? ApprovedAt { get; set; }
    }
}
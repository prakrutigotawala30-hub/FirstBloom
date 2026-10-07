using FirstBloom.Data;
using FirstBloom.Models;
using FirstBloom.Models.Identity;
using FirstBloom.Models.Student;
using FirstBloom.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FirstBloom.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class AdmissionsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly EmailService _emailService;

        // =====================================================
        // CONSTRUCTOR
        // =====================================================

        public AdmissionsController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            EmailService emailService)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
            _emailService = emailService;
        }


        // =====================================================
        // INDEX
        // GET: /Admin/Admissions
        // =====================================================

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var applications =
                await _context.AdmissionApplications
                    .OrderByDescending(x => x.CreatedAt)
                    .ToListAsync();

            return View(applications);
        }


        // =====================================================
        // DETAILS
        // GET: /Admin/Admissions/Details/5
        // =====================================================

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var application =
                await _context.AdmissionApplications
                    .FirstOrDefaultAsync(x => x.Id == id);

            if (application == null)
            {
                return NotFound();
            }

            return View(application);
        }


        // =====================================================
        // APPROVE
        // =====================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Approve(int id)
        {
            var application =
                await _context.AdmissionApplications
                    .FirstOrDefaultAsync(x => x.Id == id);

            if (application == null)
            {
                return NotFound();
            }


            // =================================================
            // STATUS CHECK
            // =================================================

            if (application.Status == AdmissionStatus.Approved)
            {
                TempData["Error"] =
                    "This admission is already approved.";

                return RedirectToAction(
                    nameof(Details),
                    new { id });
            }


            if (application.Status == AdmissionStatus.Rejected)
            {
                TempData["Error"] =
                    "A rejected admission cannot be approved.";

                return RedirectToAction(
                    nameof(Details),
                    new { id });
            }


            if (application.Status != AdmissionStatus.Waiting)
            {
                TempData["Error"] =
                    "Only applications waiting for review can be approved.";

                return RedirectToAction(
                    nameof(Details),
                    new { id });
            }


            // =================================================
            // FIND USER
            // =================================================

            ApplicationUser? user = null;

            if (!string.IsNullOrWhiteSpace(application.UserId))
            {
                user =
                    await _userManager.FindByIdAsync(
                        application.UserId);
            }


            if (user == null &&
                !string.IsNullOrWhiteSpace(
                    application.ApplicantEmail))
            {
                user =
                    await _userManager.FindByEmailAsync(
                        application.ApplicantEmail);
            }


            if (user == null)
            {
                TempData["Error"] =
                    "Applicant account could not be found.";

                return RedirectToAction(
                    nameof(Details),
                    new { id });
            }


            // =================================================
            // CREATE STUDENT ROLE IF REQUIRED
            // =================================================

            if (!await _roleManager.RoleExistsAsync("Student"))
            {
                var roleResult =
                    await _roleManager.CreateAsync(
                        new IdentityRole("Student"));

                if (!roleResult.Succeeded)
                {
                    TempData["Error"] =
                        string.Join(
                            ", ",
                            roleResult.Errors.Select(
                                x => x.Description));

                    return RedirectToAction(
                        nameof(Details),
                        new { id });
                }
            }


            // =================================================
            // ADD STUDENT ROLE
            // =================================================

            if (!await _userManager.IsInRoleAsync(
                    user,
                    "Student"))
            {
                var roleResult =
                    await _userManager.AddToRoleAsync(
                        user,
                        "Student");

                if (!roleResult.Succeeded)
                {
                    TempData["Error"] =
                        string.Join(
                            ", ",
                            roleResult.Errors.Select(
                                x => x.Description));

                    return RedirectToAction(
                        nameof(Details),
                        new { id });
                }
            }


            // =================================================
            // GENERATE STUDENT ID
            // =================================================

            if (string.IsNullOrWhiteSpace(user.StudentId))
            {
                user.StudentId =
                    await GenerateStudentIdAsync();
            }


            // =================================================
            // UPDATE USER
            // =================================================

            user.StudentApprovedAt =
                DateTime.Now;


            // =================================================
            // UPDATE APPLICATION
            // =================================================

            application.UserId =
                user.Id;

            application.Status =
                AdmissionStatus.Approved;

            application.ApprovedAt =
                DateTime.Now;


            // =================================================
            // CREATE / UPDATE STUDENT PROFILE
            // =================================================

            var studentProfile =
                await _context.StudentProfiles
                    .FirstOrDefaultAsync(
                        x => x.UserId == user.Id);


            if (studentProfile == null)
            {
                studentProfile =
                    new StudentProfile
                    {
                        UserId = user.Id,
                        CreatedAt = DateTime.Now
                    };

                _context.StudentProfiles.Add(
                    studentProfile);
            }


            // =================================================
            // BASIC STUDENT INFORMATION
            // =================================================

            studentProfile.FullName =
                $"{application.ChildFirstName} {application.ChildLastName}"
                    .Trim();

            studentProfile.Email =
                user.Email;

            studentProfile.Mobile =
                application.FatherPhone
                ?? application.MotherPhone;

            studentProfile.StudentId =
                user.StudentId;

            studentProfile.AdmissionStatus =
                "Approved";

            studentProfile.AdmissionApplicationId =
                application.Id;

            studentProfile.ApplicationNumber =
                application.ApplicationNumber;

            studentProfile.ApprovedAt =
                application.ApprovedAt;


            // =================================================
            // CHILD INFORMATION
            // =================================================

            studentProfile.ChildFirstName =
                application.ChildFirstName;

            studentProfile.ChildLastName =
                application.ChildLastName;

            studentProfile.DateOfBirth =
                application.DateOfBirth;

            studentProfile.Gender =
                application.Gender;

            studentProfile.BloodGroup =
                application.BloodGroup;

            studentProfile.PreviousSchool =
                application.PreviousSchool;


            // =================================================
            // PARENT INFORMATION
            // =================================================

            studentProfile.FatherName =
                application.FatherName;

            studentProfile.FatherOccupation =
                application.FatherOccupation;

            studentProfile.FatherPhone =
                application.FatherPhone;

            studentProfile.MotherName =
                application.MotherName;

            studentProfile.MotherOccupation =
                application.MotherOccupation;

            studentProfile.MotherPhone =
                application.MotherPhone;

            studentProfile.ParentEmail =
                application.ParentEmail;


            // =================================================
            // ADDRESS
            // =================================================

            studentProfile.Address =
                application.Address;

            studentProfile.City =
                application.City;

            studentProfile.State =
                application.State;

            studentProfile.Pincode =
                application.Pincode;


            // =================================================
            // PROGRAM INFORMATION
            // =================================================

            studentProfile.Program =
                application.Program;

            studentProfile.AcademicYear =
                application.AcademicYear;

            studentProfile.PreferredStartDate =
                application.PreferredStartDate;


            // =================================================
            // TRANSPORT / DAY CARE
            //
            // These are no longer collected from the new
            // admission form.
            //
            // Keep existing database values empty.
            // =================================================

            studentProfile.TransportRequired = null;

            studentProfile.DayCareRequired = null;


            // =================================================
            // SAVE EVERYTHING
            // =================================================

            await _context.SaveChangesAsync();


            // =================================================
            // APPROVAL EMAIL
            // =================================================

            if (!string.IsNullOrWhiteSpace(user.Email))
            {
                try
                {
                    var safeName =
                        System.Net.WebUtility.HtmlEncode(
                            studentProfile.FullName);

                    var safeStudentId =
                        System.Net.WebUtility.HtmlEncode(
                            user.StudentId ?? "");

                    var dashboardUrl =
                        GetStudentDashboardUrl();

                    var emailBody = $@"
<!DOCTYPE html>
<html>
<head>
<meta charset=""UTF-8"">
<meta name=""viewport""
      content=""width=device-width, initial-scale=1.0"">
</head>

<body style=""
margin:0;
padding:0;
background:#eef5ff;
font-family:Arial,Helvetica,sans-serif;
"">

<div style=""
max-width:600px;
margin:40px auto;
background:#ffffff;
border-radius:16px;
overflow:hidden;
box-shadow:0 5px 20px rgba(0,0,0,0.10);
"">

<div style=""
background:#073b82;
padding:30px;
text-align:center;
"">

<div style=""
font-size:42px;
color:#f5bd19;
"">
✿
</div>

<h1 style=""
margin:5px 0;
color:#ffffff;
font-size:30px;
"">
FirstBloom
</h1>

<p style=""
margin:0;
color:#ffffff;
font-size:14px;
letter-spacing:2px;
"">
ACADEMY
</p>

</div>

<div style=""
padding:35px;
"">

<h2 style=""
color:#062f6b;
margin-top:0;
"">
Congratulations, {safeName}! 🎉
</h2>

<p style=""
color:#26364a;
font-size:16px;
line-height:1.6;
"">
We are pleased to inform you that your
admission application has been
<strong>approved</strong>.
</p>

<div style=""
background:#eef5ff;
border-radius:10px;
padding:20px;
margin:25px 0;
"">

<p style=""
margin:0 0 10px;
color:#68778a;
font-size:13px;
"">
YOUR STUDENT ID
</p>

<strong style=""
color:#073b82;
font-size:24px;
"">
{safeStudentId}
</strong>

</div>

<p style=""
color:#26364a;
font-size:16px;
line-height:1.6;
"">
Your account is now registered as a
<strong>Student Account</strong>.
</p>

<div style=""
text-align:center;
margin:30px 0;
"">

<a href=""{dashboardUrl}""
style=""
display:inline-block;
background:#f5bd19;
color:#062f6b;
padding:15px 30px;
text-decoration:none;
border-radius:8px;
font-size:16px;
font-weight:bold;
"">
Open Student Dashboard
</a>

</div>

</div>

<div style=""
background:#fff8df;
padding:20px;
text-align:center;
"">

<p style=""
margin:0;
color:#68778a;
font-size:13px;
"">
© {DateTime.Now.Year} FirstBloom Academy
</p>

</div>

</div>

</body>
</html>
";


                    await _emailService.SendEmailAsync(
                        user.Email,
                        "🎉 FirstBloom Academy Admission Approved",
                        emailBody);
                }
                catch
                {
                    TempData["Warning"] =
                        "Admission approved, but the approval email could not be sent.";
                }
            }


            // =================================================
            // SUCCESS MESSAGE
            // =================================================

            if (TempData["Warning"] == null)
            {
                TempData["Success"] =
                    $"Admission approved successfully. {user.Email} is now a Student.";
            }


            return RedirectToAction(
                nameof(Details),
                new { id });
        }


        // =====================================================
        // REJECT
        // =====================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reject(
            int id,
            string? rejectionReason)
        {
            var application =
                await _context.AdmissionApplications
                    .FirstOrDefaultAsync(x => x.Id == id);

            if (application == null)
            {
                return NotFound();
            }


            if (application.Status == AdmissionStatus.Rejected)
            {
                TempData["Error"] =
                    "This admission is already rejected.";

                return RedirectToAction(
                    nameof(Details),
                    new { id });
            }


            if (application.Status == AdmissionStatus.Approved)
            {
                TempData["Error"] =
                    "An approved admission cannot be rejected.";

                return RedirectToAction(
                    nameof(Details),
                    new { id });
            }


            if (application.Status != AdmissionStatus.Waiting)
            {
                TempData["Error"] =
                    "Only applications waiting for review can be rejected.";

                return RedirectToAction(
                    nameof(Details),
                    new { id });
            }


            // =================================================
            // FIND USER
            // =================================================

            ApplicationUser? user = null;

            if (!string.IsNullOrWhiteSpace(application.UserId))
            {
                user =
                    await _userManager.FindByIdAsync(
                        application.UserId);
            }


            if (user == null &&
                !string.IsNullOrWhiteSpace(
                    application.ApplicantEmail))
            {
                user =
                    await _userManager.FindByEmailAsync(
                        application.ApplicantEmail);
            }


            // =================================================
            // UPDATE APPLICATION
            // =================================================

            application.Status =
                AdmissionStatus.Rejected;

            application.RejectedAt =
                DateTime.Now;

            application.RejectionReason =
                string.IsNullOrWhiteSpace(
                    rejectionReason)
                    ? "Your admission application was not approved."
                    : rejectionReason.Trim();


            // =================================================
            // UPDATE STUDENT PROFILE
            // =================================================

            if (!string.IsNullOrWhiteSpace(
                    application.UserId))
            {
                var profile =
                    await _context.StudentProfiles
                        .FirstOrDefaultAsync(
                            x => x.UserId ==
                                 application.UserId);

                if (profile != null)
                {
                    profile.AdmissionStatus =
                        "Rejected";

                    profile.RejectionReason =
                        application.RejectionReason;
                }
            }


            await _context.SaveChangesAsync();


            // =================================================
            // SEND EMAIL
            // =================================================

            var email =
                user?.Email ??
                application.ApplicantEmail;


            if (!string.IsNullOrWhiteSpace(email))
            {
                try
                {
                    var safeName =
                        System.Net.WebUtility.HtmlEncode(
                            user?.FullName ?? "Applicant");

                    var safeReason =
                        System.Net.WebUtility.HtmlEncode(
                            application.RejectionReason ??
                            "Your admission application was not approved.");

                    var emailBody = $@"
<!DOCTYPE html>
<html>
<head>
<meta charset=""UTF-8"">
<meta name=""viewport""
content=""width=device-width, initial-scale=1.0"">
</head>

<body style=""
margin:0;
padding:0;
background:#eef5ff;
font-family:Arial,Helvetica,sans-serif;
"">

<div style=""
max-width:600px;
margin:40px auto;
background:#ffffff;
border-radius:16px;
overflow:hidden;
box-shadow:0 5px 20px rgba(0,0,0,0.10);
"">

<div style=""
background:#073b82;
padding:30px;
text-align:center;
"">

<div style=""
font-size:42px;
color:#f5bd19;
"">
✿
</div>

<h1 style=""
margin:5px 0;
color:#ffffff;
"">
FirstBloom
</h1>

<p style=""
margin:0;
color:#ffffff;
letter-spacing:2px;
"">
ACADEMY
</p>

</div>

<div style=""
padding:35px;
"">

<h2 style=""
color:#062f6b;
"">
Hello, {safeName}
</h2>

<p style=""
color:#26364a;
font-size:16px;
line-height:1.6;
"">
Thank you for applying to
<strong>FirstBloom Academy</strong>.
</p>

<p style=""
color:#26364a;
font-size:16px;
line-height:1.6;
"">
After reviewing your admission application,
we regret to inform you that it has not
been approved at this time.
</p>

<div style=""
background:#fff4f4;
border-left:4px solid #dc3545;
padding:18px;
margin:25px 0;
"">

<strong style=""
color:#842029;
"">
Reason
</strong>

<p style=""
color:#5c2b2f;
line-height:1.6;
"">
{safeReason}
</p>

</div>

<p style=""
color:#68778a;
font-size:14px;
line-height:1.6;
"">
You may log in to your account and submit
a new admission application if you wish.
</p>

</div>

<div style=""
background:#fff8df;
padding:20px;
text-align:center;
"">

<p style=""
margin:0;
color:#68778a;
font-size:13px;
"">
© {DateTime.Now.Year} FirstBloom Academy
</p>

</div>

</div>

</body>
</html>
";


                    await _emailService.SendEmailAsync(
                        email,
                        "FirstBloom Academy Admission Update",
                        emailBody);
                }
                catch
                {
                    TempData["Warning"] =
                        "Admission rejected, but the notification email could not be sent.";
                }
            }


            if (TempData["Warning"] == null)
            {
                TempData["Success"] =
                    "Admission rejected successfully.";
            }


            return RedirectToAction(
                nameof(Details),
                new { id });
        }


        // =====================================================
        // GENERATE STUDENT ID
        // =====================================================

        private async Task<string> GenerateStudentIdAsync()
        {
            string studentId;

            do
            {
                studentId =
                    "FB" +
                    DateTime.Now.Year +
                    Random.Shared.Next(
                        1000,
                        9999);
            }
            while (
                await _userManager.Users
                    .AnyAsync(
                        x => x.StudentId == studentId));

            return studentId;
        }


        // =====================================================
        // STUDENT DASHBOARD URL
        // =====================================================

        private string GetStudentDashboardUrl()
        {
            return Url.Action(
                "Index",
                "StudentDashboard",
                null,
                Request.Scheme)
                ?? "/";
        }
    }
}
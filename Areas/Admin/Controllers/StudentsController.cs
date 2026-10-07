
using FirstBloom.Data;
using FirstBloom.Models;
using FirstBloom.Models.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FirstBloom.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class StudentsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public StudentsController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }


        // =========================================================
        // STUDENT LIST
        // GET:
        // /Admin/Students
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var students =
                await (
                    from user in _context.Users

                    join application in _context.AdmissionApplications
                        on user.Id equals application.UserId
                        into applications

                    from application in applications
                        .OrderByDescending(x => x.CreatedAt)
                        .Take(1)
                        .DefaultIfEmpty()

                        // DO NOT use: where application != null

                    orderby user.FullName

                    select new StudentAdminListItem
                    {
                        UserId = user.Id,

                        // ================================
                        // ACCOUNT INFORMATION
                        // ================================

                        FullName = user.FullName,

                        Email = user.Email,

                        Mobile = user.PhoneNumber,

                        EmailConfirmed = user.EmailConfirmed,

                        // ================================
                        // ADMISSION INFORMATION
                        // ================================

                        ApplicationId =
                            application != null
                                ? application.Id
                                : 0,

                        ApplicationNumber =
                            application != null
                                ? application.ApplicationNumber
                                : "Not Started",

                        ChildName =
                            application != null
                                ? application.ChildFirstName + " " +
                                  application.ChildLastName
                                : "No application",

                        Program =
                            application != null
                                ? application.Program
                                : null,

                        Status =
                            application != null
                                ? application.Status
                                : AdmissionStatus.Draft,

                        SubmittedAt =
                            application != null
                                ? application.SubmittedAt
                                : null,

                        CreatedAt =
                            application != null
                                ? application.CreatedAt
                                : user.Id != null
                                    ? DateTime.Now
                                    : DateTime.Now
                    }
                ).ToListAsync();

            return View(students);
        }


        // =========================================================
        // STUDENT DETAILS
        // GET:
        // /Admin/Students/Details/5
        // =========================================================

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

            ApplicationUser? user = null;

            if (!string.IsNullOrWhiteSpace(
                application.UserId))
            {
                user =
                    await _userManager.FindByIdAsync(
                        application.UserId);
            }

            var model = new StudentAdminDetailsViewModel
            {
                User = user,
                Application = application
            };

            return View(model);
        }


        // =========================================================
        // APPROVE ADMISSION
        // POST:
        // /Admin/Students/Approve
        // =========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Approve(
            int id)
        {
            var application =
                await _context.AdmissionApplications
                    .FirstOrDefaultAsync(x => x.Id == id);

            if (application == null)
            {
                return NotFound();
            }

            // Already approved
            if (application.Status ==
                AdmissionStatus.Approved)
            {
                TempData["SuccessMessage"] =
                    "This admission is already approved.";

                return RedirectToAction(
                    nameof(Details),
                    new { id });
            }

            // Approve application
            application.Status =
                AdmissionStatus.Approved;

            application.ApprovedAt =
                DateTime.Now;

            application.RejectionReason = null;

            application.RejectedAt = null;

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] =
                "Admission approved successfully.";

            return RedirectToAction(
                nameof(Details),
                new { id });
        }


        // =========================================================
        // REJECT ADMISSION - GET
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> Reject(int id)
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


        // =========================================================
        // REJECT ADMISSION - POST
        // =========================================================

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

            if (string.IsNullOrWhiteSpace(
                rejectionReason))
            {
                ModelState.AddModelError(
                    "rejectionReason",
                    "Please provide a rejection reason.");

                return View(application);
            }

            application.Status =
                AdmissionStatus.Rejected;

            application.RejectionReason =
                rejectionReason.Trim();

            application.RejectedAt =
                DateTime.Now;

            application.ApprovedAt = null;

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] =
                "Admission rejected successfully.";

            return RedirectToAction(
                nameof(Details),
                new { id });
        }


        // =========================================================
        // SEND APPLICATION BACK TO WAITING
        // Useful if admin rejected by mistake
        // =========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SetWaiting(
            int id)
        {
            var application =
                await _context.AdmissionApplications
                    .FirstOrDefaultAsync(x => x.Id == id);

            if (application == null)
            {
                return NotFound();
            }

            application.Status =
                AdmissionStatus.Waiting;

            application.RejectionReason = null;

            application.RejectedAt = null;

            application.ApprovedAt = null;

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] =
                "Application moved back to waiting status.";

            return RedirectToAction(
                nameof(Details),
                new { id });
        }


        // =========================================================
        // DELETE STUDENT
        // =========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(
            string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                return BadRequest();
            }

            var user =
                await _userManager.FindByIdAsync(
                    userId);

            if (user == null)
            {
                TempData["ErrorMessage"] =
                    "Student account was not found.";

                return RedirectToAction(
                    nameof(Index));
            }

            // Delete all admission applications
            var applications =
                await _context.AdmissionApplications
                    .Where(x => x.UserId == userId)
                    .ToListAsync();

            if (applications.Any())
            {
                _context.AdmissionApplications.RemoveRange(
                    applications);
            }

            // Delete Identity account
            var deleteResult =
                await _userManager.DeleteAsync(user);

            if (!deleteResult.Succeeded)
            {
                foreach (var error in deleteResult.Errors)
                {
                    ModelState.AddModelError(
                        string.Empty,
                        error.Description);
                }

                return RedirectToAction(
                    nameof(Index));
            }

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] =
                "Student account and admission data deleted successfully.";

            return RedirectToAction(
                nameof(Index));
        }
    }


    // =============================================================
    // STUDENT LIST VIEW MODEL
    // =============================================================

    public class StudentAdminListItem
    {
        public string UserId { get; set; } = string.Empty;

        public string? FullName { get; set; }

        public string? Email { get; set; }

        public string? Mobile { get; set; }

        public bool EmailConfirmed { get; set; }

        public int ApplicationId { get; set; }

        public string ApplicationNumber { get; set; }
            = string.Empty;

        public string ChildName { get; set; }
            = string.Empty;

        public string? Program { get; set; }

        public AdmissionStatus Status { get; set; }

        public DateTime? SubmittedAt { get; set; }

        public DateTime CreatedAt { get; set; }
    }


    // =============================================================
    // STUDENT DETAILS VIEW MODEL
    // =============================================================

    public class StudentAdminDetailsViewModel
    {
        public ApplicationUser? User { get; set; }

        public AdmissionApplication Application { get; set; }
            = null!;
    }
}

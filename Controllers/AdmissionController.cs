
using FirstBloom.Data;
using FirstBloom.Models;
using FirstBloom.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace FirstBloom.Controllers
{
    public class AdmissionsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;

        private const string ApplicationSessionKey =
            "AdmissionApplicationId";


        // =========================================================
        // CONSTRUCTOR
        // =========================================================

        public AdmissionsController(
            ApplicationDbContext context,
            IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }


        // =========================================================
        // ADMISSION HOME
        // =========================================================

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }


        // =========================================================
        // APPLY NOW
        // =========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult StartApplication()
        {
            if (User.Identity == null ||
                !User.Identity.IsAuthenticated)
            {
                var returnUrl =
                    Url.Action(
                        nameof(CreateApplication),
                        "Admissions");

                return RedirectToAction(
                    "Register",
                    "StudentAccount",
                    new
                    {
                        returnUrl
                    });
            }


            return RedirectToAction(
                nameof(CreateApplication));
        }


        // =========================================================
        // CREATE / RESUME APPLICATION
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> CreateApplication()
        {
            if (User.Identity == null ||
                !User.Identity.IsAuthenticated)
            {
                var returnUrl =
                    Url.Action(
                        nameof(CreateApplication),
                        "Admissions");

                return RedirectToAction(
                    "Login",
                    "StudentAccount",
                    new
                    {
                        returnUrl
                    });
            }


            var userId =
                User.FindFirstValue(
                    ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction(
                    "Login",
                    "StudentAccount");
            }


            var existingApplication =
                await _context.AdmissionApplications
                    .Where(x => x.UserId == userId)
                    .OrderByDescending(x => x.CreatedAt)
                    .FirstOrDefaultAsync();


            if (existingApplication == null)
            {
                var newApplication =
                    CreateNewApplication(userId);

                _context.AdmissionApplications.Add(
                    newApplication);

                await _context.SaveChangesAsync();

                SetApplicationSession(
                    newApplication.Id);

                return RedirectToAction(
                    nameof(Step1));
            }


            SetApplicationSession(
                existingApplication.Id);


            // =====================================================
            // APPROVED
            // =====================================================

            if (existingApplication.Status ==
                AdmissionStatus.Approved)
            {
                return RedirectToAction(
                    "Index",
                    "StudentDashboard");
            }


            // =====================================================
            // WAITING
            // =====================================================

            if (existingApplication.Status ==
                AdmissionStatus.Waiting)
            {
                return RedirectToAction(
                    nameof(Status));
            }


            // =====================================================
            // REJECTED
            // =====================================================

            if (existingApplication.Status ==
                AdmissionStatus.Rejected)
            {
                var newApplication =
                    CreateNewApplication(userId);

                _context.AdmissionApplications.Add(
                    newApplication);

                await _context.SaveChangesAsync();

                SetApplicationSession(
                    newApplication.Id);

                return RedirectToAction(
                    nameof(Step1));
            }


            // =====================================================
            // DRAFT
            // =====================================================

            if (existingApplication.Status ==
                AdmissionStatus.Draft)
            {
                return RedirectToAction(
                    GetStepAction(
                        existingApplication.CurrentStep));
            }


            return RedirectToAction(
                nameof(Index));
        }


        // =========================================================
        // CREATE NEW APPLICATION
        // =========================================================

        private AdmissionApplication CreateNewApplication(
            string userId)
        {
            return new AdmissionApplication
            {
                ApplicationNumber =
                    GenerateApplicationNumber(),

                Status =
                    AdmissionStatus.Draft,

                CurrentStep =
                    1,

                CreatedAt =
                    DateTime.Now,

                UserId =
                    userId,

                ApplicantEmail =
                    User.Identity?.Name
            };
        }


        // =========================================================
        // GENERATE APPLICATION NUMBER
        // =========================================================

        private string GenerateApplicationNumber()
        {
            return
                "FB-" +
                DateTime.Now.ToString(
                    "yyyyMMddHHmmss") +
                "-" +
                Guid.NewGuid()
                    .ToString("N")[..6]
                    .ToUpperInvariant();
        }


        // =========================================================
        // GET STEP ACTION
        // =========================================================

        private string GetStepAction(
            int currentStep)
        {
            return currentStep switch
            {
                1 => nameof(Step1),
                2 => nameof(Step2),
                3 => nameof(Step3),
                4 => nameof(Step4),
                5 => nameof(Step5),
                6 => nameof(Step6),

                _ => nameof(Step1)
            };
        }


        // =========================================================
        // SESSION
        // =========================================================

        private void SetApplicationSession(
            int applicationId)
        {
            HttpContext.Session.SetInt32(
                ApplicationSessionKey,
                applicationId);
        }


        // =========================================================
        // GET CURRENT APPLICATION
        // =========================================================

        private async Task<AdmissionApplication?>
            GetCurrentApplication()
        {
            var applicationId =
                HttpContext.Session.GetInt32(
                    ApplicationSessionKey);


            if (!applicationId.HasValue)
            {
                var userId =
                    User.FindFirstValue(
                        ClaimTypes.NameIdentifier);

                if (string.IsNullOrEmpty(userId))
                {
                    return null;
                }


                var latestApplication =
                    await _context.AdmissionApplications
                        .Where(x => x.UserId == userId)
                        .OrderByDescending(
                            x => x.CreatedAt)
                        .FirstOrDefaultAsync();


                if (latestApplication == null)
                {
                    return null;
                }


                SetApplicationSession(
                    latestApplication.Id);

                applicationId =
                    latestApplication.Id;
            }


            var currentUserId =
                User.FindFirstValue(
                    ClaimTypes.NameIdentifier);


            if (string.IsNullOrEmpty(currentUserId))
            {
                return null;
            }


            return await _context.AdmissionApplications
                .FirstOrDefaultAsync(x =>
                    x.Id == applicationId.Value &&
                    x.UserId == currentUserId);
        }


        // =========================================================
        // STEP 1 - GET
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> Step1()
        {
            var application =
                await GetCurrentApplication();

            if (application == null)
            {
                return RedirectToAction(
                    nameof(CreateApplication));
            }


            if (application.Status !=
                AdmissionStatus.Draft)
            {
                return RedirectToAction(
                    nameof(Status));
            }


            var model =
                new AdmissionStep1ViewModel
                {
                    ChildFirstName =
                        application.ChildFirstName,

                    ChildLastName =
                        application.ChildLastName,

                    DateOfBirth =
                        application.DateOfBirth,

                    Gender =
                        application.Gender,

                    BloodGroup =
                        application.BloodGroup,

                    PreviousSchool =
                        application.PreviousSchool
                };


            return View(model);
        }


        // =========================================================
        // STEP 1 - POST
        // =========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Step1(
            AdmissionStep1ViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }


            var application =
                await GetCurrentApplication();

            if (application == null)
            {
                return RedirectToAction(
                    nameof(CreateApplication));
            }


            if (application.Status !=
                AdmissionStatus.Draft)
            {
                return RedirectToAction(
                    nameof(Status));
            }


            application.ChildFirstName =
                model.ChildFirstName;

            application.ChildLastName =
                model.ChildLastName;

            application.DateOfBirth =
                model.DateOfBirth;

            application.Gender =
                model.Gender;

            application.BloodGroup =
                model.BloodGroup;

            application.PreviousSchool =
                model.PreviousSchool;

            application.CurrentStep =
                2;


            await _context.SaveChangesAsync();


            return RedirectToAction(
                nameof(Step2));
        }


        // =========================================================
        // STEP 2 - GET
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> Step2()
        {
            var application =
                await GetCurrentApplication();

            if (application == null)
            {
                return RedirectToAction(
                    nameof(CreateApplication));
            }


            if (application.Status !=
                AdmissionStatus.Draft)
            {
                return RedirectToAction(
                    nameof(Status));
            }


            var model =
                new AdmissionStep2ViewModel
                {
                    FatherName =
                        application.FatherName,

                    FatherOccupation =
                        application.FatherOccupation,

                    FatherPhone =
                        application.FatherPhone,

                    MotherName =
                        application.MotherName,

                    MotherOccupation =
                        application.MotherOccupation,

                    MotherPhone =
                        application.MotherPhone,

                    ParentEmail =
                        application.ParentEmail
                };


            return View(model);
        }


        // =========================================================
        // STEP 2 - POST
        // =========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Step2(
            AdmissionStep2ViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }


            var application =
                await GetCurrentApplication();

            if (application == null)
            {
                return RedirectToAction(
                    nameof(CreateApplication));
            }


            if (application.Status !=
                AdmissionStatus.Draft)
            {
                return RedirectToAction(
                    nameof(Status));
            }


            application.FatherName =
                model.FatherName;

            application.FatherOccupation =
                model.FatherOccupation;

            application.FatherPhone =
                model.FatherPhone;

            application.MotherName =
                model.MotherName;

            application.MotherOccupation =
                model.MotherOccupation;

            application.MotherPhone =
                model.MotherPhone;

            application.ParentEmail =
                model.ParentEmail;

            application.CurrentStep =
                3;


            await _context.SaveChangesAsync();


            return RedirectToAction(
                nameof(Step3));
        }


        // =========================================================
        // STEP 3 - GET
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> Step3()
        {
            var application =
                await GetCurrentApplication();

            if (application == null)
            {
                return RedirectToAction(
                    nameof(CreateApplication));
            }


            if (application.Status !=
                AdmissionStatus.Draft)
            {
                return RedirectToAction(
                    nameof(Status));
            }


            var model =
                new AdmissionStep3ViewModel
                {
                    Address =
                        application.Address,

                    City =
                        application.City,

                    State =
                        application.State,

                    Pincode =
                        application.Pincode
                };


            return View(model);
        }


        // =========================================================
        // STEP 3 - POST
        // =========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Step3(
            AdmissionStep3ViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }


            var application =
                await GetCurrentApplication();

            if (application == null)
            {
                return RedirectToAction(
                    nameof(CreateApplication));
            }


            if (application.Status !=
                AdmissionStatus.Draft)
            {
                return RedirectToAction(
                    nameof(Status));
            }


            application.Address =
                model.Address;

            application.City =
                model.City;

            application.State =
                model.State;

            application.Pincode =
                model.Pincode;

            application.CurrentStep =
                4;


            await _context.SaveChangesAsync();


            return RedirectToAction(
                nameof(Step4));
        }


        // =========================================================
        // STEP 4 - GET
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> Step4()
        {
            var application =
                await GetCurrentApplication();

            if (application == null)
            {
                return RedirectToAction(
                    nameof(CreateApplication));
            }


            if (application.Status !=
                AdmissionStatus.Draft)
            {
                return RedirectToAction(
                    nameof(Status));
            }


            // -----------------------------------------------------
            // GET ONLY ACTIVE PROGRAMS
            // -----------------------------------------------------

            var programs =
                await _context.Programs
                    .Where(p => p.IsActive)
                    .OrderBy(p => p.ProgramName)
                    .ToListAsync();


            if (!programs.Any())
            {
                TempData["Error"] =
                    "No active programs are currently available.";

                return RedirectToAction(
                    nameof(Index));
            }


            // -----------------------------------------------------
            // CURRENT ACADEMIC YEAR
            // -----------------------------------------------------

            var currentDate =
                DateTime.Now;

            var academicYear =
                GetCurrentAcademicYear(
                    currentDate);


            // -----------------------------------------------------
            // GET SELECTED PROGRAM
            // -----------------------------------------------------

            Programs? selectedProgram = null;


            if (!string.IsNullOrWhiteSpace(
                application.Program))
            {
                selectedProgram =
                    programs.FirstOrDefault(
                        p => p.ProgramName ==
                             application.Program);
            }


            // -----------------------------------------------------
            // IF PROGRAM WAS SAVED BUT NO LONGER ACTIVE
            // -----------------------------------------------------

            if (selectedProgram == null &&
                !string.IsNullOrWhiteSpace(
                    application.Program))
            {
                selectedProgram =
                    await _context.Programs
                        .FirstOrDefaultAsync(
                            p => p.ProgramName ==
                                 application.Program);
            }


            var model =
                new AdmissionStep4ViewModel
                {
                    Program =
                        application.Program ?? string.Empty,

                    AcademicYear =
                        academicYear,

                    PreferredStartDate =
                        selectedProgram != null
                            ? selectedProgram.StartDate
                                .ToString("yyyy-MM-dd")
                            : string.Empty
                };


            ViewBag.Programs =
                programs;


            return View(model);
        }


        // =========================================================
        // STEP 4 - POST
        // =========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Step4(
            AdmissionStep4ViewModel model)
        {
            // -----------------------------------------------------
            // ONLY VALIDATE PROGRAM FROM USER
            // -----------------------------------------------------

            if (string.IsNullOrWhiteSpace(
                model.Program))
            {
                ModelState.AddModelError(
                    nameof(model.Program),
                    "Please select a program.");
            }


            var application =
                await GetCurrentApplication();


            if (application == null)
            {
                return RedirectToAction(
                    nameof(CreateApplication));
            }


            if (application.Status !=
                AdmissionStatus.Draft)
            {
                return RedirectToAction(
                    nameof(Status));
            }


            // -----------------------------------------------------
            // FIND PROGRAM FROM DATABASE
            //
            // DO NOT TRUST:
            // model.AcademicYear
            // model.PreferredStartDate
            //
            // Both values are controlled by the server.
            // -----------------------------------------------------

            Programs? selectedProgram = null;


            if (!string.IsNullOrWhiteSpace(
                model.Program))
            {
                selectedProgram =
                    await _context.Programs
                        .FirstOrDefaultAsync(
                            p =>
                                p.ProgramName ==
                                model.Program &&
                                p.IsActive);
            }


            if (selectedProgram == null)
            {
                ModelState.AddModelError(
                    nameof(model.Program),
                    "The selected program is not available.");
            }


            if (!ModelState.IsValid)
            {
                var programs =
                    await _context.Programs
                        .Where(p => p.IsActive)
                        .OrderBy(p => p.ProgramName)
                        .ToListAsync();

                ViewBag.Programs =
                    programs;

                model.AcademicYear =
                    GetCurrentAcademicYear(
                        DateTime.Now);

                model.PreferredStartDate =
                    selectedProgram != null
                        ? selectedProgram.StartDate
                            .ToString("yyyy-MM-dd")
                        : string.Empty;

                return View(model);
            }


            // -----------------------------------------------------
            // SERVER CONTROLLED ACADEMIC YEAR
            // -----------------------------------------------------

            var academicYear =
                GetCurrentAcademicYear(
                    DateTime.Now);


            // -----------------------------------------------------
            // SERVER CONTROLLED START DATE
            // -----------------------------------------------------

            var startDate =
                selectedProgram!.StartDate
                    .ToString("yyyy-MM-dd");


            // -----------------------------------------------------
            // SAVE PROGRAM
            // -----------------------------------------------------

            application.Program =
                selectedProgram.ProgramName;


            // -----------------------------------------------------
            // SAVE CURRENT ACADEMIC YEAR
            // -----------------------------------------------------

            application.AcademicYear =
                academicYear;


            // -----------------------------------------------------
            // SAVE ADMIN CONTROLLED START DATE
            // -----------------------------------------------------

            application.PreferredStartDate =
                startDate;


            application.CurrentStep =
                5;


            await _context.SaveChangesAsync();


            return RedirectToAction(
                nameof(Step5));
        }


        // =========================================================
        // CURRENT ACADEMIC YEAR
        // =========================================================
        //
        // Example:
        //
        // June 2026 -> 2026-2027
        // January 2026 -> 2025-2026
        //
        // Change the month below if your academy starts
        // its academic year in another month.
        // =========================================================

        private string GetCurrentAcademicYear(
            DateTime date)
        {
            int startYear;


            if (date.Month >= 6)
            {
                startYear =
                    date.Year;
            }
            else
            {
                startYear =
                    date.Year - 1;
            }


            return
                $"{startYear}-{startYear + 1}";
        }


        // =========================================================
        // STEP 5 - GET
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> Step5()
        {
            var application =
                await GetCurrentApplication();

            if (application == null)
            {
                return RedirectToAction(
                    nameof(CreateApplication));
            }


            if (application.Status !=
                AdmissionStatus.Draft)
            {
                return RedirectToAction(
                    nameof(Status));
            }


            return View();
        }


        // =========================================================
        // STEP 5 - POST
        // =========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Step5(
            AdmissionStep5ViewModel model)
        {
            var application =
                await GetCurrentApplication();


            if (application == null)
            {
                return RedirectToAction(
                    nameof(CreateApplication));
            }


            if (application.Status !=
                AdmissionStatus.Draft)
            {
                return RedirectToAction(
                    nameof(Status));
            }


            try
            {
                // -------------------------------------------------
                // BIRTH CERTIFICATE
                // -------------------------------------------------

                if (model.BirthCertificate != null)
                {
                    application.BirthCertificatePath =
                        await SaveFile(
                            model.BirthCertificate,
                            application.ApplicationNumber);
                }


                // -------------------------------------------------
                // CHILD PHOTO
                // -------------------------------------------------

                if (model.ChildPhoto != null)
                {
                    application.ChildPhotoPath =
                        await SaveFile(
                            model.ChildPhoto,
                            application.ApplicationNumber);
                }


                // -------------------------------------------------
                // ADDRESS PROOF
                // -------------------------------------------------

                if (model.AddressProof != null)
                {
                    application.AddressProofPath =
                        await SaveFile(
                            model.AddressProof,
                            application.ApplicationNumber);
                }


                application.CurrentStep =
                    6;


                await _context.SaveChangesAsync();


                return RedirectToAction(
                    nameof(Step6));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(
                    string.Empty,
                    ex.Message);

                return View(model);
            }
        }


        // =========================================================
        // SAVE FILE
        // =========================================================

        private async Task<string> SaveFile(
            IFormFile file,
            string applicationNumber)
        {
            if (file == null)
            {
                throw new InvalidOperationException(
                    "No file was selected.");
            }


            if (file.Length <= 0)
            {
                throw new InvalidOperationException(
                    "Uploaded file is empty.");
            }


            const long maxFileSize =
                5 * 1024 * 1024;


            if (file.Length > maxFileSize)
            {
                throw new InvalidOperationException(
                    "File size cannot exceed 5 MB.");
            }


            var allowedExtensions =
                new[]
                {
                    ".jpg",
                    ".jpeg",
                    ".png",
                    ".pdf"
                };


            var extension =
                Path.GetExtension(
                    file.FileName)
                .ToLowerInvariant();


            if (!allowedExtensions.Contains(
                extension))
            {
                throw new InvalidOperationException(
                    "Invalid file type. Only JPG, JPEG, PNG and PDF files are allowed.");
            }


            var folder =
                Path.Combine(
                    _environment.WebRootPath,
                    "uploads",
                    "admissions",
                    applicationNumber);


            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }


            var fileName =
                Guid.NewGuid()
                    .ToString("N") +
                extension;


            var filePath =
                Path.Combine(
                    folder,
                    fileName);


            await using var stream =
                new FileStream(
                    filePath,
                    FileMode.Create);


            await file.CopyToAsync(
                stream);


            return
                $"/uploads/admissions/" +
                $"{applicationNumber}/{fileName}";
        }


        // =========================================================
        // STEP 6 - GET
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> Step6()
        {
            var application =
                await GetCurrentApplication();

            if (application == null)
            {
                return RedirectToAction(
                    nameof(CreateApplication));
            }


            if (application.Status !=
                AdmissionStatus.Draft)
            {
                return RedirectToAction(
                    nameof(Status));
            }


            return View(application);
        }


        // =========================================================
        // STEP 6 - POST
        // =========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Step6(
            AdmissionStep6ViewModel model)
        {
            if (!model.DeclarationAccepted)
            {
                ModelState.AddModelError(
                    nameof(model.DeclarationAccepted),
                    "Please accept the declaration.");
            }


            if (!ModelState.IsValid)
            {
                var applicationForView =
                    await GetCurrentApplication();

                if (applicationForView == null)
                {
                    return RedirectToAction(
                        nameof(CreateApplication));
                }


                return View(
                    applicationForView);
            }


            var application =
                await GetCurrentApplication();


            if (application == null)
            {
                return RedirectToAction(
                    nameof(CreateApplication));
            }


            if (application.Status !=
                AdmissionStatus.Draft)
            {
                return RedirectToAction(
                    nameof(Status));
            }


            application.DeclarationAccepted =
                true;

            application.ParentSignature =
                model.ParentSignature;


            application.Status =
                AdmissionStatus.Waiting;

            application.CurrentStep =
                6;

            application.SubmittedAt =
                DateTime.Now;


            await _context.SaveChangesAsync();


            return RedirectToAction(
                nameof(Status));
        }


        // =========================================================
        // APPLICATION STATUS
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> Status()
        {
            if (User.Identity == null ||
                !User.Identity.IsAuthenticated)
            {
                return RedirectToAction(
                    "Login",
                    "StudentAccount");
            }


            var application =
                await GetCurrentApplication();


            if (application == null)
            {
                return RedirectToAction(
                    nameof(CreateApplication));
            }


            if (application.Status ==
                AdmissionStatus.Approved)
            {
                return RedirectToAction(
                    "Index",
                    "StudentDashboard");
            }


            if (application.Status ==
                AdmissionStatus.Rejected)
            {
                return RedirectToAction(
                    "AdmissionRejected",
                    "StudentAccount",
                    new
                    {
                        id = application.Id
                    });
            }


            return View(application);
        }
    }
}
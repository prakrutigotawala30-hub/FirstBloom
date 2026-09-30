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

        public AdmissionsController(
            ApplicationDbContext context,
            IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        // =========================================================
        // ADMISSION HOME PAGE
        // /Admissions
        // =========================================================

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }


        // =========================================================
        // APPLY NOW
        // POST: /Admissions/StartApplication
        // =========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult StartApplication()
        {
            // User is not logged in
            if (User.Identity == null ||
                !User.Identity.IsAuthenticated)
            {
                // IMPORTANT:
                // CreateApplication is GET, so after registration/login
                // the user can safely return to this action.
                var returnUrl = Url.Action(
                    nameof(CreateApplication),
                    "Admissions"
                );

                return RedirectToAction(
                    "Register",
                    "StudentAccount",
                    new { returnUrl }
                );
            }

            // User is already logged in
            return RedirectToAction(
                nameof(CreateApplication)
            );
        }


        // =========================================================
        // CREATE / RESUME ADMISSION APPLICATION
        // GET: /Admissions/CreateApplication
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> CreateApplication()
        {
            // User must be logged in
            if (User.Identity == null ||
                !User.Identity.IsAuthenticated)
            {
                var returnUrl = Url.Action(
                    nameof(CreateApplication),
                    "Admissions"
                );

                return RedirectToAction(
                    "Login",
                    "StudentAccount",
                    new { returnUrl }
                );
            }

            // Get logged-in user's Identity ID
            var userId = User.FindFirstValue(
                ClaimTypes.NameIdentifier
            );

            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction(
                    "Login",
                    "StudentAccount"
                );
            }

            // =====================================================
            // CHECK EXISTING APPLICATION
            // =====================================================

            var existingApplication =
                await _context.AdmissionApplications
                    .FirstOrDefaultAsync(x =>
                        x.UserId == userId &&
                        (
                            x.Status == AdmissionStatus.Draft ||
                            x.Status == AdmissionStatus.Waiting
                        )
                    );

            // =====================================================
            // EXISTING APPLICATION FOUND
            // =====================================================

            if (existingApplication != null)
            {
                // Store application ID in session
                HttpContext.Session.SetInt32(
                    "AdmissionApplicationId",
                    existingApplication.Id
                );

                // Application already submitted
                if (existingApplication.Status ==
                    AdmissionStatus.Waiting)
                {
                    return RedirectToAction(
                        nameof(Status)
                    );
                }

                // Draft application
                // Resume from the last completed step
                return RedirectToAction(
                    GetStepAction(
                        existingApplication.CurrentStep
                    )
                );
            }

            // =====================================================
            // CREATE NEW APPLICATION
            // =====================================================

            var application = new AdmissionApplication
            {
                ApplicationNumber =
                    "FB-" +
                    DateTime.Now.ToString("yyyyMMddHHmmss") +
                    "-" +
                    Guid.NewGuid()
                        .ToString("N")[..6]
                        .ToUpper(),

                Status = AdmissionStatus.Draft,

                CurrentStep = 1,

                CreatedAt = DateTime.Now,

                UserId = userId,

                ApplicantEmail =
                    User.Identity?.Name
            };

            _context.AdmissionApplications.Add(
                application
            );

            await _context.SaveChangesAsync();

            // Store current application ID
            HttpContext.Session.SetInt32(
                "AdmissionApplicationId",
                application.Id
            );

            // Start Step 1
            return RedirectToAction(
                nameof(Step1)
            );
        }


        // =========================================================
        // GET STEP ACTION
        // =========================================================

        private string GetStepAction(int currentStep)
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
        // GET CURRENT USER APPLICATION
        // =========================================================

        private async Task<AdmissionApplication?>
            GetCurrentApplication()
        {
            // Get application ID from session
            var applicationId =
                HttpContext.Session.GetInt32(
                    "AdmissionApplicationId"
                );

            if (!applicationId.HasValue)
            {
                return null;
            }

            // Get logged-in user's ID
            var userId = User.FindFirstValue(
                ClaimTypes.NameIdentifier
            );

            if (string.IsNullOrEmpty(userId))
            {
                return null;
            }

            // IMPORTANT:
            // Check BOTH application ID and UserId
            return await _context.AdmissionApplications
                .FirstOrDefaultAsync(x =>
                    x.Id == applicationId.Value &&
                    x.UserId == userId
                );
        }


        // =========================================================
        // STEP 1 - CHILD INFORMATION
        // GET: /Admissions/Step1
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> Step1()
        {
            var application =
                await GetCurrentApplication();

            if (application == null)
            {
                return RedirectToAction(
                    nameof(Index)
                );
            }

            // Only Draft applications can continue
            if (application.Status !=
                AdmissionStatus.Draft)
            {
                return RedirectToAction(
                    nameof(Status)
                );
            }

            var model = new AdmissionStep1ViewModel
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
        // STEP 1 - SAVE CHILD INFORMATION
        // POST: /Admissions/Step1
        // =========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Step1(
            AdmissionStep1ViewModel model)
        {
            // Validate form
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var application =
                await GetCurrentApplication();

            if (application == null)
            {
                return RedirectToAction(
                    nameof(Index)
                );
            }

            if (application.Status !=
                AdmissionStatus.Draft)
            {
                return RedirectToAction(
                    nameof(Status)
                );
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

            application.CurrentStep = 2;

            await _context.SaveChangesAsync();

            return RedirectToAction(
                nameof(Step2)
            );
        }


        // =========================================================
        // STEP 2 - PARENT INFORMATION
        // GET: /Admissions/Step2
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> Step2()
        {
            var application =
                await GetCurrentApplication();

            if (application == null)
            {
                return RedirectToAction(
                    nameof(Index)
                );
            }

            if (application.Status !=
                AdmissionStatus.Draft)
            {
                return RedirectToAction(
                    nameof(Status)
                );
            }

            var model = new AdmissionStep2ViewModel
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
        // STEP 2 - SAVE PARENT INFORMATION
        // POST: /Admissions/Step2
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
                    nameof(Index)
                );
            }

            if (application.Status !=
                AdmissionStatus.Draft)
            {
                return RedirectToAction(
                    nameof(Status)
                );
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

            application.CurrentStep = 3;

            await _context.SaveChangesAsync();

            return RedirectToAction(
                nameof(Step3)
            );
        }


        // =========================================================
        // STEP 3 - ADDRESS
        // GET: /Admissions/Step3
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> Step3()
        {
            var application =
                await GetCurrentApplication();

            if (application == null)
            {
                return RedirectToAction(
                    nameof(Index)
                );
            }

            if (application.Status !=
                AdmissionStatus.Draft)
            {
                return RedirectToAction(
                    nameof(Status)
                );
            }

            var model = new AdmissionStep3ViewModel
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
        // STEP 3 - SAVE ADDRESS
        // POST: /Admissions/Step3
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
                    nameof(Index)
                );
            }

            if (application.Status !=
                AdmissionStatus.Draft)
            {
                return RedirectToAction(
                    nameof(Status)
                );
            }

            application.Address =
                model.Address;

            application.City =
                model.City;

            application.State =
                model.State;

            application.Pincode =
                model.Pincode;

            application.CurrentStep = 4;

            await _context.SaveChangesAsync();

            return RedirectToAction(
                nameof(Step4)
            );
        }


        // =========================================================
        // STEP 4 - PROGRAM
        // GET: /Admissions/Step4
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> Step4()
        {
            var application =
                await GetCurrentApplication();

            if (application == null)
            {
                return RedirectToAction(
                    nameof(Index)
                );
            }

            if (application.Status !=
                AdmissionStatus.Draft)
            {
                return RedirectToAction(
                    nameof(Status)
                );
            }

            var model = new AdmissionStep4ViewModel
            {
                Program =
                    application.Program,

                AcademicYear =
                    application.AcademicYear,

                PreferredStartDate =
                    application.PreferredStartDate,

                TransportRequired =
                    application.TransportRequired,

                DayCareRequired =
                    application.DayCareRequired
            };

            return View(model);
        }


        // =========================================================
        // STEP 4 - SAVE PROGRAM
        // POST: /Admissions/Step4
        // =========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Step4(
            AdmissionStep4ViewModel model)
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
                    nameof(Index)
                );
            }

            if (application.Status !=
                AdmissionStatus.Draft)
            {
                return RedirectToAction(
                    nameof(Status)
                );
            }

            application.Program =
                model.Program;

            application.AcademicYear =
                model.AcademicYear;

            application.PreferredStartDate =
                model.PreferredStartDate;

            application.TransportRequired =
                model.TransportRequired;

            application.DayCareRequired =
                model.DayCareRequired;

            application.CurrentStep = 5;

            await _context.SaveChangesAsync();

            return RedirectToAction(
                nameof(Step5)
            );
        }


        // =========================================================
        // STEP 5 - DOCUMENTS
        // GET: /Admissions/Step5
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> Step5()
        {
            var application =
                await GetCurrentApplication();

            if (application == null)
            {
                return RedirectToAction(
                    nameof(Index)
                );
            }

            if (application.Status !=
                AdmissionStatus.Draft)
            {
                return RedirectToAction(
                    nameof(Status)
                );
            }

            return View();
        }


        // =========================================================
        // STEP 5 - SAVE DOCUMENTS
        // POST: /Admissions/Step5
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
                    nameof(Index)
                );
            }

            if (application.Status !=
                AdmissionStatus.Draft)
            {
                return RedirectToAction(
                    nameof(Status)
                );
            }

            // Birth Certificate
            if (model.BirthCertificate != null)
            {
                application.BirthCertificatePath =
                    await SaveFile(
                        model.BirthCertificate,
                        application.ApplicationNumber
                    );
            }

            // Child Photo
            if (model.ChildPhoto != null)
            {
                application.ChildPhotoPath =
                    await SaveFile(
                        model.ChildPhoto,
                        application.ApplicationNumber
                    );
            }

            // Address Proof
            if (model.AddressProof != null)
            {
                application.AddressProofPath =
                    await SaveFile(
                        model.AddressProof,
                        application.ApplicationNumber
                    );
            }

            application.CurrentStep = 6;

            await _context.SaveChangesAsync();

            return RedirectToAction(
                nameof(Step6)
            );
        }


        // =========================================================
        // SAVE UPLOADED FILE
        // =========================================================

        private async Task<string> SaveFile(
            IFormFile file,
            string applicationNumber)
        {
            var folder =
                Path.Combine(
                    _environment.WebRootPath,
                    "uploads",
                    "admissions",
                    applicationNumber
                );

            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }

            // Get extension
            var extension =
                Path.GetExtension(file.FileName)
                    .ToLowerInvariant();

            // Allowed extensions
            var allowedExtensions =
                new[]
                {
                    ".jpg",
                    ".jpeg",
                    ".png",
                    ".pdf"
                };

            if (!allowedExtensions.Contains(
                extension))
            {
                throw new InvalidOperationException(
                    "Invalid file type. Only JPG, JPEG, PNG and PDF files are allowed."
                );
            }

            // Generate unique filename
            var fileName =
                Guid.NewGuid()
                    .ToString("N") +
                extension;

            var filePath =
                Path.Combine(
                    folder,
                    fileName
                );

            // Save file
            using var stream =
                new FileStream(
                    filePath,
                    FileMode.Create
                );

            await file.CopyToAsync(stream);

            return
                $"/uploads/admissions/{applicationNumber}/{fileName}";
        }


        // =========================================================
        // STEP 6 - DECLARATION
        // GET: /Admissions/Step6
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> Step6()
        {
            var application =
                await GetCurrentApplication();

            if (application == null)
            {
                return RedirectToAction(
                    nameof(Index)
                );
            }

            if (application.Status !=
                AdmissionStatus.Draft)
            {
                return RedirectToAction(
                    nameof(Status)
                );
            }

            return View(application);
        }


        // =========================================================
        // STEP 6 - FINAL SUBMISSION
        // POST: /Admissions/Step6
        // =========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Step6(
            AdmissionStep6ViewModel model)
        {
            // Declaration must be accepted
            if (!model.DeclarationAccepted)
            {
                ModelState.AddModelError(
                    nameof(model.DeclarationAccepted),
                    "Please accept the declaration."
                );
            }

            if (!ModelState.IsValid)
            {
                var applicationForView =
                    await GetCurrentApplication();

                if (applicationForView == null)
                {
                    return RedirectToAction(
                        nameof(Index)
                    );
                }

                return View(applicationForView);
            }

            var application =
                await GetCurrentApplication();

            if (application == null)
            {
                return RedirectToAction(
                    nameof(Index)
                );
            }

            if (application.Status !=
                AdmissionStatus.Draft)
            {
                return RedirectToAction(
                    nameof(Status)
                );
            }

            // Save declaration
            application.DeclarationAccepted = true;

            application.ParentSignature =
                model.ParentSignature;

            // Change application status
            application.Status =
                AdmissionStatus.Waiting;

            application.CurrentStep = 6;

            application.SubmittedAt =
                DateTime.Now;

            await _context.SaveChangesAsync();

            return RedirectToAction(
                nameof(Status)
            );
        }


        // =========================================================
        // APPLICATION STATUS
        // GET: /Admissions/Status
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> Status()
        {
            var application =
                await GetCurrentApplication();

            if (application == null)
            {
                return RedirectToAction(
                    nameof(Index)
                );
            }

            return View(application);
        }
    }
}
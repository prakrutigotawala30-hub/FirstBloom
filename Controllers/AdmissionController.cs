using FirstBloom.Data;
using FirstBloom.Models;
using FirstBloom.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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


        // ==========================================
        // ADMISSION HOME PAGE
        // /Admissions
        // ==========================================

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }


        // ==========================================
        // APPLY NOW
        // ==========================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> StartApplication()
        {
            var application = new AdmissionApplication
            {
                ApplicationNumber =
                    "FB-" +
                    DateTime.Now.ToString("yyyyMMddHHmmss"),

                Status = AdmissionStatus.Draft,

                CurrentStep = 1,

                CreatedAt = DateTime.Now,

                ApplicantEmail = User.Identity?.Name
            };

            _context.AdmissionApplications.Add(application);

            await _context.SaveChangesAsync();

            HttpContext.Session.SetInt32(
                "AdmissionApplicationId",
                application.Id
            );

            return RedirectToAction(nameof(Step1));
        }


        // ==========================================
        // GET CURRENT APPLICATION
        // ==========================================

        private async Task<AdmissionApplication?> GetCurrentApplication()
        {
            var applicationId =
                HttpContext.Session.GetInt32(
                    "AdmissionApplicationId");

            if (!applicationId.HasValue)
            {
                return null;
            }

            return await _context.AdmissionApplications
                .FirstOrDefaultAsync(x =>
                    x.Id == applicationId.Value);
        }


        // ==========================================
        // STEP 1 GET
        // ==========================================

        [HttpGet]
        public async Task<IActionResult> Step1()
        {
            var application = await GetCurrentApplication();

            if (application == null)
            {
                return RedirectToAction(nameof(Index));
            }

            if (application.Status != AdmissionStatus.Draft)
            {
                return RedirectToAction(nameof(Status));
            }

            var model = new AdmissionStep1ViewModel
            {
                ChildFirstName = application.ChildFirstName,
                ChildLastName = application.ChildLastName,
                DateOfBirth = application.DateOfBirth,
                Gender = application.Gender,
                BloodGroup = application.BloodGroup,
                PreviousSchool = application.PreviousSchool
            };

            return View(model);
        }


        // ==========================================
        // STEP 1 POST
        // ==========================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Step1(
    AdmissionStep1ViewModel model)
        {
            var application = await GetCurrentApplication();

            if (application == null)
            {
                return RedirectToAction(nameof(Index));
            }

            application.ChildFirstName = model.ChildFirstName;
            application.ChildLastName = model.ChildLastName;
            application.DateOfBirth = model.DateOfBirth;
            application.Gender = model.Gender;
            application.BloodGroup = model.BloodGroup;
            application.PreviousSchool = model.PreviousSchool;

            application.CurrentStep = 2;

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Step2));
        }
        // ==========================================
        // STEP 2 GET
        // ==========================================

        [HttpGet]
        public async Task<IActionResult> Step2()
        {
            var application = await GetCurrentApplication();

            if (application == null)
            {
                return RedirectToAction(nameof(Index));
            }

            var model = new AdmissionStep2ViewModel
            {
                FatherName = application.FatherName,
                FatherOccupation = application.FatherOccupation,
                FatherPhone = application.FatherPhone,

                MotherName = application.MotherName,
                MotherOccupation = application.MotherOccupation,
                MotherPhone = application.MotherPhone,

                ParentEmail = application.ParentEmail
            };

            return View(model);
        }


        // ==========================================
        // STEP 2 POST
        // ==========================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Step2(
            AdmissionStep2ViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var application = await GetCurrentApplication();

            if (application == null)
            {
                return RedirectToAction(nameof(Index));
            }

            application.FatherName = model.FatherName;
            application.FatherOccupation = model.FatherOccupation;
            application.FatherPhone = model.FatherPhone;

            application.MotherName = model.MotherName;
            application.MotherOccupation = model.MotherOccupation;
            application.MotherPhone = model.MotherPhone;

            application.ParentEmail = model.ParentEmail;

            application.CurrentStep = 3;

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Step3));
        }


        // ==========================================
        // STEP 3 GET
        // ==========================================

        [HttpGet]
        public async Task<IActionResult> Step3()
        {
            var application = await GetCurrentApplication();

            if (application == null)
            {
                return RedirectToAction(nameof(Index));
            }

            var model = new AdmissionStep3ViewModel
            {
                Address = application.Address,
                City = application.City,
                State = application.State,
                Pincode = application.Pincode
            };

            return View(model);
        }


        // ==========================================
        // STEP 3 POST
        // ==========================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Step3(
            AdmissionStep3ViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var application = await GetCurrentApplication();

            if (application == null)
            {
                return RedirectToAction(nameof(Index));
            }

            application.Address = model.Address;
            application.City = model.City;
            application.State = model.State;
            application.Pincode = model.Pincode;

            application.CurrentStep = 4;

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Step4));
        }


        // ==========================================
        // STEP 4 GET
        // ==========================================

        [HttpGet]
        public async Task<IActionResult> Step4()
        {
            var application = await GetCurrentApplication();

            if (application == null)
            {
                return RedirectToAction(nameof(Index));
            }

            var model = new AdmissionStep4ViewModel
            {
                Program = application.Program,
                AcademicYear = application.AcademicYear,
                PreferredStartDate =
                    application.PreferredStartDate,

                TransportRequired =
                    application.TransportRequired,

                DayCareRequired =
                    application.DayCareRequired
            };

            return View(model);
        }


        // ==========================================
        // STEP 4 POST
        // ==========================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Step4(
            AdmissionStep4ViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var application = await GetCurrentApplication();

            if (application == null)
            {
                return RedirectToAction(nameof(Index));
            }

            application.Program = model.Program;
            application.AcademicYear = model.AcademicYear;
            application.PreferredStartDate =
                model.PreferredStartDate;

            application.TransportRequired =
                model.TransportRequired;

            application.DayCareRequired =
                model.DayCareRequired;

            application.CurrentStep = 5;

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Step5));
        }


        // ==========================================
        // STEP 5 GET
        // ==========================================

        [HttpGet]
        public async Task<IActionResult> Step5()
        {
            var application = await GetCurrentApplication();

            if (application == null)
            {
                return RedirectToAction(nameof(Index));
            }

            return View();
        }


        // ==========================================
        // STEP 5 POST
        // ==========================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Step5(
            AdmissionStep5ViewModel model)
        {
            var application = await GetCurrentApplication();

            if (application == null)
            {
                return RedirectToAction(nameof(Index));
            }

            if (model.BirthCertificate != null)
            {
                application.BirthCertificatePath =
                    await SaveFile(
                        model.BirthCertificate,
                        application.ApplicationNumber);
            }

            if (model.ChildPhoto != null)
            {
                application.ChildPhotoPath =
                    await SaveFile(
                        model.ChildPhoto,
                        application.ApplicationNumber);
            }

            if (model.AddressProof != null)
            {
                application.AddressProofPath =
                    await SaveFile(
                        model.AddressProof,
                        application.ApplicationNumber);
            }

            application.CurrentStep = 6;

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Step6));
        }


        // ==========================================
        // SAVE FILE
        // ==========================================

        private async Task<string> SaveFile(
            IFormFile file,
            string applicationNumber)
        {
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

            var extension =
                Path.GetExtension(file.FileName)
                    .ToLowerInvariant();

            var allowedExtensions = new[]
            {
                ".jpg",
                ".jpeg",
                ".png",
                ".pdf"
            };

            if (!allowedExtensions.Contains(extension))
            {
                throw new InvalidOperationException(
                    "Invalid file type.");
            }

            var fileName =
                Guid.NewGuid().ToString() + extension;

            var filePath =
                Path.Combine(folder, fileName);

            using var stream =
                new FileStream(
                    filePath,
                    FileMode.Create);

            await file.CopyToAsync(stream);

            return
                $"/uploads/admissions/{applicationNumber}/{fileName}";
        }


        // ==========================================
        // STEP 6 GET
        // ==========================================

        [HttpGet]
        public async Task<IActionResult> Step6()
        {
            var application = await GetCurrentApplication();

            if (application == null)
            {
                return RedirectToAction(nameof(Index));
            }

            return View(application);
        }


        // ==========================================
        // STEP 6 SUBMIT
        // ==========================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Step6(
            AdmissionStep6ViewModel model)
        {
            if (!model.DeclarationAccepted)
            {
                ModelState.AddModelError(
                    "DeclarationAccepted",
                    "Please accept the declaration.");
            }

            if (!ModelState.IsValid)
            {
                var applicationForView =
                    await GetCurrentApplication();

                return View(applicationForView);
            }

            var application =
                await GetCurrentApplication();

            if (application == null)
            {
                return RedirectToAction(nameof(Index));
            }

            application.DeclarationAccepted = true;

            application.ParentSignature =
                model.ParentSignature;

            application.Status =
                AdmissionStatus.Waiting;

            application.CurrentStep = 6;

            application.SubmittedAt = DateTime.Now;

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Status));
        }


        // ==========================================
        // APPLICATION STATUS
        // ==========================================

        [HttpGet]
        public async Task<IActionResult> Status()
        {
            var application =
                await GetCurrentApplication();

            if (application == null)
            {
                return RedirectToAction(nameof(Index));
            }

            return View(application);
        }
    }
}
using Microsoft.AspNetCore.Authorization;
using FirstBloom.Data;
using FirstBloom.Models;
using FirstBloom.Models.Identity;
using FirstBloom.Models.Student;
using FirstBloom.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
namespace FirstBloom.Controllers
{
    public class StudentAccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ApplicationDbContext _context;
        private readonly EmailService _emailService;
        private readonly AppInstanceService _appInstanceService;
        public StudentAccountController(
      UserManager<ApplicationUser> userManager,
      SignInManager<ApplicationUser> signInManager,
      ApplicationDbContext context,
      EmailService emailService,
      AppInstanceService appInstanceService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _context = context;
            _emailService = emailService;
            _appInstanceService = appInstanceService;
        }


        // =====================================================
        // PROFILE
        // =====================================================

        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            if (User.Identity == null ||
                !User.Identity.IsAuthenticated)
            {
                return RedirectToAction(nameof(Login));
            }

            var userId = User.FindFirstValue(
                ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction(nameof(Login));
            }

            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
            {
                return RedirectToAction(nameof(Login));
            }

            var application =
                await _context.AdmissionApplications
                    .Where(x => x.UserId == userId)
                    .OrderByDescending(x => x.CreatedAt)
                    .FirstOrDefaultAsync();

            ViewBag.Application = application;

            return View(user);
        }


        // =====================================================
        // REGISTER - GET
        // =====================================================

        [AllowAnonymous]
        [HttpGet]
        public IActionResult Register(string? returnUrl = null)
        {
            ViewBag.ReturnUrl = returnUrl;

            return View();
        }


        // =====================================================
        // REGISTER - POST
        // =====================================================

        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(
            StudentRegisterModel model,
            string? returnUrl = null)
        {
            ViewBag.ReturnUrl = returnUrl;

            if (!ModelState.IsValid)
            {
                return View(model);
            }


            // =================================================
            // CHECK EXISTING EMAIL
            // =================================================

            var existingUser =
                await _userManager.FindByEmailAsync(model.Email);

            if (existingUser != null)
            {
                ModelState.AddModelError(
                    nameof(model.Email),
                    "An account with this email already exists.");

                return View(model);
            }


            // =================================================
            // CREATE USER
            // =================================================

            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                PhoneNumber = model.Mobile,
                FullName = model.FullName,
                EmailConfirmed = false
            };

            var createResult =
                await _userManager.CreateAsync(
                    user,
                    model.Password);

            if (!createResult.Succeeded)
            {
                foreach (var error in createResult.Errors)
                {
                    ModelState.AddModelError(
                        string.Empty,
                        error.Description);
                }

                return View(model);
            }


            // =================================================
            // ASSIGN STUDENT ROLE
            // =================================================

            if (!await _userManager.IsInRoleAsync(user, "Student"))
            {
                var roleResult =
                    await _userManager.AddToRoleAsync(
                        user,
                        "Student");

                if (!roleResult.Succeeded)
                {
                    foreach (var error in roleResult.Errors)
                    {
                        ModelState.AddModelError(
                            string.Empty,
                            error.Description);
                    }

                    await _userManager.DeleteAsync(user);

                    return View(model);
                }
            }


            // =================================================
            // GENERATE EMAIL CONFIRMATION TOKEN
            // =================================================

            var token =
                await _userManager
                    .GenerateEmailConfirmationTokenAsync(user);


            // =================================================
            // CREATE CONFIRMATION URL
            // =================================================

            var confirmationUrl =
                Url.Action(
                    nameof(ConfirmEmail),
                    "StudentAccount",
                    new
                    {
                        userId = user.Id,
                        token = token
                    },
                    protocol: Request.Scheme);

            if (string.IsNullOrWhiteSpace(confirmationUrl))
            {
                await _userManager.DeleteAsync(user);

                ModelState.AddModelError(
                    string.Empty,
                    "Unable to create email confirmation link.");

                return View(model);
            }


            // =================================================
            // EMAIL HTML
            // =================================================

            var safeName =
                System.Net.WebUtility.HtmlEncode(
                    model.FullName);

            var safeConfirmationUrl =
                System.Net.WebUtility.HtmlEncode(
                    confirmationUrl);

            var emailBody = $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset=""UTF-8"">

    <meta name=""viewport""
          content=""width=device-width, initial-scale=1.0"">

    <title>Confirm Your FirstBloom Account</title>
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

        <!-- HEADER -->

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


        <!-- CONTENT -->

        <div style=""
            padding:35px;
        "">

            <h2 style=""
                color:#062f6b;
                margin-top:0;
            "">
                Welcome, {safeName}! 🌸
            </h2>

            <p style=""
                color:#26364a;
                font-size:16px;
                line-height:1.6;
            "">
                Thank you for registering with
                <strong>FirstBloom Academy</strong>.
            </p>

            <p style=""
                color:#26364a;
                font-size:16px;
                line-height:1.6;
            "">
                Please confirm your email address by
                clicking the button below.
            </p>


            <!-- BUTTON -->

            <div style=""
                text-align:center;
                margin:35px 0;
            "">

                <a href=""{safeConfirmationUrl}""
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
                    Confirm My Email
                </a>

            </div>


            <p style=""
                color:#68778a;
                font-size:14px;
                line-height:1.6;
            "">
                After confirming your email, you can log in
                to your FirstBloom Academy account.
            </p>

            <p style=""
                color:#68778a;
                font-size:13px;
                line-height:1.6;
            "">
                If you did not create this account,
                you can safely ignore this email.
            </p>

        </div>


        <!-- FOOTER -->

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


            // =================================================
            // SEND EMAIL
            // =================================================

            try
            {
                await _emailService.SendEmailAsync(
                    model.Email,
                    "Confirm Your FirstBloom Academy Account",
                    emailBody);
            }
            catch (Exception ex)
            {
                // Remove user if email cannot be sent
                await _userManager.DeleteAsync(user);

                ModelState.AddModelError(
                    string.Empty,
                    "We could not send the confirmation email. " +
                    ex.Message);

                return View(model);
            }


            // =================================================
            // SUCCESS
            // =================================================

            return RedirectToAction(
                nameof(RegisterSuccess));
        }


        // =====================================================
        // REGISTER SUCCESS
        // =====================================================

        [AllowAnonymous]
        [HttpGet]
        public IActionResult RegisterSuccess()
        {
            return View();
        }


        // =====================================================
        // CONFIRM EMAIL
        // =====================================================

        [AllowAnonymous]
        [HttpGet]
        [Route("StudentAccount/ConfirmEmail")]
        public async Task<IActionResult> ConfirmEmail(
            string userId,
            string token)
        {
            // =================================================
            // CHECK PARAMETERS
            // =================================================

            if (string.IsNullOrWhiteSpace(userId) ||
                string.IsNullOrWhiteSpace(token))
            {
                TempData["ErrorMessage"] =
                    "Invalid email confirmation link.";

                return RedirectToAction(nameof(Login));
            }


            // =================================================
            // FIND USER
            // =================================================

            var user =
                await _userManager.FindByIdAsync(userId);

            if (user == null)
            {
                TempData["ErrorMessage"] =
                    "The user account could not be found.";

                return RedirectToAction(nameof(Login));
            }


            // =================================================
            // ALREADY CONFIRMED
            // =================================================

            if (await _userManager.IsEmailConfirmedAsync(user))
            {
                TempData["SuccessMessage"] =
                    "Your email has already been confirmed.";

                return RedirectToAction(nameof(Login));
            }


            // =================================================
            // CONFIRM EMAIL USING IDENTITY TOKEN
            // =================================================

            var result =
                await _userManager.ConfirmEmailAsync(
                    user,
                    token);


            // =================================================
            // CONFIRMATION FAILED
            // =================================================

            if (!result.Succeeded)
            {
                var errors = string.Join(
                    " ",
                    result.Errors.Select(x => x.Description));

                TempData["ErrorMessage"] =
                    "Email confirmation failed. " + errors;

                return RedirectToAction(nameof(Login));
            }


            // =================================================
            // RELOAD USER FROM DATABASE
            // =================================================

            var confirmedUser =
                await _userManager.FindByIdAsync(user.Id);

            if (confirmedUser == null)
            {
                TempData["ErrorMessage"] =
                    "Unable to verify the confirmed account.";

                return RedirectToAction(nameof(Login));
            }


            // =================================================
            // VERIFY EMAIL CONFIRMED
            // =================================================

            var isConfirmed =
                await _userManager.IsEmailConfirmedAsync(
                    confirmedUser);

            if (!isConfirmed)
            {
                TempData["ErrorMessage"] =
                    "Email confirmation could not be saved.";

                return RedirectToAction(nameof(Login));
            }


            // =================================================
            // SUCCESS
            // =================================================

            TempData["SuccessMessage"] =
                "Your email has been confirmed successfully. " +
                "You can now log in.";

            return RedirectToAction(nameof(Login));
        }


        // =====================================================
        // LOGIN - GET
        // =====================================================

        [AllowAnonymous]
        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            ViewBag.ReturnUrl = returnUrl;

            return View();
        }


        // =====================================================
        // LOGIN - POST
        // =====================================================

        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(
            StudentLoginModel model,
            string? returnUrl = null)
        {
            ViewBag.ReturnUrl = returnUrl;

            if (!ModelState.IsValid)
            {
                return View(model);
            }


            // =================================================
            // FIND USER
            // =================================================

            var user =
                await _userManager.FindByEmailAsync(
                    model.Email);

            if (user == null)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Invalid email or password.");

                return View(model);
            }


            // =================================================
            // CHECK EMAIL CONFIRMATION
            // =================================================

            if (!await _userManager.IsEmailConfirmedAsync(user))
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Please confirm your email before logging in.");

                return View(model);
            }


            // =================================================
            // LOGIN
            // =================================================

            // var result =
            //     await _signInManager.PasswordSignInAsync(
            //         user.UserName!,
            //         model.Password,
            //         model.RememberMe,
            //         lockoutOnFailure: true);

            var result =
                await _signInManager.CheckPasswordSignInAsync(
                    user,
                    model.Password,
                    lockoutOnFailure: true);

            // =================================================
            // LOCKED OUT
            // =================================================

            if (result.IsLockedOut)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Your account has been temporarily locked.");

                return View(model);
            }


            // =================================================
            // LOGIN FAILED
            // =================================================

            if (!result.Succeeded)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Invalid email or password.");

                return View(model);
            }

            // =================================================
            // CREATE STUDENT LOGIN COOKIE
            // =================================================

            var claims = new List<Claim>
{
    new Claim(
        "FirstBloomAppInstanceId",
        _appInstanceService.InstanceId)
};

            await _signInManager.SignInWithClaimsAsync(
                user,
                new AuthenticationProperties
                {
                    IsPersistent = model.RememberMe
                },
                claims);


            // =================================================
            // RETURN URL
            // =================================================

            if (!string.IsNullOrWhiteSpace(returnUrl) &&
                Url.IsLocalUrl(returnUrl))
            {
                // Do not directly use an admission return URL.
                // Admission status must control the destination.

                if (!returnUrl.Contains(
                        "/Admissions/",
                        StringComparison.OrdinalIgnoreCase))
                {
                    return Redirect(returnUrl);
                }
            }


            // =================================================
            // FIND LATEST ADMISSION APPLICATION
            // =================================================

            var application =
                await _context.AdmissionApplications
                    .Where(x => x.UserId == user.Id)
                    .OrderByDescending(x => x.CreatedAt)
                    .FirstOrDefaultAsync();


            // =================================================
            // CASE 1:
            // NO APPLICATION
            // =================================================

            if (application == null)
            {
                return RedirectToAction(
                    "CreateApplication",
                    "Admissions");
            }


            // =================================================
            // SAVE APPLICATION ID IN SESSION
            // =================================================

            HttpContext.Session.SetInt32(
                "AdmissionApplicationId",
                application.Id);


            // =================================================
            // CASE 2:
            // DRAFT
            // =================================================

            if (application.Status ==
                AdmissionStatus.Draft)
            {
                var step = application.CurrentStep;

                if (step < 1)
                {
                    step = 1;
                }

                if (step > 6)
                {
                    step = 6;
                }

                return RedirectToAction(
                    "Step" + step,
                    "Admissions");
            }


            // =================================================
            // CASE 3:
            // WAITING FOR APPROVAL
            // =================================================

            if (application.Status ==
                AdmissionStatus.Waiting)
            {
                return RedirectToAction(
                    nameof(WaitingForApproval),
                    new
                    {
                        id = application.Id
                    });
            }


            // =================================================
            // CASE 4:
            // APPROVED
            // =================================================

            if (application.Status ==
                AdmissionStatus.Approved)
            {
                return RedirectToAction(
                    "Index",
                    "StudentDashboard");
            }


            // =================================================
            // CASE 5:
            // REJECTED
            // =================================================

            if (application.Status ==
                AdmissionStatus.Rejected)
            {
                return RedirectToAction(
                    nameof(AdmissionRejected),
                    new
                    {
                        id = application.Id
                    });
            }


            // =================================================
            // FALLBACK
            // =================================================

            return RedirectToAction(
                "CreateApplication",
                "Admissions");
        }


        // =====================================================
        // WAITING FOR APPROVAL
        // =====================================================

        [HttpGet]
        public async Task<IActionResult> WaitingForApproval(
            int id)
        {
            if (User.Identity == null ||
                !User.Identity.IsAuthenticated)
            {
                return RedirectToAction(nameof(Login));
            }


            var userId =
                User.FindFirstValue(
                    ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction(nameof(Login));
            }


            var application =
                await _context.AdmissionApplications
                    .FirstOrDefaultAsync(x =>
                        x.Id == id &&
                        x.UserId == userId);

            if (application == null)
            {
                return NotFound();
            }


            // Keep session synchronized

            HttpContext.Session.SetInt32(
                "AdmissionApplicationId",
                application.Id);


            // =================================================
            // ADMIN APPROVED
            // =================================================

            if (application.Status ==
                AdmissionStatus.Approved)
            {
                return RedirectToAction(
                    "Index",
                    "StudentDashboard");
            }


            // =================================================
            // ADMIN REJECTED
            // =================================================

            if (application.Status ==
                AdmissionStatus.Rejected)
            {
                return RedirectToAction(
                    nameof(AdmissionRejected),
                    new
                    {
                        id = application.Id
                    });
            }


            // =================================================
            // STILL WAITING
            // =================================================

            return View(application);
        }


        // =====================================================
        // ADMISSION REJECTED
        // =====================================================

        [HttpGet]
        public async Task<IActionResult> AdmissionRejected(
            int id)
        {
            if (User.Identity == null ||
                !User.Identity.IsAuthenticated)
            {
                return RedirectToAction(nameof(Login));
            }


            var userId =
                User.FindFirstValue(
                    ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction(nameof(Login));
            }


            var application =
                await _context.AdmissionApplications
                    .FirstOrDefaultAsync(x =>
                        x.Id == id &&
                        x.UserId == userId);

            if (application == null)
            {
                return NotFound();
            }


            // Keep session synchronized

            HttpContext.Session.SetInt32(
                "AdmissionApplicationId",
                application.Id);


            // =================================================
            // ADMIN APPROVED AFTER REJECTION
            // =================================================

            if (application.Status ==
                AdmissionStatus.Approved)
            {
                return RedirectToAction(
                    "Index",
                    "StudentDashboard");
            }


            // =================================================
            // IF APPLICATION IS DRAFT
            // =================================================

            if (application.Status ==
                AdmissionStatus.Draft)
            {
                return RedirectToAction(
                    "CreateApplication",
                    "Admissions");
            }


            // =================================================
            // IF WAITING
            // =================================================

            if (application.Status ==
                AdmissionStatus.Waiting)
            {
                return RedirectToAction(
                    nameof(WaitingForApproval),
                    new
                    {
                        id = application.Id
                    });
            }


            // =================================================
            // REJECTED
            // =================================================

            return View(application);
        }


        // =====================================================
        // LOGOUT
        // =====================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();

            HttpContext.Session.Remove(
                "AdmissionApplicationId");

            return RedirectToAction(
                "Index",
                "Home");
        }
    }
}
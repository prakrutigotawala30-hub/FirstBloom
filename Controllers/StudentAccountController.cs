using FirstBloom.Data;
using FirstBloom.Models.Identity;
using FirstBloom.Models.Student;
using FirstBloom.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Net.Mail;
using System.Security.Claims;
using static System.Net.Mime.MediaTypeNames;

namespace FirstBloom.Controllers
{
    public class StudentAccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ApplicationDbContext _context;
        private readonly EmailService _emailService;

        public StudentAccountController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ApplicationDbContext context,
            EmailService emailService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _context = context;
            _emailService = emailService;
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

        [HttpGet]
        public IActionResult Register(string? returnUrl = null)
        {
            ViewBag.ReturnUrl = returnUrl;

            return View();
        }


        // =====================================================
        // REGISTER - POST
        // =====================================================

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

                // Important:
                // Email will be confirmed using the link.
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
            // GENERATE EMAIL CONFIRMATION TOKEN
            // =================================================

            var token =
                await _userManager
                    .GenerateEmailConfirmationTokenAsync(user);


            // =================================================
            // GENERATE CONFIRMATION URL
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

            var emailBody = $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">

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
                Welcome, {System.Net.WebUtility.HtmlEncode(model.FullName)}! 🌸
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

                <a href=""{confirmationUrl}""
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
                // Remove account if email could not be sent
                await _userManager.DeleteAsync(user);

                ModelState.AddModelError(
                    string.Empty,
                    "We could not send the confirmation email. " +
                    "Please check your email configuration. " +
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

        [HttpGet]
        public IActionResult RegisterSuccess()
        {
            return View();
        }


        // =====================================================
        // CONFIRM EMAIL
        // =====================================================

        [HttpGet]
        public async Task<IActionResult> ConfirmEmail(
            string userId,
            string token)
        {
            if (string.IsNullOrWhiteSpace(userId) ||
                string.IsNullOrWhiteSpace(token))
            {
                return BadRequest();
            }


            var user =
                await _userManager.FindByIdAsync(userId);

            if (user == null)
            {
                return NotFound();
            }


            // Already confirmed
            if (await _userManager.IsEmailConfirmedAsync(user))
            {
                ViewBag.Message =
                    "Your email has already been confirmed.";

                return View();
            }


            var result =
                await _userManager.ConfirmEmailAsync(
                    user,
                    token);


            if (!result.Succeeded)
            {
                ViewBag.Message =
                    "Email confirmation failed or the link has expired.";

                return View();
            }


            ViewBag.Message =
                "Your email has been confirmed successfully.";

            return View();
        }


        // =====================================================
        // LOGIN - GET
        // =====================================================

        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            ViewBag.ReturnUrl = returnUrl;

            return View();
        }


        // =====================================================
        // LOGIN - POST
        // =====================================================

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
            // CHECK EMAIL
            // =================================================

            if (!await _userManager.IsEmailConfirmedAsync(user))
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Please confirm your email before logging in.");

                return View(model);
            }


            // =================================================
            // PASSWORD LOGIN
            // =================================================

            var result =
                await _signInManager.PasswordSignInAsync(
                    user.UserName!,
                    model.Password,
                    model.RememberMe,
                    lockoutOnFailure: true);


            if (result.Succeeded)
            {
                if (!string.IsNullOrWhiteSpace(returnUrl) &&
                    Url.IsLocalUrl(returnUrl))
                {
                    return Redirect(returnUrl);
                }

                return RedirectToAction(
                    "Index",
                    "StudentDashboard");
            }


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
            // INVALID LOGIN
            // =================================================

            ModelState.AddModelError(
                string.Empty,
                "Invalid email or password.");

            return View(model);
        }


        // =====================================================
        // LOGOUT
        // =====================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();

            return RedirectToAction(
                "Index",
                "Home");
        }
    }
}

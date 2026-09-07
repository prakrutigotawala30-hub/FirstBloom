using FirstBloom.Models.Identity;
using FirstBloom.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace FirstBloom.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IConfiguration _configuration;
        private readonly EmailService _emailService;

        public AccountController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            RoleManager<IdentityRole> roleManager,
            IConfiguration configuration,
            EmailService emailService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _configuration = configuration;
            _emailService = emailService;
        }


        // =========================================================
        // REGISTER - GET
        // =========================================================

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Register()
        {
            return View();
        }


        // =========================================================
        // REGISTER - POST
        // =========================================================

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }


            // -----------------------------------------------------
            // CHECK PRIVATE ADMIN REGISTRATION KEY
            // -----------------------------------------------------

            var registrationKey =
                _configuration["AdminSettings:RegistrationKey"];

            if (string.IsNullOrWhiteSpace(registrationKey))
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Admin registration key is not configured.");

                return View(model);
            }

            if (model.RegistrationKey != registrationKey)
            {
                ModelState.AddModelError(
                    nameof(model.RegistrationKey),
                    "Invalid admin registration key.");

                return View(model);
            }


            // -----------------------------------------------------
            // CHECK IF EMAIL ALREADY EXISTS
            // -----------------------------------------------------

            var existingUser =
                await _userManager.FindByEmailAsync(model.Email);

            if (existingUser != null)
            {
                ModelState.AddModelError(
                    nameof(model.Email),
                    "An account with this email already exists.");

                return View(model);
            }


            // -----------------------------------------------------
            // CREATE ADMIN USER
            // -----------------------------------------------------

            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                FullName = model.FullName,

                // IMPORTANT:
                // Do NOT set EmailConfirmed = true.
                // User must verify Gmail first.
                EmailConfirmed = false
            };


            var createResult =
                await _userManager.CreateAsync(
                    user,
                    model.Password);


            // -----------------------------------------------------
            // USER CREATION FAILED
            // -----------------------------------------------------

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


            // -----------------------------------------------------
            // CREATE ADMIN ROLE IF IT DOES NOT EXIST
            // -----------------------------------------------------

            if (!await _roleManager.RoleExistsAsync("Admin"))
            {
                var roleResult =
                    await _roleManager.CreateAsync(
                        new IdentityRole("Admin"));

                if (!roleResult.Succeeded)
                {
                    // Remove user if role creation failed
                    await _userManager.DeleteAsync(user);

                    foreach (var error in roleResult.Errors)
                    {
                        ModelState.AddModelError(
                            string.Empty,
                            error.Description);
                    }

                    return View(model);
                }
            }


            // -----------------------------------------------------
            // ADD USER TO ADMIN ROLE
            // -----------------------------------------------------

            var roleAssignmentResult =
                await _userManager.AddToRoleAsync(
                    user,
                    "Admin");

            if (!roleAssignmentResult.Succeeded)
            {
                await _userManager.DeleteAsync(user);

                foreach (var error in roleAssignmentResult.Errors)
                {
                    ModelState.AddModelError(
                        string.Empty,
                        error.Description);
                }

                return View(model);
            }


            // -----------------------------------------------------
            // GENERATE EMAIL CONFIRMATION TOKEN
            // -----------------------------------------------------

            var token =
                await _userManager
                    .GenerateEmailConfirmationTokenAsync(user);


            // -----------------------------------------------------
            // GENERATE CONFIRMATION LINK
            // -----------------------------------------------------

            var confirmationLink =
                Url.Action(
                    nameof(ConfirmEmail),
                    "Account",
                    new
                    {
                        area = "Admin",
                        userId = user.Id,
                        token = token
                    },
                    Request.Scheme);


            if (string.IsNullOrWhiteSpace(confirmationLink))
            {
                await _userManager.DeleteAsync(user);

                ModelState.AddModelError(
                    string.Empty,
                    "Unable to generate email confirmation link.");

                return View(model);
            }


            // -----------------------------------------------------
            // EMAIL HTML
            // -----------------------------------------------------

            var emailBody = $@"
<!DOCTYPE html>

<html>

<head>

    <meta charset='UTF-8'>

    <title>Verify FirstBloom Admin Account</title>

</head>

<body style='
    margin:0;
    padding:0;
    background:#f4f7fb;
    font-family:Arial,Helvetica,sans-serif;
'>

    <div style='
        max-width:600px;
        margin:40px auto;
        background:#ffffff;
        border-radius:12px;
        overflow:hidden;
        box-shadow:0 5px 20px rgba(0,0,0,0.08);
    '>

        <div style='
            background:#2563eb;
            padding:30px;
            text-align:center;
            color:white;
        '>

            <h1 style='
                margin:0;
                font-size:28px;
            '>
                FirstBloom Academy
            </h1>

            <p style='
                margin:10px 0 0;
                font-size:15px;
            '>
                Admin Account Verification
            </p>

        </div>


        <div style='
            padding:35px;
            color:#333333;
        '>

            <h2>
                Hello {System.Net.WebUtility.HtmlEncode(model.FullName)},
            </h2>

            <p>
                Your FirstBloom Academy administrator account
                has been successfully created.
            </p>

            <p>
                Before you can log in to the Admin Panel,
                you must verify your email address.
            </p>

            <div style='
                text-align:center;
                margin:30px 0;
            '>

                <a href='{System.Net.WebUtility.HtmlEncode(confirmationLink)}'
                   style='
                       display:inline-block;
                       padding:14px 28px;
                       background:#2563eb;
                       color:#ffffff;
                       text-decoration:none;
                       border-radius:8px;
                       font-weight:bold;
                   '>
                    Verify Email Address
                </a>

            </div>

            <p style='font-size:14px;color:#666666;'>
                If you did not create this account,
                you can safely ignore this email.
            </p>

            <p style='
                margin-top:30px;
                font-size:14px;
                color:#555555;
            '>
                Regards,<br>
                <strong>FirstBloom Academy</strong>
            </p>

        </div>

    </div>

</body>

</html>";


            // -----------------------------------------------------
            // SEND EMAIL
            // -----------------------------------------------------

            try
            {
                await _emailService.SendEmailAsync(
                    model.Email,
                    "Verify Your FirstBloom Admin Account",
                    emailBody);
            }
            catch (Exception)
            {
                // If email cannot be sent, remove created user
                // so an incomplete admin registration is not left.

                await _userManager.DeleteAsync(user);

                ModelState.AddModelError(
                    string.Empty,
                    "Unable to send confirmation email. Please check your Gmail SMTP settings.");

                return View(model);
            }


            // -----------------------------------------------------
            // DO NOT LOGIN HERE
            // -----------------------------------------------------

            return View("RegistrationConfirmation");
        }


        // =========================================================
        // CONFIRM EMAIL
        // =========================================================

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> ConfirmEmail(
            string userId,
            string token)
        {
            if (string.IsNullOrWhiteSpace(userId) ||
                string.IsNullOrWhiteSpace(token))
            {
                return View("EmailConfirmationFailed");
            }


            // -----------------------------------------------------
            // FIND USER
            // -----------------------------------------------------

            var user =
                await _userManager.FindByIdAsync(userId);

            if (user == null)
            {
                return View("EmailConfirmationFailed");
            }


            // -----------------------------------------------------
            // CHECK IF ALREADY CONFIRMED
            // -----------------------------------------------------

            if (await _userManager.IsEmailConfirmedAsync(user))
            {
                return View("EmailConfirmed");
            }


            // -----------------------------------------------------
            // CONFIRM EMAIL
            // -----------------------------------------------------

            var result =
                await _userManager.ConfirmEmailAsync(
                    user,
                    token);


            if (result.Succeeded)
            {
                return View("EmailConfirmed");
            }


            // -----------------------------------------------------
            // CONFIRMATION FAILED
            // -----------------------------------------------------

            return View("EmailConfirmationFailed");
        }


        // =========================================================
        // LOGIN - GET
        // =========================================================

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login(
            string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            return View();
        }


        // =========================================================
        // LOGIN - POST
        // =========================================================

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(
            LoginViewModel model,
            string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;


            if (!ModelState.IsValid)
            {
                return View(model);
            }


            // -----------------------------------------------------
            // FIND USER
            // -----------------------------------------------------

            var user =
                await _userManager.FindByEmailAsync(model.Email);


            if (user == null)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Invalid email or password.");

                return View(model);
            }


            // -----------------------------------------------------
            // CHECK ADMIN ROLE
            // -----------------------------------------------------

            var isAdmin =
                await _userManager.IsInRoleAsync(
                    user,
                    "Admin");


            if (!isAdmin)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "You do not have permission to access the Admin Panel.");

                return View(model);
            }


            // -----------------------------------------------------
            // CHECK EMAIL CONFIRMATION
            // -----------------------------------------------------

            if (!await _userManager.IsEmailConfirmedAsync(user))
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Please verify your email address before logging in.");

                return View(model);
            }


            // -----------------------------------------------------
            // LOGIN
            // -----------------------------------------------------

            var loginResult =
                await _signInManager.PasswordSignInAsync(
                    user.UserName!,
                    model.Password,
                    model.RememberMe,
                    lockoutOnFailure: true);


            // -----------------------------------------------------
            // SUCCESS
            // -----------------------------------------------------

            if (loginResult.Succeeded)
            {
                if (!string.IsNullOrWhiteSpace(returnUrl) &&
                    Url.IsLocalUrl(returnUrl))
                {
                    return Redirect(returnUrl);
                }

                return RedirectToAction(
                    "Index",
                    "Dashboard",
                    new
                    {
                        area = "Admin"
                    });
            }


            // -----------------------------------------------------
            // LOCKED OUT
            // -----------------------------------------------------

            if (loginResult.IsLockedOut)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Your account has been temporarily locked because of multiple failed login attempts.");

                return View(model);
            }


            // -----------------------------------------------------
            // NOT ALLOWED
            // -----------------------------------------------------

            if (loginResult.IsNotAllowed)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Login is not allowed. Please verify your email address.");

                return View(model);
            }


            // -----------------------------------------------------
            // INVALID LOGIN
            // -----------------------------------------------------

            ModelState.AddModelError(
                string.Empty,
                "Invalid email or password.");

            return View(model);
        }


        // =========================================================
        // LOGOUT
        // =========================================================

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();

            return RedirectToAction(
                nameof(Login),
                "Account",
                new
                {
                    area = "Admin"
                });
        }


        // =========================================================
        // ACCESS DENIED
        // =========================================================

        [HttpGet]
        [AllowAnonymous]
        public IActionResult AccessDenied()
        {
            return View();
        }
    }


    // =============================================================
    // REGISTER VIEW MODEL
    // =============================================================

    public class RegisterViewModel
    {
        [System.ComponentModel.DataAnnotations.Required(
            ErrorMessage = "Full name is required.")]
        [System.ComponentModel.DataAnnotations.StringLength(
            100)]
        [System.ComponentModel.DataAnnotations.Display(
            Name = "Full Name")]
        public string FullName { get; set; } = string.Empty;


        [System.ComponentModel.DataAnnotations.Required(
            ErrorMessage = "Email is required.")]
        [System.ComponentModel.DataAnnotations.EmailAddress(
            ErrorMessage = "Enter a valid email address.")]
        public string Email { get; set; } = string.Empty;


        [System.ComponentModel.DataAnnotations.Required(
            ErrorMessage = "Password is required.")]
        [System.ComponentModel.DataAnnotations.DataType(
            System.ComponentModel.DataAnnotations.DataType.Password)]
        [System.ComponentModel.DataAnnotations.MinLength(
            6,
            ErrorMessage = "Password must contain at least 6 characters.")]
        public string Password { get; set; } = string.Empty;


        [System.ComponentModel.DataAnnotations.Required(
            ErrorMessage = "Please confirm your password.")]
        [System.ComponentModel.DataAnnotations.DataType(
            System.ComponentModel.DataAnnotations.DataType.Password)]
        [System.ComponentModel.DataAnnotations.Compare(
            nameof(Password),
            ErrorMessage = "Passwords do not match.")]
        [System.ComponentModel.DataAnnotations.Display(
            Name = "Confirm Password")]
        public string ConfirmPassword { get; set; } = string.Empty;


        [System.ComponentModel.DataAnnotations.Required(
            ErrorMessage = "Registration key is required.")]
        [System.ComponentModel.DataAnnotations.Display(
            Name = "Admin Registration Key")]
        [System.ComponentModel.DataAnnotations.DataType(
            System.ComponentModel.DataAnnotations.DataType.Password)]
        public string RegistrationKey { get; set; } = string.Empty;
    }


    // =============================================================
    // LOGIN VIEW MODEL
    // =============================================================

    public class LoginViewModel
    {
        [System.ComponentModel.DataAnnotations.Required(
            ErrorMessage = "Email is required.")]
        [System.ComponentModel.DataAnnotations.EmailAddress(
            ErrorMessage = "Enter a valid email address.")]
        public string Email { get; set; } = string.Empty;


        [System.ComponentModel.DataAnnotations.Required(
            ErrorMessage = "Password is required.")]
        [System.ComponentModel.DataAnnotations.DataType(
            System.ComponentModel.DataAnnotations.DataType.Password)]
        public string Password { get; set; } = string.Empty;


        [System.ComponentModel.DataAnnotations.Display(
            Name = "Remember me")]
        public bool RememberMe { get; set; }
    }
}
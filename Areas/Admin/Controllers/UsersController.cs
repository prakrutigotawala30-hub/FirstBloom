using FirstBloom.Models.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static System.Net.Mime.MediaTypeNames;

namespace FirstBloom.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class UsersController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public UsersController(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }


        // =========================================================
        // USERS
        // GET: /Admin/Users
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var users = await _userManager.Users
                .OrderBy(u => u.UserName)
                .ToListAsync();

            var model = new List<AdminUserViewModel>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);

                model.Add(new AdminUserViewModel
                {
                    Id = user.Id,
                    UserName = user.UserName,
                    Email = user.Email,
                    EmailConfirmed = user.EmailConfirmed,
                    Roles = roles.ToList()
                });
            }

            return View(model);
        }


        // =========================================================
        // MANAGE ROLES
        // GET: /Admin/Users/ManageRoles/{id}
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> ManageRoles(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return NotFound();
            }

            var user = await _userManager.FindByIdAsync(id);

            if (user == null)
            {
                return NotFound();
            }

            // -----------------------------------------------------
            // Make sure required roles exist
            // -----------------------------------------------------

            await EnsureRolesExist();


            var currentRoles =
                await _userManager.GetRolesAsync(user);

            var availableRoles = await _roleManager.Roles
                .Where(r => r.Name != null)
                .Select(r => r.Name!)
                .OrderBy(r => r)
                .ToListAsync();


            var model = new ManageUserRolesViewModel
            {
                UserId = user.Id,
                UserName = user.UserName,
                Email = user.Email,
                EmailConfirmed = user.EmailConfirmed,
                CurrentRoles = currentRoles.ToList(),
                AvailableRoles = availableRoles
            };

            return View(model);
        }


        // =========================================================
        // ADD ROLE
        // POST: /Admin/Users/AddRole
        // =========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddRole(
            string userId,
            string roleName)
        {
            if (string.IsNullOrWhiteSpace(userId) ||
                string.IsNullOrWhiteSpace(roleName))
            {
                TempData["Error"] =
                    "Please select a valid user and role.";

                return RedirectToAction(nameof(Index));
            }


            var user =
                await _userManager.FindByIdAsync(userId);

            if (user == null)
            {
                TempData["Error"] =
                    "User not found.";

                return RedirectToAction(nameof(Index));
            }


            roleName = roleName.Trim();


            // -----------------------------------------------------
            // Check role
            // -----------------------------------------------------

            if (!await _roleManager.RoleExistsAsync(roleName))
            {
                TempData["Error"] =
                    $"Role '{roleName}' does not exist.";

                return RedirectToAction(
                    nameof(ManageRoles),
                    new { id = userId });
            }


            // -----------------------------------------------------
            // Already has role
            // -----------------------------------------------------

            if (await _userManager.IsInRoleAsync(user, roleName))
            {
                TempData["Error"] =
                    $"User already has the '{roleName}' role.";

                return RedirectToAction(
                    nameof(ManageRoles),
                    new { id = userId });
            }


            // -----------------------------------------------------
            // Add role
            // -----------------------------------------------------

            var result =
                await _userManager.AddToRoleAsync(
                    user,
                    roleName);


            if (result.Succeeded)
            {
                TempData["Success"] =
                    $"'{roleName}' role assigned successfully.";
            }
            else
            {
                TempData["Error"] =
                    string.Join(
                        " ",
                        result.Errors.Select(x => x.Description));
            }


            return RedirectToAction(
                nameof(ManageRoles),
                new { id = userId });
        }


        // =========================================================
        // REMOVE ROLE
        // POST: /Admin/Users/RemoveRole
        // =========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveRole(
            string userId,
            string roleName)
        {
            if (string.IsNullOrWhiteSpace(userId) ||
                string.IsNullOrWhiteSpace(roleName))
            {
                TempData["Error"] =
                    "Invalid user or role.";

                return RedirectToAction(nameof(Index));
            }


            var user =
                await _userManager.FindByIdAsync(userId);

            if (user == null)
            {
                TempData["Error"] =
                    "User not found.";

                return RedirectToAction(nameof(Index));
            }


            roleName = roleName.Trim();


            // -----------------------------------------------------
            // Don't allow current Admin to remove own Admin role
            // -----------------------------------------------------

            var currentUserId =
                _userManager.GetUserId(User);

            if (user.Id == currentUserId &&
                roleName.Equals(
                    "Admin",
                    StringComparison.OrdinalIgnoreCase))
            {
                TempData["Error"] =
                    "You cannot remove the Admin role from yourself.";

                return RedirectToAction(
                    nameof(ManageRoles),
                    new { id = userId });
            }


            // -----------------------------------------------------
            // Check role
            // -----------------------------------------------------

            if (!await _userManager.IsInRoleAsync(
                    user,
                    roleName))
            {
                TempData["Error"] =
                    $"User does not have the '{roleName}' role.";

                return RedirectToAction(
                    nameof(ManageRoles),
                    new { id = userId });
            }


            // -----------------------------------------------------
            // Remove role
            // -----------------------------------------------------

            var result =
                await _userManager.RemoveFromRoleAsync(
                    user,
                    roleName);


            if (result.Succeeded)
            {
                TempData["Success"] =
                    $"'{roleName}' role removed successfully.";
            }
            else
            {
                TempData["Error"] =
                    string.Join(
                        " ",
                        result.Errors.Select(x => x.Description));
            }


            return RedirectToAction(
                nameof(ManageRoles),
                new { id = userId });
        }


        // =========================================================
        // CONVERT USER TO STUDENT
        // POST: /Admin/Users/MakeStudent
        // =========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MakeStudent(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                TempData["Error"] =
                    "Invalid user.";

                return RedirectToAction(nameof(Index));
            }


            var user =
                await _userManager.FindByIdAsync(id);

            if (user == null)
            {
                TempData["Error"] =
                    "User not found.";

                return RedirectToAction(nameof(Index));
            }


            // -----------------------------------------------------
            // Never convert an Admin to Student
            // -----------------------------------------------------

            if (await _userManager.IsInRoleAsync(
                    user,
                    "Admin"))
            {
                TempData["Error"] =
                    "Admin users cannot be converted to Student.";

                return RedirectToAction(nameof(Index));
            }


            // -----------------------------------------------------
            // Make sure Student role exists
            // -----------------------------------------------------

            if (!await _roleManager.RoleExistsAsync("Student"))
            {
                await _roleManager.CreateAsync(
                    new IdentityRole("Student"));
            }


            // -----------------------------------------------------
            // Remove existing roles
            // -----------------------------------------------------

            var currentRoles =
                await _userManager.GetRolesAsync(user);

            if (currentRoles.Any())
            {
                var removeResult =
                    await _userManager.RemoveFromRolesAsync(
                        user,
                        currentRoles);

                if (!removeResult.Succeeded)
                {
                    TempData["Error"] =
                        string.Join(
                            " ",
                            removeResult.Errors
                                .Select(x => x.Description));

                    return RedirectToAction(nameof(Index));
                }
            }


            // -----------------------------------------------------
            // Add Student role
            // -----------------------------------------------------

            var result =
                await _userManager.AddToRoleAsync(
                    user,
                    "Student");


            if (result.Succeeded)
            {
                TempData["Success"] =
                    $"{user.Email} is now a Student.";
            }
            else
            {
                TempData["Error"] =
                    string.Join(
                        " ",
                        result.Errors.Select(x => x.Description));
            }


            return RedirectToAction(nameof(Index));
        }


        // =========================================================
        // DELETE USER
        // =========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                TempData["Error"] =
                    "Invalid user.";

                return RedirectToAction(nameof(Index));
            }


            var currentUserId =
                _userManager.GetUserId(User);

            if (id == currentUserId)
            {
                TempData["Error"] =
                    "You cannot delete your own account.";

                return RedirectToAction(nameof(Index));
            }


            var user =
                await _userManager.FindByIdAsync(id);

            if (user == null)
            {
                TempData["Error"] =
                    "User not found.";

                return RedirectToAction(nameof(Index));
            }


            // -----------------------------------------------------
            // Protect Admin accounts
            // -----------------------------------------------------

            if (await _userManager.IsInRoleAsync(
                    user,
                    "Admin"))
            {
                TempData["Error"] =
                    "Admin accounts cannot be deleted.";

                return RedirectToAction(nameof(Index));
            }


            var email = user.Email;


            var result =
                await _userManager.DeleteAsync(user);


            if (result.Succeeded)
            {
                TempData["Success"] =
                    $"User '{email}' deleted successfully.";
            }
            else
            {
                TempData["Error"] =
                    string.Join(
                        " ",
                        result.Errors.Select(x => x.Description));
            }


            return RedirectToAction(nameof(Index));
        }


        // =========================================================
        // ENSURE REQUIRED ROLES EXIST
        // =========================================================

        private async Task EnsureRolesExist()
        {
            string[] requiredRoles =
            {
                "Admin",
                "Student"
            };


            foreach (var role in requiredRoles)
            {
                if (!await _roleManager.RoleExistsAsync(role))
                {
                    await _roleManager.CreateAsync(
                        new IdentityRole(role));
                }
            }
        }

        // =========================================================
        // USER DETAILS
        // GET: /Admin/Users/Details/{id}
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> Details(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return NotFound();
            }

            // -----------------------------------------------------
            // Find user
            // -----------------------------------------------------

            var user =
                await _userManager.FindByIdAsync(id);

            if (user == null)
            {
                return NotFound();
            }

            // -----------------------------------------------------
            // Get user's roles
            // -----------------------------------------------------

            var roles =
                await _userManager.GetRolesAsync(user);

            // -----------------------------------------------------
            // Build model
            // -----------------------------------------------------

            var model = new AdminUserViewModel
            {
                Id = user.Id,
                UserName = user.UserName,
                Email = user.Email,
                EmailConfirmed = user.EmailConfirmed,
                Roles = roles.ToList()
            };

            return View(model);
        }

    }


    // =============================================================
    // USER VIEW MODEL
    // =============================================================

    public class AdminUserViewModel
    {
        public string Id { get; set; } = string.Empty;

        public string? UserName { get; set; }

        public string? Email { get; set; }

        public bool EmailConfirmed { get; set; }

        public List<string> Roles { get; set; } = new();
    }


    // =============================================================
    // MANAGE ROLE VIEW MODEL
    // =============================================================

    public class ManageUserRolesViewModel
    {
        public string UserId { get; set; } = string.Empty;

        public string? UserName { get; set; }

        public string? Email { get; set; }

        public bool EmailConfirmed { get; set; }

        public List<string> CurrentRoles { get; set; } = new();

        public List<string> AvailableRoles { get; set; } = new();
    }
}
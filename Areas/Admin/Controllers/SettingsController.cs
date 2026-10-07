using FirstBloom.Data;
using FirstBloom.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FirstBloom.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class SettingsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public SettingsController(ApplicationDbContext context)
        {
            _context = context;
        }


        // =====================================================
        // SETTINGS - GET
        // =====================================================

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var settings = await _context.SiteSettings
                .FirstOrDefaultAsync();

            // =================================================
            // CREATE DEFAULT SETTINGS IF NO RECORD EXISTS
            // =================================================

            if (settings == null)
            {
                settings = new SiteSetting
                {
                    AcademyName = "FirstBloom Academy",
                    Logo = "/images/logo.png",
                    Phone = "",
                    Email = "",
                    Address = "",
                    Facebook = "",
                    Instagram = "",
                    YouTube = "",
                    FooterDescription =
                        "Nurturing young minds with love, learning and care."
                };

                _context.SiteSettings.Add(settings);

                await _context.SaveChangesAsync();
            }

            return View(settings);
        }


        // =====================================================
        // SETTINGS - POST
        // =====================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(SiteSetting model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var settings = await _context.SiteSettings
                .FirstOrDefaultAsync();

            // =================================================
            // IF SETTINGS RECORD DOES NOT EXIST
            // =================================================

            if (settings == null)
            {
                settings = new SiteSetting();

                _context.SiteSettings.Add(settings);
            }


            // =================================================
            // UPDATE ACADEMY INFORMATION
            // =================================================

            settings.AcademyName =
                model.AcademyName;

            settings.Logo =
                model.Logo;


            // =================================================
            // UPDATE CONTACT INFORMATION
            // =================================================

            settings.Phone =
                model.Phone;

            settings.Email =
                model.Email;

            settings.Address =
                model.Address;


            // =================================================
            // UPDATE SOCIAL MEDIA
            // =================================================

            settings.Facebook =
                model.Facebook;

            settings.Instagram =
                model.Instagram;

            settings.YouTube =
                model.YouTube;


            // =================================================
            // UPDATE FOOTER
            // =================================================

            settings.FooterDescription =
                model.FooterDescription;


            // =================================================
            // SAVE
            // =================================================

            await _context.SaveChangesAsync();


            // =================================================
            // SUCCESS MESSAGE
            // =================================================

            TempData["SuccessMessage"] =
                "Website settings updated successfully.";


            return RedirectToAction(nameof(Index));
        }
    }
}
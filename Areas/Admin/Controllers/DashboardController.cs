using FirstBloom.Data;
using FirstBloom.Models.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FirstBloom.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public DashboardController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }



        // =========================================================
        // ADMIN DASHBOARD
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> Index()
        {


            var currentUser =
                await _userManager.GetUserAsync(User);

            string adminName =
                currentUser?.FullName
                ?? currentUser?.UserName
                ?? currentUser?.Email
                ?? "Admin";

            ViewBag.AdminName = adminName;


            // =====================================================
            // CURRENT YEAR
            // =====================================================

            int currentYear = DateTime.Now.Year;


            // =====================================================
            // TOTAL ADMISSIONS
            // =====================================================

            int totalAdmissions =
                await _context.AdmissionApplications
                    .CountAsync();


            // =====================================================
            // TOTAL STUDENTS
            // =====================================================

            int totalStudents =
                await _context.StudentProfiles
                    .CountAsync();


            // =====================================================
            // TOTAL PROGRAMS
            // =====================================================

            int totalPrograms =
                await _context.Programs
                    .CountAsync(p => p.IsActive);


            // =====================================================
            // TOTAL ENQUIRIES
            // =====================================================

            int totalEnquiries =
                await _context.ContactMessages
                    .CountAsync();


            // =====================================================
            // MONTH LABELS
            // =====================================================

            string[] monthLabels =
            {
                "Jan",
                "Feb",
                "Mar",
                "Apr",
                "May",
                "Jun",
                "Jul",
                "Aug",
                "Sep",
                "Oct",
                "Nov",
                "Dec"
            };


            // =====================================================
            // ADMISSION MONTHLY DATA
            // =====================================================

            var admissionMonthlyData =
                await _context.AdmissionApplications
                    .Where(a =>
                        a.CreatedAt.Year == currentYear)
                    .GroupBy(a =>
                        a.CreatedAt.Month)
                    .Select(g => new
                    {
                        Month = g.Key,
                        Count = g.Count()
                    })
                    .ToListAsync();


            int[] admissionData =
                new int[12];


            foreach (var item in admissionMonthlyData)
            {
                if (item.Month >= 1 &&
                    item.Month <= 12)
                {
                    admissionData[item.Month - 1] =
                        item.Count;
                }
            }


            // =====================================================
            // ENQUIRY MONTHLY DATA
            // =====================================================

            var enquiryMonthlyData =
                await _context.ContactMessages
                    .Where(e =>
                        e.CreatedAt.Year == currentYear)
                    .GroupBy(e =>
                        e.CreatedAt.Month)
                    .Select(g => new
                    {
                        Month = g.Key,
                        Count = g.Count()
                    })
                    .ToListAsync();


            int[] enquiryData =
                new int[12];


            foreach (var item in enquiryMonthlyData)
            {
                if (item.Month >= 1 &&
                    item.Month <= 12)
                {
                    enquiryData[item.Month - 1] =
                        item.Count;
                }
            }


            // =====================================================
            // RECENT ENQUIRIES
            // =====================================================

            var recentEnquiries =
                await _context.ContactMessages
                    .OrderByDescending(e => e.CreatedAt)
                    .Take(5)
                    .Select(e => new
                    {
                        e.Name,
                        e.Subject,
                        e.CreatedAt
                    })
                    .ToListAsync();


            // =====================================================
            // PREPARE RECENT ENQUIRIES FOR VIEW
            // =====================================================

            var recentEnquiryList =
                recentEnquiries
                    .Select(e =>
                    {
                        string name =
                            string.IsNullOrWhiteSpace(e.Name)
                                ? "Unknown"
                                : e.Name.Trim();


                        string initials =
                            GetInitials(name);


                        return new
                        {
                            Name = name,

                            Initials = initials,

                            Subject =
                                string.IsNullOrWhiteSpace(e.Subject)
                                    ? "General enquiry"
                                    : e.Subject,

                            Date =
                                FormatEnquiryDate(e.CreatedAt)
                        };
                    })
                    .ToList();


            // =====================================================
            // SEND DATA TO VIEW
            // =====================================================

            ViewBag.TotalAdmissions =
                totalAdmissions;

            ViewBag.TotalStudents =
                totalStudents;

            ViewBag.TotalPrograms =
                totalPrograms;

            ViewBag.TotalEnquiries =
                totalEnquiries;


            // =====================================================
            // CHART DATA
            // =====================================================

            ViewBag.AdmissionLabels =
                monthLabels;

            ViewBag.AdmissionData =
                admissionData;

            ViewBag.EnquiryData =
                enquiryData;


            // =====================================================
            // RECENT ENQUIRIES
            // =====================================================

            ViewBag.RecentEnquiries =
                recentEnquiryList;


            return View();
        }


        // =========================================================
        // GET INITIALS
        // =========================================================

        private static string GetInitials(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return "?";
            }


            var parts =
                name.Split(
                    ' ',
                    StringSplitOptions.RemoveEmptyEntries);


            if (parts.Length == 1)
            {
                return parts[0]
                    .Substring(
                        0,
                        Math.Min(
                            2,
                            parts[0].Length))
                    .ToUpperInvariant();
            }


            string first =
                parts[0].Substring(0, 1);


            string last =
                parts[^1].Substring(0, 1);


            return (
                first + last
            ).ToUpperInvariant();
        }


        // =========================================================
        // FORMAT ENQUIRY DATE
        // =========================================================

        private static string FormatEnquiryDate(
            DateTime date)
        {
            if (date.Date == DateTime.Today)
            {
                return "Today";
            }


            if (date.Date ==
                DateTime.Today.AddDays(-1))
            {
                return "Yesterday";
            }


            return date.ToString("dd MMM");
        }
    }
}
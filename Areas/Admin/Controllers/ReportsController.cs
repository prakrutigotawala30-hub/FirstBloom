
using FirstBloom.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FirstBloom.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class ReportsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ReportsController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            // =====================================================
            // BASIC COUNTS
            // =====================================================

            ViewBag.TotalPrograms =
                await _context.Programs.CountAsync();

            ViewBag.TotalAdmissions =
                await _context.AdmissionApplications.CountAsync();

            ViewBag.TotalBlogs =
                await _context.Blogs.CountAsync();

            ViewBag.PublishedBlogs =
                await _context.Blogs.CountAsync(x => x.IsPublished);

            ViewBag.DraftBlogs =
                await _context.Blogs.CountAsync(x => !x.IsPublished);

            ViewBag.TotalEnquiries =
                await _context.ContactMessages.CountAsync();

            ViewBag.ReadEnquiries =
                await _context.ContactMessages.CountAsync(x => x.IsRead);

            ViewBag.UnreadEnquiries =
                await _context.ContactMessages.CountAsync(x => !x.IsRead);

            ViewBag.TotalNotices =
                await _context.Notices.CountAsync();

            ViewBag.TotalFAQs =
                await _context.FAQs.CountAsync();

            ViewBag.TotalTestimonials =
                await _context.Testimonials.CountAsync();

            ViewBag.TotalAcademicYears =
                await _context.AcademicYears.CountAsync();

            ViewBag.TotalNoticeReads =
                await _context.NoticeReads.CountAsync();


            // =====================================================
            // MONTHLY ENQUIRIES
            // =====================================================

            int currentYear = DateTime.Now.Year;

            var monthlyEnquiries =
                await _context.ContactMessages
                    .Where(x => x.CreatedAt.Year == currentYear)
                    .GroupBy(x => x.CreatedAt.Month)
                    .Select(g => new
                    {
                        Month = g.Key,
                        Count = g.Count()
                    })
                    .ToListAsync();

            var monthLabels = new List<string>();
            var monthValues = new List<int>();

            for (int month = 1; month <= 12; month++)
            {
                monthLabels.Add(
                    new DateTime(currentYear, month, 1)
                        .ToString("MMM")
                );

                var data =
                    monthlyEnquiries
                        .FirstOrDefault(x => x.Month == month);

                monthValues.Add(data?.Count ?? 0);
            }

            ViewBag.MonthLabels = monthLabels;
            ViewBag.MonthValues = monthValues;


            // =====================================================
            // RECENT ENQUIRIES
            // =====================================================

            ViewBag.RecentEnquiries =
                await _context.ContactMessages
                    .AsNoTracking()
                    .OrderByDescending(x => x.CreatedAt)
                    .Take(5)
                    .ToListAsync();


            // =====================================================
            // RECENT ADMISSIONS
            // =====================================================

            ViewBag.RecentAdmissions =
                await _context.AdmissionApplications
                    .AsNoTracking()
                    .OrderByDescending(x => x.CreatedAt)
                    .Take(5)
                    .ToListAsync();


            // =====================================================
            // RECENT BLOGS
            // =====================================================

            ViewBag.RecentBlogs =
                await _context.Blogs
                    .AsNoTracking()
                    .OrderByDescending(x => x.PublishedDate)
                    .Take(5)
                    .ToListAsync();


            // =====================================================
            // RETURN
            // =====================================================

            return View();
        }
    }
}

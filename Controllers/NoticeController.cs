using FirstBloom.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace FirstBloom.Controllers
{
    [Authorize]
    public class NoticeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public NoticeController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(
                ClaimTypes.NameIdentifier);

            if (string.IsNullOrWhiteSpace(userId))
            {
                return Challenge();
            }

            var notices = await _context.Notices
                .Where(x =>
                    x.IsActive &&
                    (
                        x.NoticeType == "Public" ||
                        (
                            x.NoticeType == "Personal" &&
                            x.UserId == userId
                        )
                    )
                )
                .OrderByDescending(x =>
                    x.PublishedAt ?? x.CreatedAt)
                .ToListAsync();

            return View(notices);
        }


        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var userId = User.FindFirstValue(
                ClaimTypes.NameIdentifier);

            if (string.IsNullOrWhiteSpace(userId))
            {
                return Challenge();
            }

            var notice = await _context.Notices
                .FirstOrDefaultAsync(x =>
                    x.Id == id &&
                    x.IsActive &&
                    (
                        x.NoticeType == "Public" ||
                        (
                            x.NoticeType == "Personal" &&
                            x.UserId == userId
                        )
                    )
                );

            if (notice == null)
            {
                return NotFound();
            }

            return View(notice);
        }


        [HttpGet]
        public async Task<IActionResult> GetNotifications()
        {
            var userId = User.FindFirstValue(
                ClaimTypes.NameIdentifier);

            if (string.IsNullOrWhiteSpace(userId))
            {
                return Unauthorized();
            }

            var notices = await _context.Notices
                .Where(x =>
                    x.IsActive &&
                    (
                        x.NoticeType == "Public" ||
                        (
                            x.NoticeType == "Personal" &&
                            x.UserId == userId
                        )
                    )
                )
                .OrderByDescending(x =>
                    x.PublishedAt ?? x.CreatedAt)
                .Select(x => new
                {
                    id = x.Id,
                    title = x.Title,
                    message = x.Message,
                    noticeType = x.NoticeType,
                    publishedAt = x.PublishedAt,
                    createdAt = x.CreatedAt
                })
                .ToListAsync();

            return Json(notices);
        }


        [HttpGet]
        public async Task<IActionResult> GetNotificationCount()
        {
            var userId = User.FindFirstValue(
                ClaimTypes.NameIdentifier);

            if (string.IsNullOrWhiteSpace(userId))
            {
                return Unauthorized();
            }

            var count = await _context.Notices
                .CountAsync(x =>
                    x.IsActive &&
                    (
                        x.NoticeType == "Public" ||
                        (
                            x.NoticeType == "Personal" &&
                            x.UserId == userId
                        )
                    )
                );

            return Json(new
            {
                count
            });
        }
    }
}
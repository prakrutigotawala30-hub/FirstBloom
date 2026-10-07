using FirstBloom.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FirstBloom.Controllers
{
    public class ProgramController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ProgramController(ApplicationDbContext context)
        {
            _context = context;
        }


        // =========================================================
        // PROGRAM INDEX
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var programs = await _context.Programs
                .Where(p => p.IsActive)
                .OrderBy(p => p.Id)
                .ToListAsync();

            return View(programs);
        }


        // =========================================================
        // PROGRAM DETAILS
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var program = await _context.Programs
                .FirstOrDefaultAsync(p =>
                    p.Id == id &&
                    p.IsActive);

            if (program == null)
            {
                return NotFound();
            }

            return View(program);
        }


        // =========================================================
        // LIVE PROGRAM SEARCH
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> LiveSearch(string query)
        {
            // -----------------------------------------------------
            // Empty search
            // -----------------------------------------------------

            if (string.IsNullOrWhiteSpace(query))
            {
                return Json(new List<object>());
            }


            // -----------------------------------------------------
            // Normalize search text
            // -----------------------------------------------------

            string search = NormalizeSearch(query);


            if (string.IsNullOrWhiteSpace(search))
            {
                return Json(new List<object>());
            }


            // -----------------------------------------------------
            // Get active programs
            // -----------------------------------------------------

            var programs = await _context.Programs
                .Where(p => p.IsActive)
                .Select(p => new
                {
                    p.Id,
                    p.ProgramName,
                    p.AgeGroup,
                    p.Duration,
                    p.ImageUrl
                })
                .ToListAsync();


            // -----------------------------------------------------
            // Find matching programs
            // -----------------------------------------------------

            var results = programs
                .Where(program =>
                {
                    string programName =
                        NormalizeProgramName(program.ProgramName);

                    // -------------------------------------------------
                    // Normal partial search
                    //
                    // Junior KG
                    // junior
                    // JUNIOR
                    // juni
                    // jun
                    // j
                    // kg
                    // -------------------------------------------------

                    if (programName.Contains(search))
                    {
                        return true;
                    }


                    // -------------------------------------------------
                    // JR = JUNIOR
                    // -------------------------------------------------

                    if (search.Contains("jr"))
                    {
                        string juniorSearch =
                            search.Replace("jr", "junior");

                        if (programName.Contains(juniorSearch))
                        {
                            return true;
                        }
                    }


                    // -------------------------------------------------
                    // SR = SENIOR
                    // -------------------------------------------------

                    if (search.Contains("sr"))
                    {
                        string seniorSearch =
                            search.Replace("sr", "senior");

                        if (programName.Contains(seniorSearch))
                        {
                            return true;
                        }
                    }


                    return false;
                })
                .OrderBy(program => program.Id)
                .Take(8)
                .Select(program => new
                {
                    id = program.Id,

                    name = program.ProgramName,

                    ageGroup = program.AgeGroup,

                    duration = program.Duration,

                    imageUrl = program.ImageUrl,

                    url = Url.Action(
                        "Details",
                        "Program",
                        new
                        {
                            id = program.Id
                        })
                })
                .ToList();


            return Json(results);
        }


        // =========================================================
        // NORMALIZE PROGRAM NAME
        // =========================================================

        private static string NormalizeProgramName(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return string.Empty;
            }

            string result = value
                .ToLowerInvariant()
                .Trim();


            // -----------------------------------------------------
            // Convert common short forms
            // -----------------------------------------------------

            result = result
                .Replace("jr.", "junior")
                .Replace("jr", "junior")
                .Replace("sr.", "senior")
                .Replace("sr", "senior");


            // -----------------------------------------------------
            // Remove extra spaces
            // -----------------------------------------------------

            result = string.Join(
                " ",
                result.Split(
                    ' ',
                    StringSplitOptions.RemoveEmptyEntries));


            return result;
        }


        // =========================================================
        // NORMALIZE USER SEARCH
        // =========================================================

        private static string NormalizeSearch(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return string.Empty;
            }

            string result = value
                .ToLowerInvariant()
                .Trim();


            // -----------------------------------------------------
            // Common short forms
            // -----------------------------------------------------

            result = result
                .Replace("jr.", "jr")
                .Replace("sr.", "sr");


            // -----------------------------------------------------
            // Remove extra spaces
            // -----------------------------------------------------

            result = string.Join(
                " ",
                result.Split(
                    ' ',
                    StringSplitOptions.RemoveEmptyEntries));


            return result;
        }
    }
}
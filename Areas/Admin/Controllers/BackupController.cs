using FirstBloom.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.Sqlite;
using System.Data;
using System.Text.RegularExpressions;

namespace FirstBloom.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class BackupController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _environment;
        private readonly ApplicationDbContext _context;

        public BackupController(
            IConfiguration configuration,
            IWebHostEnvironment environment,
            ApplicationDbContext context)
        {
            _configuration = configuration;
            _environment = environment;
            _context = context;
        }

        // =========================================================
        // BACKUP PAGE
        // =========================================================

        [HttpGet]
        public IActionResult Index()
        {
            var backups = GetBackupFiles();

            return View(backups);
        }

        // =========================================================
        // SQL SERVER BACKUP
        // =========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BackupSqlServer()
        {
            try
            {
                var connectionString =
                    _configuration.GetConnectionString("DefaultConnection");

                if (string.IsNullOrWhiteSpace(connectionString))
                {
                    TempData["ErrorMessage"] =
                        "SQL Server connection string was not found.";

                    return RedirectToAction(nameof(Index));
                }

                // Make sure this is actually a SQL Server connection
                if (connectionString.Contains("Data Source=") &&
                    connectionString.EndsWith(".db",
                        StringComparison.OrdinalIgnoreCase))
                {
                    TempData["ErrorMessage"] =
                        "The current DefaultConnection is configured for SQLite, not SQL Server.";

                    return RedirectToAction(nameof(Index));
                }

                var builder =
                    new SqlConnectionStringBuilder(connectionString);

                var databaseName = builder.InitialCatalog;

                if (string.IsNullOrWhiteSpace(databaseName))
                {
                    TempData["ErrorMessage"] =
                        "SQL Server database name could not be detected.";

                    return RedirectToAction(nameof(Index));
                }

                var backupFolder =
                    GetBackupFolder("SqlServer");

                Directory.CreateDirectory(backupFolder);

                var timestamp =
                    DateTime.Now.ToString("yyyyMMdd_HHmmss");

                var safeDatabaseName =
                    MakeSafeFileName(databaseName);

                var backupFileName =
                    $"{safeDatabaseName}_{timestamp}.bak";

                var backupPath =
                    Path.Combine(
                        backupFolder,
                        backupFileName);

                // SQL Server BACKUP DATABASE requires the path
                // to be accessible by the SQL Server service.
                var sqlBackupPath =
                    backupPath.Replace("'", "''");

                await using var connection =
                    new SqlConnection(connectionString);

                await connection.OpenAsync();

                var commandText = $@"
BACKUP DATABASE [{databaseName.Replace("]", "]]")}]
TO DISK = N'{sqlBackupPath}'
WITH COPY_ONLY,
     INIT,
     FORMAT,
     STATS = 10;";

                await using var command =
                    new SqlCommand(
                        commandText,
                        connection);

                command.CommandTimeout = 600;

                await command.ExecuteNonQueryAsync();

                TempData["SuccessMessage"] =
                    $"SQL Server backup created successfully: {backupFileName}";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] =
                    $"SQL Server backup failed: {ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }

        // =========================================================
        // SQLITE BACKUP
        // =========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BackupSqlite()
        {
            try
            {
                var connectionString =
                    _configuration.GetConnectionString("SqliteConnection");

                if (string.IsNullOrWhiteSpace(connectionString))
                {
                    TempData["ErrorMessage"] =
                        "SQLite connection string was not found.";

                    return RedirectToAction(nameof(Index));
                }

                var builder =
                    new SqliteConnectionStringBuilder(
                        connectionString);

                var databasePath =
                    builder.DataSource;

                if (string.IsNullOrWhiteSpace(databasePath))
                {
                    TempData["ErrorMessage"] =
                        "SQLite database path could not be detected.";

                    return RedirectToAction(nameof(Index));
                }

                // Convert relative SQLite path into absolute path
                if (!Path.IsPathRooted(databasePath))
                {
                    databasePath =
                        Path.Combine(
                            _environment.ContentRootPath,
                            databasePath);
                }

                databasePath =
                    Path.GetFullPath(databasePath);

                if (!System.IO.File.Exists(databasePath))
                {
                    TempData["ErrorMessage"] =
                        $"SQLite database was not found: {databasePath}";

                    return RedirectToAction(nameof(Index));
                }

                var backupFolder =
                    GetBackupFolder("SQLite");

                Directory.CreateDirectory(backupFolder);

                var timestamp =
                    DateTime.Now.ToString("yyyyMMdd_HHmmss");

                var backupFileName =
                    $"FirstBloomDB_{timestamp}.db";

                var backupPath =
                    Path.Combine(
                        backupFolder,
                        backupFileName);

                // SQLite VACUUM INTO requires a new/non-existing file.
                await using var connection =
                    new SqliteConnection(
                        connectionString);

                await connection.OpenAsync();

                var escapedBackupPath =
                    backupPath.Replace("'", "''");

                var commandText =
                    $"VACUUM INTO '{escapedBackupPath}';";

                await using var command =
                    connection.CreateCommand();

                command.CommandText =
                    commandText;

                await command.ExecuteNonQueryAsync();

                TempData["SuccessMessage"] =
                    $"SQLite backup created successfully: {backupFileName}";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] =
                    $"SQLite backup failed: {ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }

        // =========================================================
        // DOWNLOAD BACKUP
        // =========================================================

        [HttpGet]
        public IActionResult Download(
            string type,
            string fileName)
        {
            if (string.IsNullOrWhiteSpace(type) ||
                string.IsNullOrWhiteSpace(fileName))
            {
                return BadRequest();
            }

            var allowedTypes =
                new[] { "SqlServer", "SQLite" };

            if (!allowedTypes.Contains(
                    type,
                    StringComparer.OrdinalIgnoreCase))
            {
                return BadRequest();
            }

            // Prevent path traversal
            var safeFileName =
                Path.GetFileName(fileName);

            var backupFolder =
                GetBackupFolder(type);

            var filePath =
                Path.Combine(
                    backupFolder,
                    safeFileName);

            if (!System.IO.File.Exists(filePath))
            {
                return NotFound();
            }

            var extension =
                Path.GetExtension(filePath)
                    .ToLowerInvariant();

            var contentType =
                extension == ".bak"
                    ? "application/octet-stream"
                    : "application/x-sqlite3";

            return PhysicalFile(
                filePath,
                contentType,
                safeFileName);
        }

        // =========================================================
        // DELETE BACKUP
        // =========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(
            string type,
            string fileName)
        {
            if (string.IsNullOrWhiteSpace(type) ||
                string.IsNullOrWhiteSpace(fileName))
            {
                return BadRequest();
            }

            var allowedTypes =
                new[] { "SqlServer", "SQLite" };

            if (!allowedTypes.Contains(
                    type,
                    StringComparer.OrdinalIgnoreCase))
            {
                return BadRequest();
            }

            var safeFileName =
                Path.GetFileName(fileName);

            var backupFolder =
                GetBackupFolder(type);

            var filePath =
                Path.Combine(
                    backupFolder,
                    safeFileName);

            if (System.IO.File.Exists(filePath))
            {
                System.IO.File.Delete(filePath);

                TempData["SuccessMessage"] =
                    $"Backup deleted successfully: {safeFileName}";
            }
            else
            {
                TempData["ErrorMessage"] =
                    "Backup file was not found.";
            }

            return RedirectToAction(nameof(Index));
        }

        // =========================================================
        // GET BACKUP FILES
        // =========================================================

        private List<BackupFileViewModel> GetBackupFiles()
        {
            var result =
                new List<BackupFileViewModel>();

            AddBackupFiles(
                result,
                "SqlServer");

            AddBackupFiles(
                result,
                "SQLite");

            return result
                .OrderByDescending(x => x.CreatedAt)
                .ToList();
        }

        private void AddBackupFiles(
            List<BackupFileViewModel> result,
            string type)
        {
            var folder =
                GetBackupFolder(type);

            if (!Directory.Exists(folder))
            {
                return;
            }

            var files =
                Directory.GetFiles(folder);

            foreach (var file in files)
            {
                var info =
                    new FileInfo(file);

                result.Add(
                    new BackupFileViewModel
                    {
                        FileName = info.Name,
                        Type = type,
                        FullPath = info.FullName,
                        Size = info.Length,
                        CreatedAt = info.CreationTime
                    });
            }
        }

        // =========================================================
        // BACKUP FOLDER
        // =========================================================

        private string GetBackupFolder(string type)
        {
            return Path.Combine(
                _environment.ContentRootPath,
                "Backups",
                type);
        }

        // =========================================================
        // SAFE FILE NAME
        // =========================================================

        private string MakeSafeFileName(
            string fileName)
        {
            foreach (var character in
                     Path.GetInvalidFileNameChars())
            {
                fileName =
                    fileName.Replace(
                        character,
                        '_');
            }

            return fileName;
        }

        // =========================================================
        // VIEW MODEL
        // =========================================================

        public class BackupFileViewModel
        {
            public string FileName { get; set; } = string.Empty;

            public string Type { get; set; } = string.Empty;

            public string FullPath { get; set; } = string.Empty;

            public long Size { get; set; }

            public DateTime CreatedAt { get; set; }

            public string SizeFormatted
            {
                get
                {
                    if (Size < 1024)
                        return $"{Size} B";

                    if (Size < 1024 * 1024)
                        return $"{Size / 1024.0:N2} KB";

                    if (Size < 1024 * 1024 * 1024)
                        return $"{Size / (1024.0 * 1024.0):N2} MB";

                    return $"{Size / (1024.0 * 1024.0 * 1024.0):N2} GB";
                }
            }
        }
    }
}
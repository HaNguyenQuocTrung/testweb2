using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QRCoder;
using System.Drawing.Imaging;
using System.IO;
using System.Drawing;
using UrlShortener.Data;
using UrlShortener.Models;
using System.Text.RegularExpressions;

namespace UrlShortener.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;

        public HomeController(ApplicationDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        public async Task<IActionResult> Index()
        {
            // Load all saved URLs to display in the table
            var savedUrls = await _context.ShortUrls
                .OrderByDescending(u => u.Id)
                .ToListAsync();
                
            ViewBag.SavedUrls = savedUrls;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(string urlName, string originalUrl, string? customAlias = null)
        {
            if (string.IsNullOrWhiteSpace(originalUrl) || string.IsNullOrWhiteSpace(urlName))
            {
                TempData["Error"] = "URL and Name cannot be empty.";
                return RedirectToAction("Index");
            }

            // Validate URL format
            if (!Uri.TryCreate(originalUrl, UriKind.Absolute, out _))
            {
                TempData["Error"] = "Please enter a valid URL (e.g., https://example.com)";
                return RedirectToAction("Index");
            }

            string shortCode;

            if (!string.IsNullOrWhiteSpace(customAlias))
            {
                // Validate custom alias format
                if (!Regex.IsMatch(customAlias, @"^[a-zA-Z0-9_-]+$"))
                {
                    TempData["Error"] = "Custom alias can only contain letters, numbers, hyphens, and underscores";
                    return RedirectToAction("Index");
                }

                // Check if alias already exists
                if (await _context.ShortUrls.AnyAsync(u => u.ShortCode == customAlias))
                {
                    TempData["Error"] = "This alias is already taken. Please choose another one.";
                    return RedirectToAction("Index");
                }
                shortCode = customAlias;
            }
            else
            {
                // Generate random short code if no custom alias provided
                shortCode = Guid.NewGuid().ToString("N")[..6];
                
                // Ensure the generated code is unique
                while (await _context.ShortUrls.AnyAsync(u => u.ShortCode == shortCode))
                {
                    shortCode = Guid.NewGuid().ToString("N")[..6];
                }
            }

            var url = new ShortUrl
            {
                Name = urlName.Trim(),
                OriginalUrl = originalUrl,
                ShortCode = shortCode,
                CreatedAt = DateTime.UtcNow,
                ClickCount = 0
            };

            _context.ShortUrls.Add(url);
            await _context.SaveChangesAsync();

            TempData["Success"] = "URL has been shortened successfully!";
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> RedirectToOriginal(string shortCode)
        {
            var url = await _context.ShortUrls
                .FirstOrDefaultAsync(u => u.ShortCode == shortCode);

            if (url == null)
            {
                return NotFound();
            }

            // Update click count
            url.ClickCount++;
            _context.Update(url);
            await _context.SaveChangesAsync();

            // Make sure the URL has a scheme
            var originalUrl = url.OriginalUrl;
            if (!originalUrl.StartsWith("http://") && !originalUrl.StartsWith("https://"))
            {
                originalUrl = "http://" + originalUrl;
            }

            return Redirect(originalUrl);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var url = await _context.ShortUrls.FindAsync(id);
            if (url == null)
            {
                TempData["Error"] = "URL not found or already deleted.";
                return RedirectToAction("Index");
            }

            _context.ShortUrls.Remove(url);
            await _context.SaveChangesAsync();

            TempData["Success"] = "URL has been deleted successfully!";
            return RedirectToAction("Index");
        }

        [HttpGet("qrcode/{shortCode}")]
        public IActionResult GenerateQrCode(string shortCode)
        {
            var url = _context.ShortUrls.FirstOrDefault(u => u.ShortCode == shortCode);
            if (url == null) return NotFound();

            // Get the local IP address
            var hostEntry = System.Net.Dns.GetHostEntry(System.Net.Dns.GetHostName());
            var ipAddress = hostEntry.AddressList
                .FirstOrDefault(ip => ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)?.ToString() 
                ?? "localhost";

            var baseUrl = $"{Request.Scheme}://{ipAddress}:{Request.Host.Port ?? 80}";
            var fullUrl = $"{baseUrl}/{shortCode}";

            var qrGenerator = new QRCodeGenerator();
            var qrCodeData = qrGenerator.CreateQrCode(fullUrl, QRCodeGenerator.ECCLevel.Q);
            var qrCode = new PngByteQRCode(qrCodeData);
            var qrCodeBytes = qrCode.GetGraphic(20);

            return File(qrCodeBytes, "image/png");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = System.Diagnostics.Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}

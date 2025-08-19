using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UrlShortener.Data;
using UrlShortener.Models;

namespace UrlShortener.Controllers.Api
{
    [Route("api/url")]
    [ApiController]
    public class UrlController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private const string BaseUrl = "http://localhost:5226/";

        public UrlController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost("shorten")]
        public async Task<IActionResult> ShortenUrl([FromBody] ShortenUrlRequest request)
        {
            if (!Uri.TryCreate(request.OriginalUrl, UriKind.Absolute, out _))
            {
                return BadRequest("The specified URL is invalid.");
            }

            string shortCode;

            if (!string.IsNullOrEmpty(request.CustomAlias))
            {
                // Check if alias already exists
                if (await _context.ShortUrls.AnyAsync(u => u.ShortCode == request.CustomAlias))
                {
                    return BadRequest("Alias already taken. Please choose another.");
                }
                shortCode = request.CustomAlias;
            }
            else
            {
                // Generate random short code if user did not provide one
                shortCode = Guid.NewGuid().ToString("N")[..6];
                
                // Ensure the generated code is unique
                while (await _context.ShortUrls.AnyAsync(u => u.ShortCode == shortCode))
                {
                    shortCode = Guid.NewGuid().ToString("N")[..6];
                }
            }

            var url = new ShortUrl
            {
                OriginalUrl = request.OriginalUrl,
                ShortCode = shortCode,
                CreatedAt = DateTime.UtcNow
            };

            _context.ShortUrls.Add(url);
            await _context.SaveChangesAsync();

            return Ok(new { ShortUrl = $"{BaseUrl}{shortCode}" });
        }

        [HttpGet("{shortCode}")]
        public async Task<IActionResult> RedirectUrl(string shortCode)
        {
            var url = await _context.ShortUrls
                .FirstOrDefaultAsync(u => u.ShortCode == shortCode);

            if (url == null)
            {
                return NotFound("Short URL not found");
            }

            // Increment click count
            url.ClickCount++;
            await _context.SaveChangesAsync();

            return Redirect(url.OriginalUrl);
        }
    }
}

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ParkifyAPI.Data.Contexts;
using ParkifyAPI.Common.Model;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Linq;
using System.Net.Http;

namespace ParkifyAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ComplaintsController : ControllerBase
    {
        private readonly ParkifyDbContext _context;
        
        private readonly IHttpClientFactory _httpClientFactory;
        //private readonly string _ocrApiUrl = "http://13.51.15.3:8001/run-ocr";
        private readonly string _ocrApiUrl = "http://51.21.118.19:8001/run-ocr";


        public ComplaintsController(ParkifyDbContext context, IHttpClientFactory httpClientFactory)
        {
            _context = context;
            _httpClientFactory = httpClientFactory;
        }

        public class ComplaintRequest
        {
            public string Email { get; set; }
            public int LotId { get; set; }
            public string SpaceNumber { get; set; }
            public string LicensePlateDetected { get; set; }
            public string ImageBase64 { get; set; }
        }

        [HttpPost("Report")]
        public async Task<IActionResult> ReportComplaint([FromBody] ComplaintRequest request)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
            if (user == null)
                return NotFound("User not found.");

            var now = DateTime.UtcNow;

            var hasReservation = await _context.Reservations.AnyAsync(r =>
                r.UserId == user.Id &&
                r.IsActive &&
                r.LotId == request.LotId &&
                r.SpaceNumber == request.SpaceNumber &&
                r.EndTime > now);

            if (!hasReservation)
                return BadRequest("You can only report a complaint for your active reservation.");

            // Base64 kontrolü
            byte[] imageBytes;
            try
            {
                imageBytes = Convert.FromBase64String(request.ImageBase64);
            }
            catch (FormatException)
            {
                return BadRequest("Invalid base64 image data.");
            }

            var complaint = new Complaint
            {
                UserId = user.Id,
                LotId = request.LotId,
                SpaceNumber = request.SpaceNumber,
                LicensePlateDetected = request.LicensePlateDetected,
                ImageData = imageBytes, // BLOB olarak sakla
                CreatedAt = now,
                IsResolved = false
            };

            _context.Complaints.Add(complaint);
            await _context.SaveChangesAsync();
            
            _ = Task.Run(async () => await TriggerOcrProcessing());

            return Ok("Complaint submitted successfully.");
        }
        
        private async Task TriggerOcrProcessing()
        {
            try
            {
                // Her seferinde yeni HttpClient oluştur ve otomatik dispose et
                using var httpClient = _httpClientFactory.CreateClient();
                httpClient.Timeout = TimeSpan.FromMinutes(5);
                
                var response = await httpClient.PostAsync(_ocrApiUrl, null);
                if (response.IsSuccessStatusCode)
                {
                    Console.WriteLine("✅ OCR işlemi başarıyla tetiklendi.");
                }
                else
                {
                    Console.WriteLine($"❌ OCR tetikleme hatası: {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ OCR API çağrısı hatası: {ex.Message}");
            }
        }

        [HttpGet("GetByLot/{lotId}")]
        public async Task<IActionResult> GetComplaintsByLot(int lotId)
        {
            var complaints = await _context.Complaints
                .Include(c => c.User)
                .Where(c => c.LotId == lotId)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();

            var result = complaints.Select(c => new
            {
                c.Id,
                UserEmail = c.User.Email,
                c.LotId,
                c.SpaceNumber,
                c.LicensePlateDetected,
                ImageBase64 = c.ImageData != null ? Convert.ToBase64String(c.ImageData) : null,
                c.CreatedAt,
                Status = c.IsResolved ? "Resolved" : "Pending"
            });

            return Ok(result);
        }

        [HttpGet("GetByUser/{userEmail}")]
        public async Task<IActionResult> GetComplaintsByUser(string userEmail)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == userEmail);
            if (user == null)
                return Unauthorized("User not found.");

            var complaints = await _context.Complaints
                .Where(c => c.UserId == user.Id)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();

            var result = complaints.Select(c => new
            {
                c.Id,
                c.LicensePlateDetected,
                c.SpaceNumber,
                ImageBase64 = c.ImageData != null ? Convert.ToBase64String(c.ImageData) : null,
                c.CreatedAt,
                Status = c.IsResolved ? "Resolved" : "Pending"
            });

            return Ok(result);
        }
        
        [HttpGet("GetUserComplaintsWithLotName/{userEmail}")]
        public async Task<IActionResult> GetUserComplaintsWithLotName(string userEmail)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == userEmail);
            if (user == null)
                return NotFound("User not found.");

            var complaints = await _context.Complaints
                .Include(c => c.ParkingLot)
                .Where(c => c.UserId == user.Id)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();

            var result = complaints.Select(c => new
            {
                c.Id,
                LotName = c.ParkingLot?.Name,
                c.SpaceNumber,
                c.CreatedAt,
                Status = c.IsResolved ? "Resolved" : "Pending"
            });

            return Ok(result);
        }


        [HttpPut("MarkAsResolved")]
        public async Task<IActionResult> MarkAsResolved(int complaintId, string adminEmail)
        {
            var admin = await _context.Administrators.FirstOrDefaultAsync(a => a.Email == adminEmail);
            if (admin == null)
                return Unauthorized("Admin not found.");

            if (admin.LotId == null)
                return BadRequest("This admin has no assigned lot.");

            var complaint = await _context.Complaints.FirstOrDefaultAsync(c => c.Id == complaintId);
            if (complaint == null)
                return NotFound("Complaint not found.");

            if (complaint.LotId != admin.LotId)
                return Forbid("You are not authorized to resolve this complaint.");

            if (complaint.IsResolved)
                return BadRequest("Complaint is already resolved.");

            complaint.IsResolved = true;
            await _context.SaveChangesAsync();

            return Ok("Complaint marked as resolved.");
        }
    }
}

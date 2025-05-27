using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ParkifyAPI.Data.Contexts;
using ParkifyAPI.Common.Model;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Linq;

namespace ParkifyAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ComplaintsController : ControllerBase
    {
        private readonly ParkifyDbContext _context;
        private readonly IWebHostEnvironment _env;

        public ComplaintsController(ParkifyDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
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

            // Dosya kaydı
            string fileName = Guid.NewGuid().ToString() + ".jpg";
            string folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "complaints");
            Directory.CreateDirectory(folderPath);
            string filePath = Path.Combine(folderPath, fileName);

            await System.IO.File.WriteAllBytesAsync(filePath, imageBytes);

            var complaint = new Complaint
            {
                UserId = user.Id,
                LotId = request.LotId,
                SpaceNumber = request.SpaceNumber,
                LicensePlateDetected = request.LicensePlateDetected,
                ImagePath = Path.Combine("images", "complaints", fileName),
                CreatedAt = now,
                IsResolved = false
            };

            _context.Complaints.Add(complaint);
            await _context.SaveChangesAsync();

            return Ok("Complaint submitted successfully.");
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
                c.ImagePath, // örneğin: images/complaints/abc123.jpg
                c.CreatedAt,
                Status = c.IsResolved ? "Resolved" : "Pending"
            });

            return Ok(result);
        }
        
        [HttpPut("MarkAsResolved")]
        public async Task<IActionResult> MarkAsResolved(int complaintId, string adminEmail)
        {
            // Admin'i bul
            var admin = await _context.Administrators.FirstOrDefaultAsync(a => a.Email == adminEmail);
            if (admin == null)
                return Unauthorized("Admin not found.");

            if (admin.LotId == null)
                return BadRequest("This admin has no assigned lot.");

            // Şikayeti bul
            var complaint = await _context.Complaints.FirstOrDefaultAsync(c => c.Id == complaintId);
            if (complaint == null)
                return NotFound("Complaint not found.");

            // Şikayet bu adminin otoparkına mı ait?
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

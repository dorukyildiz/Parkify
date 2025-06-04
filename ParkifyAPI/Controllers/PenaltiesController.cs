using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ParkifyAPI.Data.Contexts;
using ParkifyAPI.Common.Model;


namespace ParkifyAPI.Controllers
{

    [ApiController]
    [Route("api/[controller]")]
    public class PenaltiesController : ControllerBase
    {
        private readonly ParkifyDbContext _context;

        public PenaltiesController(ParkifyDbContext context)
        {
            _context = context;
        }

        public class PenaltyRequest
        {
            public string AdminEmail { get; set; }
            public int ComplaintId { get; set; }
            public string PlateNumber { get; set; }
            public string Reason { get; set; }
            public int PenaltyPoints { get; set; }
        }

        // Ceza verme
        [HttpPost("Issue")]
        public async Task<IActionResult> IssuePenalty([FromBody] PenaltyRequest request)
        {
            var admin = await _context.Administrators.FirstOrDefaultAsync(a => a.Email == request.AdminEmail);
            if (admin == null)
                return Unauthorized("Admin not found.");

            var complaint = await _context.Complaints.FirstOrDefaultAsync(c => c.Id == request.ComplaintId);
            if (complaint == null)
                return NotFound("Complaint not found.");

            if (admin.LotId != complaint.LotId)
                return Forbid("You are not authorized to issue a penalty for this lot.");

            var penalty = new Penalty
            {
                ComplaintId = request.ComplaintId,
                AdminId = admin.AdminId,
                PlateNumber = request.PlateNumber,
                Reason = request.Reason,
                PenaltyPoints = request.PenaltyPoints,
                CreatedAt = DateTime.UtcNow
            };

            _context.Penalties.Add(penalty);
            await _context.SaveChangesAsync();

            return Ok("Penalty issued successfully.");
        }


        
        // Kullanıcının plakasına ait tüm cezalar
        [HttpGet("GetByPlate/{plate}")]
        public async Task<IActionResult> GetPenaltiesByPlate(string plate)
        {
            var penalties = await _context.Penalties
                .Include(p => p.Complaint)
                .Where(p => p.PlateNumber == plate)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            var result = penalties.Select(p => new
            {
                p.Id,
                p.Reason,
                p.PenaltyPoints,
                p.CreatedAt,
                ComplaintId = p.ComplaintId,
                ImageUrl = p.Complaint.ImagePath
            });

            return Ok(result);
        }
        
        
        // Admin'in kendi yazdığı tüm cezaları getir
        [HttpGet("GetByAdmin/{adminEmail}")]
        public async Task<IActionResult> GetPenaltiesByAdmin(string adminEmail)
        {
            var admin = await _context.Administrators.FirstOrDefaultAsync(a => a.Email == adminEmail);
            if (admin == null)
                return Unauthorized("Admin not found.");

            var penalties = await _context.Penalties
                .Where(p => p.AdminId == admin.AdminId)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            var result = penalties.Select(p => new
            {
                p.Id,
                p.PlateNumber,
                p.Reason,
                p.PenaltyPoints,
                p.CreatedAt
            });

            return Ok(result);
        }
        
        [HttpGet("GetTotalPointsByPlate/{plate}")]
        public async Task<IActionResult> GetTotalPointsByPlate(string plate)
        {
            var totalPoints = await _context.Penalties
                .Where(p => p.PlateNumber == plate)
                .SumAsync(p => (int?)p.PenaltyPoints) ?? 0;

            return Ok(new { plate, totalPoints });
        }



    }
}

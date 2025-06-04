using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ParkifyAPI.Data.Contexts;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace ParkifyAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ParkingLotsController : ControllerBase
    {
        private readonly ParkifyDbContext _context;

        public ParkingLotsController(ParkifyDbContext context)
        {
            _context = context;
        }

        // Tüm otoparkları döner (kat bilgisi ve layout dahil)
        [HttpGet]
        public async Task<IActionResult> GetAllLots()
        {
            var lots = await _context.ParkingLots.ToListAsync();

            var result = lots.Select(lot => new
            {
                lot.LotId,
                lot.Name,
                lot.Location,
                lot.TotalSpots,
                lot.NumOfFloors,
                Layout = string.IsNullOrEmpty(lot.Layout)
                    ? null
                    : JsonSerializer.Deserialize<object>(lot.Layout) // JSON'u nesne olarak döner
            });

            return Ok(result);
        }

        // Belirli bir otoparkı ID'ye göre döner (kat bilgisi ve layout dahil)
        [HttpGet("{id}")]
        public async Task<IActionResult> GetLotById(int id)
        {
            var lot = await _context.ParkingLots.FindAsync(id);

            if (lot == null)
                return NotFound("Otopark bulunamadı.");

            var result = new
            {
                lot.LotId,
                lot.Name,
                lot.Location,
                lot.TotalSpots,
                lot.NumOfFloors,
                Layout = string.IsNullOrEmpty(lot.Layout)
                    ? null
                    : JsonSerializer.Deserialize<object>(lot.Layout)
            };

            return Ok(result);
        }
    }
}
using Microsoft.AspNetCore.Mvc;
using ParkifyAPI.Data.Contexts;
using System.Linq;

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

        [HttpGet]
        public IActionResult GetAllLots()
        {
            var lots = _context.ParkingLots
                .Select(lot => new
                {
                    lot.LotId,
                    lot.Name
                }).ToList();

            return Ok(lots);
        }
    }
}
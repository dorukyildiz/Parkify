using Microsoft.AspNetCore.Mvc;
using ParkifyAPI.Common.Model;
using ParkifyAPI.Data.Contexts;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace ParkifyAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FavoriteController : ControllerBase
    {
        private readonly ParkifyDbContext _context;

        public FavoriteController(ParkifyDbContext context)
        {
            _context = context;
        }

        // Favori ekleme
        [HttpPost("addFavorite")]
        public IActionResult AddFavorite([FromQuery] string userEmail, [FromQuery] int lotId)
        {
            var user = _context.Users.FirstOrDefault(u => u.Email == userEmail);
            if (user == null)
                return NotFound("User not found.");

            var lot = _context.ParkingLots.FirstOrDefault(l => l.LotId == lotId);
            if (lot == null)
                return NotFound("Parking lot not found.");

            var existingFavorite = _context.FavoriteParkingLots
                .FirstOrDefault(f => f.UserId == user.Id && f.LotId == lotId);

            if (existingFavorite != null)
                return BadRequest("This parking lot is already a favorite.");

            var newFavorite = new FavoriteParkingLot
            {
                UserId = user.Id,
                LotId = lotId
            };

            _context.FavoriteParkingLots.Add(newFavorite);
            _context.SaveChanges();

            return Ok("Favorite parking lot added successfully.");
        }

        // Favorileri listeleme
        [HttpGet("getFavorites/{userId}")]
        public async Task<IActionResult> GetFavorites(int userId)
        {
            var favorites = await _context.FavoriteParkingLots
                .Include(f => f.Lot)
                .Where(f => f.UserId == userId)
                .Select(f => new
                {
                    lotId = f.Lot.LotId,
                    name = f.Lot.Name,
                    location = f.Lot.Location,
                    totalSpots = f.Lot.TotalSpots
                })
                .ToListAsync();

            return Ok(favorites);
        }



        // Favori silme
        [HttpDelete("removeFavorite")]
        public IActionResult RemoveFavorite([FromQuery] string userEmail, [FromQuery] int lotId)
        {
            var user = _context.Users.FirstOrDefault(u => u.Email == userEmail);
            if (user == null)
                return NotFound("User not found.");

            var favorite = _context.FavoriteParkingLots
                .FirstOrDefault(f => f.UserId == user.Id && f.LotId == lotId);

            if (favorite == null)
                return NotFound("Favorite not found.");

            _context.FavoriteParkingLots.Remove(favorite);
            _context.SaveChanges();

            return Ok("Favorite removed successfully.");
        }
    }
}

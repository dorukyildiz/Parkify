using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ParkifyAPI.DataAccess.Interfaces;
using ParkifyAPI.Common.Model;
//API endpointlerinin gösterimi
namespace ParkifyAPI.Controllers
{
    using ParkifyAPI.Data.Contexts;

    [Route("api/[controller]")]
    [ApiController]
    public class ParkingSpacesController : ControllerBase
    {
        private readonly IGenericRepository<ParkingSpace> _parkingSpacesRepository;
        private readonly ParkifyDbContext _context;


        public ParkingSpacesController(IGenericRepository<ParkingSpace> parkingSpacesRepository, ParkifyDbContext context)
        {
            _parkingSpacesRepository = parkingSpacesRepository;
            _context = context;
        }

        [HttpGet("GetAllParkingSpaces")]
        public async Task<IActionResult> GetAllParkingSpaces()
        {
            var parkingSpaces = await _parkingSpacesRepository.GetAllAsync();
            return Ok(parkingSpaces);
        }

        [HttpGet("GetParkingSpacesByLotId/{lotId}")]
        public async Task<IActionResult> GetParkingSpacesByLotId(int lotId)
        {
            var parkingSpaces = await _parkingSpacesRepository.FindAsync(ps => ps.LotId == lotId);

            if (!parkingSpaces.Any())
            {
                return NotFound($"Lot ID {lotId} için herhangi bir park alanı bulunamadı.");
            }

            return Ok(parkingSpaces);
        }

        [HttpGet("GetOccupiedParkingSpacesByLotId/{lotId}")]
        public async Task<IActionResult> GetOccupiedParkingSpacesByLotId(int lotId)
        {
            var occupiedSpaces = await _parkingSpacesRepository.FindAsync(ps => ps.LotId == lotId && ps.IsOccupied);

            if (!occupiedSpaces.Any())
            {
                return NotFound($"Lot ID {lotId} için dolu park alanı bulunamadı.");
            }

            return Ok(occupiedSpaces);
        }

        [HttpPut("ReserveParkingSpace")]
        public async Task<IActionResult> ReserveParkingSpace(string email, int lotId, string spaceNumber)
        {
            // 1. Kullanıcıyı e-posta ile bul
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);

            if (user == null)
            {
                return NotFound($"User with email '{email}' not found.");
            }

            // 2. Park yerini bul (lotId + spaceNumber)
            var parkingSpaces = await _parkingSpacesRepository.FindAsync(ps => ps.LotId == lotId && ps.SpaceNumber == spaceNumber);
            var parkingSpace = parkingSpaces.FirstOrDefault();

            if (parkingSpace == null)
            {
                return NotFound($"Parking space '{spaceNumber}' not found in lot {lotId}.");
            }

            // 3. Uygunluk kontrolü
            if (parkingSpace.IsOccupied)
            {
                return BadRequest($"Parking space '{spaceNumber}' is currently occupied.");
            }

            if (parkingSpace.IsReserved)
            {
                return BadRequest($"Parking space '{spaceNumber}' is already reserved.");
            }

            // 4. Rezervasyonu yap
            parkingSpace.IsReserved = true;
            parkingSpace.PlateNumber = user.LicensePlate;

            await _parkingSpacesRepository.UpdateAsync(parkingSpace);

            return Ok($"Parking space '{spaceNumber}' reserved for plate: {user.LicensePlate}.");
        }


        [HttpGet("GetAllReservedParkingSpaces")]
        public async Task<IActionResult> GetAllReservedParkingSpaces()
        {
            var reservedSpaces = await _parkingSpacesRepository.FindAsync(ps => ps.IsReserved);

            if (!reservedSpaces.Any())
            {
                return NotFound("Hiçbir rezerve edilmiş park yeri bulunamadı.");
            }

            return Ok(reservedSpaces);
        }
    }
}

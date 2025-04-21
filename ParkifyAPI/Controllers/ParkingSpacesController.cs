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
        public async Task<IActionResult> ReserveParkingSpace(int lotId, string spaceNumber, string plateNumber)
        {
            // 1) Aynı plakaya ait hâlihazırda bir rezervasyon var mı?
            var existingReservations = await _parkingSpacesRepository
                .FindAsync(ps => ps.PlateNumber == plateNumber && ps.IsReserved);

            if (existingReservations.Any())
            {
                return BadRequest("You already have an active reservation.");
            }

            // 2) Rezervelenmek istenen yeri al
            var parkingSpace = (await _parkingSpacesRepository
                    .FindAsync(ps => ps.LotId == lotId && ps.SpaceNumber == spaceNumber))
                .FirstOrDefault();

            if (parkingSpace == null)
                return NotFound($"Lot {lotId}, space {spaceNumber} not found.");

            if (parkingSpace.IsOccupied)
                return BadRequest($"Space {spaceNumber} is occupied.");

            if (parkingSpace.IsReserved)
                return BadRequest($"Space {spaceNumber} is already reserved.");

            // 3) Rezervasyonu yap
            parkingSpace.IsReserved = true;
            parkingSpace.PlateNumber = plateNumber;
            await _parkingSpacesRepository.UpdateAsync(parkingSpace);

            return Ok($"Space {spaceNumber} successfully reserved for {plateNumber}.");
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

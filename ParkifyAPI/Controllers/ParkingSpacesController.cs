using Microsoft.AspNetCore.Mvc;
using ParkifyAPI.DataAccess.Interfaces;
using ParkifyAPI.Common.Model;
//API endpointlerinin gösterimi
namespace ParkifyAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ParkingSpacesController : ControllerBase
    {
        private readonly IGenericRepository<ParkingSpace> _parkingSpacesRepository;

        public ParkingSpacesController(IGenericRepository<ParkingSpace> parkingSpacesRepository)
        {
            _parkingSpacesRepository = parkingSpacesRepository;
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
            var parkingSpaces = await _parkingSpacesRepository.FindAsync(ps => ps.LotId == lotId && ps.SpaceNumber == spaceNumber);
            var parkingSpace = parkingSpaces.FirstOrDefault();

            if (parkingSpace == null)
            {
                return NotFound($"Lot ID {lotId} içinde {spaceNumber} park alanı bulunamadı.");
            }

            if (parkingSpace.IsOccupied)
            {
                return BadRequest($"Park alanı {spaceNumber} şu anda dolu, rezerve edilemez.");
            }

            if (parkingSpace.IsReserved)
            {
                return BadRequest($"Park alanı {spaceNumber} daha önceden rezerve edilmiş. Rezerve eden: {parkingSpace.PlateNumber}");
            }

            parkingSpace.IsReserved = true;
            parkingSpace.PlateNumber = plateNumber;
            await _parkingSpacesRepository.UpdateAsync(parkingSpace);

            return Ok($"Park alanı {spaceNumber} başarıyla rezerve edildi. Araç plakası: {plateNumber}");
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

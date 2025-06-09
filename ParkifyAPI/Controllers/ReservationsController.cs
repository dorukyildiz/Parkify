using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ParkifyAPI.Common.Model;
using ParkifyAPI.Data.Contexts;
using System.Linq;

namespace ParkifyAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReservationsController : ControllerBase
    {
        private readonly ParkifyDbContext _context;

        public ReservationsController(ParkifyDbContext context)
        {
            _context = context;
        }

        [HttpGet("GetUserReservations")]
        public async Task<IActionResult> GetUserReservations(string licensePlate)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.LicensePlate == licensePlate);
            if (user == null)
                return NotFound("User not found.");

            var now = DateTime.UtcNow;

            // Süresi dolmuş aktif rezervasyonları pasif hale getir
            var expired = _context.Reservations
                .Where(r => r.UserId == user.Id && r.IsActive && r.EndTime < now);
            foreach (var r in expired)
                r.IsActive = false;

            await _context.SaveChangesAsync();

            // Kullanıcının tüm rezervasyonlarını getir
            var reservations = await _context.Reservations
                .Where(r => r.UserId == user.Id)
                .ToListAsync();

            var result = reservations.Select(r => new
            {
                r.LotId,
                r.SpaceNumber,
                r.StartTime,
                r.EndTime,
                Status = r.IsActive ? "Active" : "Expired"
            });

            return Ok(result);
        }
        
        [HttpGet("GetUserReservationsWithLotName")]
        public async Task<IActionResult> GetUserReservationsWithLotName(string licensePlate)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.LicensePlate == licensePlate);
            if (user == null)
                return NotFound("User not found.");

            var now = DateTime.UtcNow;

            // Süresi dolmuş aktif rezervasyonları pasif hale getir
            var expiredReservations = await _context.Reservations
                .Where(r => r.UserId == user.Id && r.IsActive && r.EndTime < now)
                .ToListAsync();

            foreach (var r in expiredReservations)
                r.IsActive = false;

            await _context.SaveChangesAsync();

            // Kullanıcının tüm rezervasyonlarını lot bilgisi ile birlikte getir
            var reservations = await _context.Reservations
                .Where(r => r.UserId == user.Id)
                .Join(_context.ParkingLots,
                    reservation => reservation.LotId,
                    lot => lot.LotId,
                    (reservation, lot) => new
                    {
                        reservation.LotId,
                        LotName = lot.Name,
                        reservation.SpaceNumber,
                        reservation.StartTime,
                        reservation.EndTime,
                        Status = reservation.IsActive ? "Active" : "Expired"
                    })
                .OrderByDescending(r => r.StartTime)
                .ToListAsync();

            return Ok(reservations);
        }
        
        [HttpGet("HasActiveReservation")]
        public async Task<IActionResult> HasActiveReservation(string licensePlate)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.LicensePlate == licensePlate);
            if (user == null)
                return NotFound("User not found.");

            var now = DateTime.UtcNow;
            var hasActive = await _context.Reservations
                .AnyAsync(r => r.UserId == user.Id && r.IsActive && r.EndTime > now);

            return Ok(hasActive);
        }
        
        [HttpDelete("CancelReservation")]
        public async Task<IActionResult> CancelReservation(string licensePlate, int lotId, string spaceNumber)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.LicensePlate == licensePlate);
            if (user == null)
                return NotFound("User not found.");

            var reservation = await _context.Reservations.FirstOrDefaultAsync(r =>
                r.UserId == user.Id && r.LotId == lotId && r.SpaceNumber == spaceNumber && r.IsActive);

            if (reservation == null)
                return NotFound("Active reservation not found for this user and space.");

            reservation.IsActive = false;

            var parkingSpace = await _context.ParkingSpaces
                .FirstOrDefaultAsync(ps => ps.LotId == lotId && ps.SpaceNumber == spaceNumber);

            if (parkingSpace != null)
            {
                parkingSpace.IsReserved = false;
                parkingSpace.PlateNumber = null;
            }

            await _context.SaveChangesAsync();

            return Ok("Reservation cancelled successfully.");
        }

    }
}

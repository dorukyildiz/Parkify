using Microsoft.AspNetCore.Mvc;
using ParkifyAPI.Common.Model;
using ParkifyAPI.Data.Contexts;
using System.Linq;

namespace ParkifyAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AdminController : ControllerBase
    {
        private readonly ParkifyDbContext _context;

        public AdminController(ParkifyDbContext context)
        {
            _context = context;
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] Administrator loginInput)
        {
            if (string.IsNullOrWhiteSpace(loginInput.Email) ||
                string.IsNullOrWhiteSpace(loginInput.Password))
                return BadRequest();

            var admin = _context.Administrators
                .FirstOrDefault(a =>
                    a.Email == loginInput.Email &&
                    a.Password == loginInput.Password);

            if (admin == null)
                return Unauthorized();

            return Ok(new
            {
                name = admin.Name,
                lot_id = admin.LotId
            });
        }
    }

}
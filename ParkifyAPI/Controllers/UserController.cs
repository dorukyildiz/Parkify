using Microsoft.AspNetCore.Mvc;
using ParkifyAPI.Common.Model;
using ParkifyAPI.Data.Contexts;

namespace ParkifyAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly ParkifyDbContext _context;

        public UserController(ParkifyDbContext context)
        {
            _context = context;
        }

        [HttpPost("register")]
        public IActionResult Register([FromBody] User userInput)
        {
            var existingUser = _context.Users.FirstOrDefault(u =>
                u.Email == userInput.Email || u.LicensePlate == userInput.LicensePlate);

            if (existingUser != null)
                return BadRequest("This email or license plate is already registered.");

            var newUser = new User
            {
                Email = userInput.Email,
                Password = userInput.Password,
                LicensePlate = userInput.LicensePlate
            };

            _context.Users.Add(newUser);
            _context.SaveChanges();

            return Ok();
        }
        
        
        [HttpPost("login")]
        public IActionResult Login([FromBody] User loginInput)
        {
            var user = _context.Users.FirstOrDefault(u => u.Email == loginInput.Email);

            if (user == null || user.Password != loginInput.Password)
                return Unauthorized();

            return Ok(); 
        }



        
        
    }
}
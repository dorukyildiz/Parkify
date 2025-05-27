using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using ParkifyAPI.Common.Model;
using ParkifyAPI.Data.Contexts;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using BCrypt.Net;

namespace ParkifyAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly ParkifyDbContext _context;
        private readonly IConfiguration _configuration;

        public UserController(ParkifyDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        [HttpPost("register")]
        public IActionResult Register([FromBody] User userInput)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var existingUser = _context.Users.FirstOrDefault(u =>
                u.Email == userInput.Email || u.LicensePlate == userInput.LicensePlate);

            if (existingUser != null)
                return BadRequest("This email or license plate is already registered.");

            var newUser = new User
            {
                Email = userInput.Email,
                Password = BCrypt.Net.BCrypt.HashPassword(userInput.Password),
                LicensePlate = userInput.LicensePlate
            };

            _context.Users.Add(newUser);
            _context.SaveChanges();

            return Ok("Registration successful.");
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] User loginInput)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = _context.Users.FirstOrDefault(u => u.Email == loginInput.Email);
            if (user == null || !BCrypt.Net.BCrypt.Verify(loginInput.Password, user.Password))
                return Unauthorized("Invalid credentials.");

            var token = GenerateJwtToken(user);
            return Ok(new
            {
                token,
                email = user.Email,
                licensePlate = user.LicensePlate
            });
        }

        private string GenerateJwtToken(User user)
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.Email, user.Email),
                new Claim("license_plate", user.LicensePlate)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddHours(2),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}

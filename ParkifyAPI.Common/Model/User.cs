using System.ComponentModel.DataAnnotations.Schema;

namespace ParkifyAPI.Common.Model
{
    public class User
    {
        public int Id { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }

        [Column("license_plate")]
        public string LicensePlate { get; set; }
    }
}
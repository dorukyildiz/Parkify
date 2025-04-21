using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ParkifyAPI.Common.Model
{
    [Table("Administrators")]
    public class Administrator
    {
        [Key]
        [Column("admin_id")]
        public int AdminId { get; set; }

        [Column("name")]
        public string Name { get; set; }

        [Column("email")]
        public string Email { get; set; }

        [Column("password")]
        public string Password { get; set; }

        [Column("lot_id")]
        public int? LotId { get; set; }
    }
}
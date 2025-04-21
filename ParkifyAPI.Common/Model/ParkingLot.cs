using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ParkifyAPI.Common.Model
{
    [Table("Parking_Lots")]
    public class ParkingLot
    {
        [Key]
        [Column("lot_id")]
        public int LotId { get; set; }

        [Column("name")]
        public string Name { get; set; }
    }
}
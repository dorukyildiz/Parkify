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

        [Column("location")]
        public string Location { get; set; }

        [Column("total_spots")]
        public int TotalSpots { get; set; }

        [Column("num_of_floors")]
        public int NumOfFloors { get; set; }

        [Column("layout", TypeName = "json")]
        public string Layout { get; set; }
    }
}
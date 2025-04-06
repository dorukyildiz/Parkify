using System.ComponentModel.DataAnnotations.Schema;
//Entity sınıfı

namespace ParkifyAPI.Common.Model
{
    [Table("Parking_Spaces")]
    public class ParkingSpace
    {
        [Column("lot_id")]
        public int LotId { get; set; }
        
        [Column("space_number")]
        public string SpaceNumber { get; set; }
        
        [Column("is_occupied")]
        public bool IsOccupied { get; set; }
        
        [Column("is_reserved")]
        public bool IsReserved { get; set; }
        
        [Column("plate_number")]
        public string? PlateNumber { get; set; }
    }
}
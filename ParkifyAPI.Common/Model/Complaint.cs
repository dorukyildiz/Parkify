using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ParkifyAPI.Common.Model
{
    [Table("Complaints")]
    public class Complaint
    {
        [Key]
        public int Id { get; set; }

        [Column("user_id")]
        public int UserId { get; set; }

        [Column("lot_id")]
        public int LotId { get; set; }

        [Column("space_number")]
        public string SpaceNumber { get; set; }

        [Column("license_plate_detected")]
        public string LicensePlateDetected { get; set; }

        [Column("image_path")]
        public string ImagePath { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; }

        [Column("is_resolved")]
        public bool IsResolved { get; set; } = false;

        [ForeignKey("UserId")]
        public User User { get; set; }

        [ForeignKey("LotId")]
        public ParkingLot ParkingLot { get; set; }
    }
}
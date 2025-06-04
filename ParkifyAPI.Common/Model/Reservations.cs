using System.ComponentModel.DataAnnotations.Schema;

namespace ParkifyAPI.Common.Model
{
    using System.ComponentModel.DataAnnotations;

    [Table("Reservations")]
    public class Reservation
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("user_id")]
        public int UserId { get; set; }

        [Column("lot_id")]
        public int LotId { get; set; }

        [Column("space_number")]
        public string SpaceNumber { get; set; }

        [Column("start_time")]
        public DateTime StartTime { get; set; }

        [Column("end_time")]
        public DateTime EndTime { get; set; }

        [Column("is_active")]
        public bool IsActive { get; set; }

        [ForeignKey("UserId")]
        public User User { get; set; }
    }

}
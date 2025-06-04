using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ParkifyAPI.Common.Model
{
    [Table("Penalties")]
    public class Penalty
    {
        [Key]
        public int Id { get; set; }

        [Column("complaint_id")]
        public int ComplaintId { get; set; }

        [Column("admin_id")]
        public int AdminId { get; set; }

        [Column("plate_number")]
        public string PlateNumber { get; set; }

        [Column("reason")]
        public string Reason { get; set; }

        [Column("penalty_points")]
        public int PenaltyPoints { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; }

        [ForeignKey("ComplaintId")]
        public Complaint Complaint { get; set; }

        [ForeignKey("AdminId")]
        public Administrator Admin { get; set; }
    }
}
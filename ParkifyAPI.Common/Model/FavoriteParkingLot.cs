namespace ParkifyAPI.Common.Model
{
    public class FavoriteParkingLot
    {
        public int Id { get; set; }

        public int UserId { get; set; }
        public User User { get; set; }

        public int LotId { get; set; }
        public ParkingLot Lot { get; set; }
    }


}
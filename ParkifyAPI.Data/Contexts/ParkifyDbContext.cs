using Microsoft.EntityFrameworkCore;
using ParkifyAPI.Common.Model;
//EFCore ile birlikte, veritabanı ile iletişim
namespace ParkifyAPI.Data.Contexts
{
    public class ParkifyDbContext : DbContext
    {
        public ParkifyDbContext(DbContextOptions<ParkifyDbContext> options) : base(options) { }

        public DbSet<ParkingSpace> ParkingSpaces { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<ParkingSpace>()
                .ToTable("Parking_Spaces")
                .HasKey(ps => new { ps.SpaceNumber, ps.LotId });
        }
    }
}
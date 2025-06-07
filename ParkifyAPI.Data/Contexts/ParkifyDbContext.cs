using Microsoft.EntityFrameworkCore;
using ParkifyAPI.Common.Model;
//EFCore ile birlikte, veritabanı ile iletişim
namespace ParkifyAPI.Data.Contexts
{
    public class ParkifyDbContext : DbContext
    {
        public ParkifyDbContext(DbContextOptions<ParkifyDbContext> options) : base(options) { }

        public DbSet<ParkingSpace> ParkingSpaces { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Administrator> Administrators { get; set; }
        
        public DbSet<ParkingLot> ParkingLots { get; set; }
        public DbSet<Reservation> Reservations { get; set; }
        
        public DbSet<Complaint> Complaints { get; set; }
        
        public DbSet<Penalty> Penalties { get; set; }
        
        public DbSet<FavoriteParkingLot> FavoriteParkingLots { get; set; }


        

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<ParkingSpace>()
                .ToTable("Parking_Spaces")
                .HasKey(ps => new { ps.SpaceNumber, ps.LotId });

            modelBuilder.Entity<User>()
                .ToTable("Users");
            
            modelBuilder.Entity<Administrator>()
                .ToTable("Administrators");
            
            modelBuilder.Entity<ParkingLot>().ToTable("Parking_Lots");
            
            modelBuilder.Entity<Reservation>().ToTable("Reservations");
            
            modelBuilder.Entity<Complaint>().ToTable("Complaints");
            
            modelBuilder.Entity<Penalty>().ToTable("Penalties");
            
            modelBuilder.Entity<FavoriteParkingLot>()
                .ToTable("FavoriteParkingLots")
                .HasKey(f => f.Id);

            modelBuilder.Entity<FavoriteParkingLot>()
                .Property(f => f.Id)
                .HasColumnName("id");

            modelBuilder.Entity<FavoriteParkingLot>()
                .Property(f => f.UserId)
                .HasColumnName("user_id");

            modelBuilder.Entity<FavoriteParkingLot>()
                .Property(f => f.LotId)
                .HasColumnName("lot_id");

            modelBuilder.Entity<FavoriteParkingLot>()
                .HasOne(f => f.User)
                .WithMany()
                .HasForeignKey(f => f.UserId);

            modelBuilder.Entity<FavoriteParkingLot>()
                .HasOne(f => f.Lot)
                .WithMany()
                .HasForeignKey(f => f.LotId);

        }
    }


}
using Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace Backend
{
    public class FlightStatsDbContext : DbContext
    {
        public FlightStatsDbContext(DbContextOptions<FlightStatsDbContext> options) : base(options) { }

        public DbSet<Config> Configs { get; set; }
        public DbSet<Airport> Airports { get; set; }
        public DbSet<Flight> Flights { get; set; }
        public DbSet<FlightData> FlightData { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            IEnumerable<Airport> airports = AirportDataLoader.LoadAirports("airports.dat");

            modelBuilder.Entity<Airport>().HasData(airports);

            modelBuilder.Entity<Flight>()
                .HasOne(f => f.Origin)
                .WithMany()
                .HasForeignKey(f => f.OriginId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Flight>()
                .HasOne(f => f.Destination)
                .WithMany()
                .HasForeignKey(f => f.DestinationId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}

namespace Backend.Models
{
    public class Airport
    {
        public int AirportId { get; set; }
        public required string Name { get; set; }
        public required string City { get; set; }
        public required string Country { get; set; }
        public required string IATA { get; set; }
        public required string ICAO { get; set; }
        public float Latitude { get; set; } = 0;
        public float Longitude { get; set; } = 0;
        public int Altitude { get; set; } = 0;
        public required string Timezone {  get; set; }
    }
}

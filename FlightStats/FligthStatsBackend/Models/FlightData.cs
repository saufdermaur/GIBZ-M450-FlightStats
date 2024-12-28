namespace Backend.Models
{
    public class FlightData
    {
        public int FlightDataId { get; set; }
        public int FlightId { get; set; }
        public required string FlightNumber { get; set; }
        public DateTime? DateTime { get; set; }
        public int? Price { get; set; }

        public required Flight Flight { get; set; }
    }
}

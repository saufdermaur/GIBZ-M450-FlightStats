namespace Backend.Models
{
    public class FlightData
    {
        public int FlightDataId { get; set; }
        public int FlightId { get; set; }
        public DateTime? FetchedTime { get; set; }
        public int? Price { get; set; }
        public required Flight Flight { get; set; }
    }
}

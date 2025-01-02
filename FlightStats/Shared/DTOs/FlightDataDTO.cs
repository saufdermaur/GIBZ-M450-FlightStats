namespace Shared.DTOs
{
    public class FlightDataDTO
    {
        public int FlightDataId { get; set; }
        public int FlightId { get; set; }
        public DateTime? FetchedTime { get; set; }
        public int? Price { get; set; }

        public required FlightDTO Flight { get; set; }
    }
}

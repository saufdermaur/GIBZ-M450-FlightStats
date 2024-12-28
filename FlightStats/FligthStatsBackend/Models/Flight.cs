namespace Backend.Models
{
    public class Flight
    {
        public int FlightId { get; set; }
        public int OriginId { get; set; }
        public int DestinationId { get; set; }
        public string? FlightNumber { get; set; }
        public bool? IsBeingTracked { get; set; } = true;

        public required Airport Origin { get; set; }
        public required Airport Destination { get; set; }
    }
}

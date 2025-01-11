namespace Shared.DTOs
{
    public class FlightDTO
    {
        public int FlightId { get; set; }
        public string? FlightNumber { get; set; }
        public DateTime FlightDepartureTime { get; set; }
        public DateTime FlightArrivalTime { get; set; }
        public required AirportDTO Origin { get; set; }
        public required AirportDTO Destination { get; set; }
        public int Price { get; set; }
    }

    public enum Frequency
    {
        Minute,
        Hour,
        Day,
        Week,
        Month
    }
}

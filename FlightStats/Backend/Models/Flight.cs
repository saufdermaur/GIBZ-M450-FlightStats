﻿namespace Backend.Models
{
    public class Flight
    {
        public int FlightId { get; set; }
        public int OriginId { get; set; }
        public int DestinationId { get; set; }
        public required string FlightNumber { get; set; }
        public required DateTime FlightDepartureTime { get; set; }
        public required DateTime FlightArrivalTime { get; set; }
        public required Airport Origin { get; set; }
        public required Airport Destination { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.DTOs
{
    public class FlightDTO
    {
        public string? FlightNumber { get; set; }
        public DateTime? FlightDepartureTime { get; set; }
        public DateTime? FlightArrivalTime { get; set; }
        public required AirportDTO Origin { get; set; }
        public required AirportDTO Destination { get; set; }
    }
}

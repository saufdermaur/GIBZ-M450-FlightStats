using Backend.Models;
using Shared.DTOs;

namespace Backend.Selenium
{
    public interface ISeleniumFlights
    {
        List<FlightDTO> GetAllFlights(Airport originAirport, Airport destinationAirport, DateTime flightDate);
        void SearchForFlights(Airport originAirport, Airport destinationAirport, DateTime flightDate);
        void TrackNewFlight(Airport originAirport, Airport destinationAirport, DateTime flightDate, string flightNumber);
    }
}
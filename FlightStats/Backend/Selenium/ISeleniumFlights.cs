using Backend.Models;
using Shared.DTOs;

namespace Backend.Selenium
{
    public interface ISeleniumFlights
    {
        List<FlightDTO> GetAllFlights(Airport originAirport, Airport destinationAirport, DateTime flightDate);
        void SearchForFlights(Airport originAirport, Airport destinationAirport, DateTime flightDate);
        FlightDTO GetSpecificFlight(Airport originAirport, Airport destinationAirport, DateTime flightDate, string flightNumber);
        List<DayPrice> GetSpeGetCheapestMostExpensiveDateWithFlexibilitycificFlight(Airport originAirport, Airport destinationAirport, DateTime flightDate, string flightNumber, int flexibility);
    }
}
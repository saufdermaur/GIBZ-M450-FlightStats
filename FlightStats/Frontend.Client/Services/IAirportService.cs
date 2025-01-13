using Shared.DTOs;

namespace Frontend.Client.Services
{
    public interface IAirportService
    {
        Task<IEnumerable<AirportDTO>> SearchAirport(string query);
    }
}

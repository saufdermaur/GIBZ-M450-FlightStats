using Shared.DTOs;
using System.Net.Http.Json;

namespace Frontend.Client.Services
{
    public class AirportsService : IAirportService
    {
        private readonly HttpClient _httpClient = new();
        private readonly string _baseAddress = "https://localhost:7019/api/Airports";
        public async Task<IEnumerable<AirportDTO>> SearchAirport(string query)
        {
            if (string.IsNullOrEmpty(query))
                return [];
            try
            {
                List<AirportDTO>? response = await _httpClient.GetFromJsonAsync<List<AirportDTO>>($"{_baseAddress}/search?query={query}");
                if (response is not null)
                    return response;
                else
                    return Array.Empty<AirportDTO>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching airports: {ex.Message}");
                return [];
            }
        }
    }
}

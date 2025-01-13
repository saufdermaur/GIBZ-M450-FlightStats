using Backend.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shared.DTOs;

namespace Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AirportsController(FlightStatsDbContext context) : ControllerBase
    {
        private readonly FlightStatsDbContext _context = context;

        [HttpGet("search")]
        public async Task<IActionResult> SearchAirports(string query)
        {
            if (string.IsNullOrEmpty(query))
                return BadRequest("Query parameter is required.");

            try
            {
                List<Airport> airports = await _context.Airports
                    .Where(_ => _.Name.Contains(query) || _.IATA.Contains(query) || _.City.Contains(query))
                    .Take(10)
                    .ToListAsync();

                return Ok(airports.Select(_ => AirportToDTO(_)).ToList());
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while processing your request.");
            }
        }
        private static AirportDTO AirportToDTO(Airport airport)
        {
            return new AirportDTO
            {
                AirportId = airport.AirportId,
                Code = airport.IATA,
                Name = airport.Name
            };
        }
    }
}

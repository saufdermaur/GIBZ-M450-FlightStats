using Backend.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AirportsController : ControllerBase
    {
        private readonly FlightStatsDbContext _context;

        public AirportsController(FlightStatsDbContext context)
        {
            _context = context;
        }

        [HttpGet("search")]
        public async Task<IActionResult> SearchAirports(string query)
        {
            if (string.IsNullOrEmpty(query))
            {
                return BadRequest("Query parameter is required.");
            }

            List<Airport> airports = await _context.Airports
                .Where(a => a.Name.Contains(query) || a.IATA.Contains(query) || a.City.Contains(query))
                .Take(10)
                .ToListAsync();

            return Ok(airports);
        }
    }
}

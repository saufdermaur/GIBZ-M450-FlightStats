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

        // GET: api/Airports
        [HttpGet]
        public async Task<IActionResult> GetAirports()
        {
            var airports = await _context.Airports.ToListAsync();
            return Ok(airports);
        }

        // GET: api/Airports/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetAirport(int id)
        {
            var airport = await _context.Airports
                .FirstOrDefaultAsync(a => a.AirportId == id);

            if (airport == null)
            {
                return NotFound();
            }

            return Ok(airport);
        }

        // POST: api/Airports
        [HttpPost]
        public async Task<IActionResult> CreateAirport([FromBody] Airport airport)
        {
            if (ModelState.IsValid)
            {
                _context.Airports.Add(airport);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetAirport), new { id = airport.AirportId }, airport);
            }

            return BadRequest(ModelState);
        }

        // PUT: api/Airports/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAirport(int id, [FromBody] Airport airport)
        {
            if (id != airport.AirportId)
            {
                return BadRequest();
            }

            _context.Entry(airport).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!AirportExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // DELETE: api/Airports/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAirport(int id)
        {
            var airport = await _context.Airports.FindAsync(id);
            if (airport == null)
            {
                return NotFound();
            }

            _context.Airports.Remove(airport);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool AirportExists(int id)
        {
            return _context.Airports.Any(e => e.AirportId == id);
        }
    }
}

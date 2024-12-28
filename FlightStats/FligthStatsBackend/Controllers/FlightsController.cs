using Backend.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FlightsController : ControllerBase
    {
        private readonly FlightStatsDbContext _context;

        public FlightsController(FlightStatsDbContext context)
        {
            _context = context;
        }

        // GET: api/Flights
        [HttpGet]
        public async Task<IActionResult> GetFlights()
        {
            var flights = await _context.Flights.Include(f => f.Destination).Include(f => f.Origin).ToListAsync();
            return Ok(flights);
        }

        // GET: api/Flights/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetFlight(int id)
        {
            var flight = await _context.Flights
                .Include(f => f.Destination)
                .Include(f => f.Origin)
                .FirstOrDefaultAsync(m => m.FlightId == id);

            if (flight == null)
            {
                return NotFound();
            }

            return Ok(flight);
        }

        // POST: api/Flights
        [HttpPost]
        public async Task<IActionResult> CreateFlight([FromBody] Flight flight)
        {
            if (ModelState.IsValid)
            {
                _context.Flights.Add(flight);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetFlight), new { id = flight.FlightId }, flight);
            }

            return BadRequest(ModelState);
        }

        // PUT: api/Flights/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateFlight(int id, [FromBody] Flight flight)
        {
            if (id != flight.FlightId)
            {
                return BadRequest();
            }

            _context.Entry(flight).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!FlightExists(id))
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

        // DELETE: api/Flights/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteFlight(int id)
        {
            var flight = await _context.Flights.FindAsync(id);
            if (flight == null)
            {
                return NotFound();
            }

            _context.Flights.Remove(flight);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool FlightExists(int id)
        {
            return _context.Flights.Any(e => e.FlightId == id);
        }
    }
}

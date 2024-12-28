using Backend.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FlightDatasController : ControllerBase
    {
        private readonly FlightStatsDbContext _context;

        public FlightDatasController(FlightStatsDbContext context)
        {
            _context = context;
        }

        // GET: api/FlightDatas
        [HttpGet]
        public async Task<IActionResult> GetFlightDatas()
        {
            var flightData = await _context.FlightData.Include(f => f.Flight).ToListAsync();
            return Ok(flightData);
        }

        // GET: api/FlightDatas/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetFlightData(int id)
        {
            var flightData = await _context.FlightData
                .Include(f => f.Flight)
                .FirstOrDefaultAsync(m => m.FlightDataId == id);

            if (flightData == null)
            {
                return NotFound();
            }

            return Ok(flightData);
        }

        // POST: api/FlightDatas
        [HttpPost]
        public async Task<IActionResult> CreateFlightData([FromBody] FlightData flightData)
        {
            if (ModelState.IsValid)
            {
                _context.FlightData.Add(flightData);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetFlightData), new { id = flightData.FlightDataId }, flightData);
            }

            return BadRequest(ModelState);
        }

        // PUT: api/FlightDatas/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateFlightData(int id, [FromBody] FlightData flightData)
        {
            if (id != flightData.FlightDataId)
            {
                return BadRequest();
            }

            _context.Entry(flightData).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!FlightDataExists(id))
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

        // DELETE: api/FlightDatas/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteFlightData(int id)
        {
            var flightData = await _context.FlightData.FindAsync(id);
            if (flightData == null)
            {
                return NotFound();
            }

            _context.FlightData.Remove(flightData);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool FlightDataExists(int id)
        {
            return _context.FlightData.Any(e => e.FlightDataId == id);
        }
    }
}

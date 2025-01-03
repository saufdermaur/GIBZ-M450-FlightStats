using Backend.Models;
using Backend.Selenium;
using Hangfire;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shared.DTOs;

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
            List<Flight> flights = await _context.Flights.Include(f => f.Destination).Include(f => f.Origin).ToListAsync();
            return Ok(flights);
        }

        // GET: api/Flights/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetFlight(int id)
        {
            Flight? flight = await _context.Flights
                .Include(f => f.Destination)
                .Include(f => f.Origin)
                .FirstOrDefaultAsync(m => m.FlightId == id);

            if (flight == null)
            {
                return NotFound();
            }

            return Ok(flight);
        }

        //// POST: api/Flights
        //[HttpPost]
        //public async Task<IActionResult> CreateFlight([FromBody] Flight flight)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        _context.Flights.Add(flight);
        //        await _context.SaveChangesAsync();

        //        return CreatedAtAction(nameof(GetFlight), new { id = flight.FlightId }, flight);
        //    }

        //    return BadRequest(ModelState);
        //}

        [HttpGet("all")] // date needs to be like: 2025-01-02T15:30:00
        public async Task<IActionResult> GetAllFlights([FromQuery] int originId, [FromQuery] int destinationId, [FromQuery] DateTime flightDate)
        {
            Airport? airportOrigin = await _context.Airports.FirstOrDefaultAsync(a => a.AirportId == originId);
            Airport? airportDestination = await _context.Airports.FirstOrDefaultAsync(a => a.AirportId == destinationId);

            if (airportOrigin == null || airportDestination == null)
            {
                return NotFound();
            }

            SeleniumFlights seleniumFlights = new SeleniumFlights(_context);
            List<FlightDTO> availableFlights = seleniumFlights.GetAllFlights(airportOrigin, airportDestination, flightDate);

            if (availableFlights == null)
            {
                return NotFound();
            }

            return Ok(availableFlights);
        }

        [HttpGet("specificFlight")]
        public async Task<IActionResult> GetSpecificFlight([FromQuery] int originId, [FromQuery] int destinationId, [FromQuery] DateTime flightDate, [FromQuery] string flightNumber)
        {
            Airport? airportOrigin = await _context.Airports.FirstOrDefaultAsync(a => a.AirportId == originId);
            Airport? airportDestination = await _context.Airports.FirstOrDefaultAsync(a => a.AirportId == destinationId);

            if (airportOrigin == null || airportDestination == null)
            {
                return NotFound();
            }

            SeleniumFlights seleniumFlights = new SeleniumFlights(_context);
            FlightDTO availableFlight = seleniumFlights.GetSpecificFlight(airportOrigin, airportDestination, flightDate, flightNumber);

            if (availableFlight == null)
            {
                return NotFound();
            }

            return Ok(availableFlight);
        }

        // to update a flight, send an already existing "JobForFlight_{flightNumber}" => updated with params
        [HttpPost]
        public async Task<IActionResult> NewOrUpdateTrackableFlight([FromQuery] int originId, [FromQuery] int destinationId, [FromQuery] DateTime flightDate, [FromQuery] string flightNumber, [FromQuery] Frequency frequency)
        {
            Airport? airportOrigin = await _context.Airports.FirstOrDefaultAsync(a => a.AirportId == originId);
            Airport? airportDestination = await _context.Airports.FirstOrDefaultAsync(a => a.AirportId == destinationId);

            if (airportOrigin == null || airportDestination == null)
            {
                return NotFound();
            }

            Func<string> cronJob;

            if (frequency == Frequency.Minute)
            {
                cronJob = Cron.Minutely;
            }
            else if (frequency == Frequency.Hour)
            {
                cronJob = Cron.Hourly;
            }
            else if (frequency == Frequency.Day)
            {
                cronJob = Cron.Daily;
            }
            else if (frequency == Frequency.Week)
            {
                cronJob = Cron.Weekly;
            }
            else
            {
                cronJob = Cron.Monthly;
            }

            SeleniumFlights seleniumFlights = new SeleniumFlights(_context);
            RecurringJob.AddOrUpdate($"JobForFlight_{flightNumber}", () => seleniumFlights.TrackNewFlight(airportOrigin, airportDestination, flightDate, flightNumber), cronJob);
            return Ok();
        }

        [HttpDelete]
        public IActionResult DeleteTrackableFlight([FromQuery] string flightNumber)
        {
            RecurringJob.RemoveIfExists($"JobForFlight_{flightNumber}");
            return Ok();
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
            Flight? flight = await _context.Flights.FindAsync(id);
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

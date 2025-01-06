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
        private readonly ISeleniumFlights _seleniumFlights;

        public FlightsController(FlightStatsDbContext context, ISeleniumFlights selenium)
        {
            _context = context;
            _seleniumFlights = selenium;
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

        [HttpGet("GetAllFlights")] // date needs to be like: 2025-01-02T15:30:00
        public async Task<IActionResult> GetAllFlights([FromQuery] int originId, [FromQuery] int destinationId, [FromQuery] DateTime flightDate)
        {
            Airport? airportOrigin = await _context.Airports.FirstOrDefaultAsync(a => a.AirportId == originId);
            Airport? airportDestination = await _context.Airports.FirstOrDefaultAsync(a => a.AirportId == destinationId);

            if (airportOrigin == null || airportDestination == null)
            {
                return NotFound();
            }

            List<FlightDTO> availableFlights = _seleniumFlights.GetAllFlights(airportOrigin, airportDestination, flightDate);

            if (availableFlights == null)
            {
                return NotFound();
            }

            return Ok(availableFlights);
        }

        // to update a flight, send an already existing "JobForFlight_{flightNumber}" => updated with params
        [HttpPost("NewOrUpdateJobFlight")]
        public async Task<IActionResult> NewOrUpdateJobFlight([FromQuery] int originId, [FromQuery] int destinationId, [FromQuery] DateTime flightDate, [FromQuery] string flightNumber, [FromQuery] Frequency frequency)
        {
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

            RecurringJob.AddOrUpdate($"JobForFlight_{flightNumber}", () => TrackNewFlightAndSaveJob(originId, destinationId, flightDate, flightNumber), cronJob);

            return Ok();
        }

        [NonAction]
        public async Task TrackNewFlightAndSaveJob(int originId, int destinationId, DateTime flightDate, string flightNumber)
        {
            try
            {
                Airport? airportOrigin = await _context.Airports.FirstOrDefaultAsync(a => a.AirportId == originId);
                Airport? airportDestination = await _context.Airports.FirstOrDefaultAsync(a => a.AirportId == destinationId);

                if (airportOrigin == null || airportDestination == null)
                {
                    return;
                }

                FlightDTO? findFlight = _seleniumFlights.GetSpecificFlight(airportOrigin, airportDestination, flightDate, flightNumber);

                if (findFlight != null)
                {
                    FlightData flightData = new FlightData()
                    {
                        Flight = null,
                        FetchedTime = DateTime.Now,
                        Price = findFlight.Price,
                    };

                    Flight? dbFlight = _context.Flights.FirstOrDefault(f => f.FlightNumber.Equals(flightNumber));

                    if (dbFlight != null)
                    {
                        flightData.Flight = dbFlight;
                    }
                    else
                    {
                        if (airportOrigin == null || airportDestination == null)
                        {
                            throw new InvalidOperationException("Origin or destination airport not found in the database.");
                        }

                        Flight flight = new Flight()
                        {
                            Destination = airportDestination,
                            Origin = airportOrigin,
                            FlightNumber = flightNumber,
                            FlightDepartureTime = findFlight.FlightDepartureTime,
                            FlightArrivalTime = findFlight.FlightArrivalTime,
                        };
                        flightData.Flight = flight;
                    }

                    _context.Add(flightData);
                    _context.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        [HttpDelete("DeleteJobFlight")]
        public IActionResult DeleteJobFlight([FromQuery] string flightNumber)
        {
            RecurringJob.RemoveIfExists($"JobForFlight_{flightNumber}");
            return Ok();
        }

        // DELETE: api/Flights/5
        [HttpDelete("DeleteJobFlightAndAllInfo,{id}")]
        public async Task<IActionResult> DeleteFlight(int id)
        {
            Flight? flight = await _context.Flights.FindAsync(id);
            if (flight == null)
            {
                return NotFound();
            }

            _context.Flights.Remove(flight);
            await _context.SaveChangesAsync();
            RecurringJob.RemoveIfExists($"JobForFlight_{flight.FlightNumber}");

            return NoContent();
        }
    }
}

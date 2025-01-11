using Backend.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shared.DTOs;

namespace Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FlightsController(FlightStatsDbContext context) : ControllerBase
    {
        private readonly FlightStatsDbContext _context = context;

        // GET: api/Flights
        [HttpGet]
        public async Task<IActionResult> GetFlights()
        {
            try
            {
                List<Flight> flights = await _context.Flights
                    .Include(f => f.Destination)
                    .Include(f => f.Origin)
                    .ToListAsync();
                return Ok(flights.Select(f => FlightToDTO(f)));
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while processing your request.");
            }
        }

        // GET: api/Flights/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetFlight(int id)
        {
            if (id <= 0)
            {
                return BadRequest("Flight Id must be a positive integer.");
            }

            try
            {
                Flight? flight = await _context.Flights
                    .Include(f => f.Destination)
                    .Include(f => f.Origin)
                    .FirstOrDefaultAsync(m => m.FlightId == id);

                if (flight == null)
                {
                    return NotFound($"Flight with Id {id} not found.");
                }

                return Ok(flight);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while processing your request.");
            }
        }




        // TODO: add stats


        // GET: api/Flights/GetCheapestMostExpensiveWeekday
        [HttpGet("GetCheapestMostExpensiveWeekday,{id}")]
        public async Task<IActionResult> GetCheapestMostExpensiveWeekday(int id)
        {
            if (id <= 0)
            {
                return BadRequest("Flight Id must be a positive integer.");
            }

            try
            {
                Flight? flight = await _context.Flights
                    .Include(f => f.Destination)
                    .Include(f => f.Origin)
                    .FirstOrDefaultAsync(m => m.FlightId == id);

                if (flight == null)
                {
                    return NotFound($"Flight with Id {id} not found.");
                }

                return Ok(flight);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while processing your request.");
            }
        }

        // GET: api/Flights/GetCheapestMostExpensiveDate
        [HttpGet("GetCheapestMostExpensiveDate,{id}")]
        public async Task<IActionResult> GetCheapestMostExpensiveDate(int id)
        {
            if (id <= 0)
            {
                return BadRequest("Flight Id must be a positive integer.");
            }

            try
            {
                Flight? flight = await _context.Flights
                    .Include(f => f.Destination)
                    .Include(f => f.Origin)
                    .FirstOrDefaultAsync(m => m.FlightId == id);

                if (flight == null)
                {
                    return NotFound($"Flight with Id {id} not found.");
                }

                return Ok(flight);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while processing your request.");
            }
        }

        // GET: api/Flights/GetCheapestMostExpensiveDateUntilFlight
        [HttpGet("GetCheapestMostExpensiveDateUntilFlight,{id}")]
        public async Task<IActionResult> GetCheapestMostExpensiveDateUntilFlight(int id)
        {
            if (id <= 0)
            {
                return BadRequest("Flight Id must be a positive integer.");
            }

            try
            {
                Flight? flight = await _context.Flights
                    .Include(f => f.Destination)
                    .Include(f => f.Origin)
                    .FirstOrDefaultAsync(m => m.FlightId == id);

                if (flight == null)
                {
                    return NotFound($"Flight with Id {id} not found.");
                }

                return Ok(flight);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while processing your request.");
            }
        }

        // GET: api/Flights/GetCheapestMostExpensiveDateWithFlexibility
        [HttpGet("GetCheapestMostExpensiveDateWithFlexibility,{id}")]
        public async Task<IActionResult> GetCheapestMostExpensiveDateWithFlexibility(int id)
        {
            if (id <= 0)
            {
                return BadRequest("Flight Id must be a positive integer.");
            }

            try
            {
                Flight? flight = await _context.Flights
                    .Include(f => f.Destination)
                    .Include(f => f.Origin)
                    .FirstOrDefaultAsync(m => m.FlightId == id);

                if (flight == null)
                {
                    return NotFound($"Flight with Id {id} not found.");
                }

                return Ok(flight);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while processing your request.");
            }
        }

        private FlightDTO FlightToDTO(Flight flight)
        {
            return new FlightDTO
            {
                FlightId = flight.FlightId,
                FlightNumber = flight.FlightNumber,
                FlightDepartureTime = flight.FlightDepartureTime,
                FlightArrivalTime = flight.FlightArrivalTime,
                Origin = AirportToDTO(flight.Origin),
                Destination = AirportToDTO(flight.Destination),
                Price = 0
            };
        }

        private FlightDataDTO FlightDataToDTO(FlightData flightData)
        {
            return new FlightDataDTO
            {
                FlightDataId = flightData.FlightDataId,
                FlightId = flightData.FlightId,
                FetchedTime = flightData.FetchedTime,
                Price = flightData.Price,
                Flight = FlightToDTO(flightData.Flight)
            };
        }

        private AirportDTO AirportToDTO(Airport airport)
        {
            return new AirportDTO
            {
                Name = airport.Name,
                Code = airport.IATA
            };
        }
    }
}

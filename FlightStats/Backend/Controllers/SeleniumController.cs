using Backend.Models;
using Backend.Selenium;
using Hangfire;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Shared.DTOs;

namespace Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SeleniumController(FlightStatsDbContext context, ISeleniumFlights selenium) : ControllerBase
    {
        private readonly FlightStatsDbContext _context = context;
        private readonly ISeleniumFlights _seleniumFlights = selenium;

        #region GeneralEndpoints

        // GET: api/Flights/GetAllFlights
        [HttpGet("GetAllFlights")] // date needs to be like: 2025-01-02T15:30:00
        public async Task<IActionResult> GetAllFlights([FromQuery] int originId, [FromQuery] int destinationId, [FromQuery] DateTime flightDate)
        {
            if (originId <= 0 || destinationId <= 0)
            {
                return BadRequest("Origin and destination Ids must be positive integers.");
            }

            try
            {
                Airport? airportOrigin = await _context.Airports.FirstOrDefaultAsync(a => a.AirportId == originId);
                Airport? airportDestination = await _context.Airports.FirstOrDefaultAsync(a => a.AirportId == destinationId);

                if (airportOrigin is null || airportDestination is null)
                {
                    return NotFound("One or both airports not found.");
                }

                List<FlightDTO> availableFlights = _seleniumFlights.GetAllFlights(airportOrigin, airportDestination, flightDate);

                if (availableFlights is null)
                {
                    return NotFound("No flights found");
                }

                return Ok(availableFlights);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while processing your request.");
            }
        }

        // to update a flight, send an already existing "JobForFlight_{flightNumber}" => updated with params
        // no try catch because no db or other critical component, hangfire has own exception handling
        // POST: api/Flights/NewOrUpdateJobFlight
        [HttpPost("NewOrUpdateJobFlight")]
        public async Task<IActionResult> NewOrUpdateJobFlightAsync([FromQuery] int originId, [FromQuery] int destinationId, [FromQuery] DateTime flightDate, [FromQuery] string flightNumber, [FromQuery] string cronExpression)
        {
            if (originId <= 0 || destinationId <= 0)
            {
                return BadRequest("Origin and destination Ids must be positive integers.");
            };

            if (string.IsNullOrWhiteSpace(flightNumber))
            {
                return BadRequest("Flight number cannot be empty.");
            }

            if (flightDate <= DateTime.Today)
            {
                return BadRequest("Flight can't be today or in the past");
            }

            Airport? airportOrigin = await _context.Airports.FindAsync(originId);
            Airport? airportDestination = await _context.Airports.FindAsync(destinationId);

            if (airportOrigin is null || airportDestination is null)
            {
                return NotFound("Origin or destination not found.");
            }

            FlightDTO? flightDetails = _seleniumFlights.GetSpecificFlight(airportOrigin, airportDestination, flightDate, flightNumber);

            if (flightDetails is null)
            {
                return NotFound("Flight couldn't be found.");
            }

            Func<string> cronJob = () => cronExpression;

            RecurringJob.AddOrUpdate($"JobForFlight_{flightNumber}", () => TrackNewFlightAndSaveJob(originId, destinationId, flightDate, flightNumber), cronJob);

            return Ok();
        }

        // no try catch because hangfire has own exception handling
        // public because of hangfire
        [NonAction]
        public async Task TrackNewFlightAndSaveJob(int originId, int destinationId, DateTime flightDate, string flightNumber)
        {
            if (originId <= 0 || destinationId <= 0 || flightNumber.IsNullOrEmpty())
            {
                return;
            };

            // if flights date is <= call date, delete job. keep data for stats
            if (flightDate <= DateTime.Today)
            {
                RecurringJob.RemoveIfExists($"JobForFlight_{flightNumber}");
                return;
            }

            Airport? airportOrigin = await _context.Airports.FindAsync(originId);
            Airport? airportDestination = await _context.Airports.FindAsync(destinationId);

            if (airportOrigin is null || airportDestination is null)
            {
                return;
            }

            FlightDTO? flightDetails = _seleniumFlights.GetSpecificFlight(airportOrigin, airportDestination, flightDate, flightNumber);

            if (flightDetails is null)
            {
                return;
            }

            Flight? dbFlight = await _context.Flights.FirstOrDefaultAsync(f => f.FlightNumber == flightNumber);

            Flight flight = dbFlight ?? new Flight
            {
                Destination = airportDestination,
                Origin = airportOrigin,
                FlightNumber = flightNumber,
                FlightDepartureTime = flightDetails.FlightDepartureTime,
                FlightArrivalTime = flightDetails.FlightArrivalTime,
            };

            if (dbFlight is null)
            {
                await _context.Flights.AddAsync(flight);
            }

            FlightData flightData = new()
            {
                Flight = flight,
                FetchedTime = DateTime.Now,
                Price = flightDetails.Price,
            };

            await _context.FlightData.AddAsync(flightData);
            await _context.SaveChangesAsync();
        }

        // DELETE: api/Flights/DeleteJobFlight
        [HttpDelete("DeleteJobFlight")]
        public IActionResult DeleteJobFlight([FromQuery] string flightNumber)
        {
            if (string.IsNullOrWhiteSpace(flightNumber))
            {
                return BadRequest("Flight number is required.");
            }

            RecurringJob.RemoveIfExists($"JobForFlight_{flightNumber}");
            return Ok();
        }

        // DELETE: api/Flights/DeleteJobFlightAndAllInfo,5
        [HttpDelete("DeleteJobFlightAndAllInfo,{id}")]
        public async Task<IActionResult> DeleteJobFlightAndAllInfo(int id)
        {
            if (id <= 0)
            {
                return BadRequest("Id must be a positive integer");
            }

            try
            {
                Flight? flight = await _context.Flights.FindAsync(id);

                if (flight is null)
                {
                    return NotFound($"Couldn't find a flight with Id {id}");
                }

                _context.Flights.Remove(flight);
                await _context.SaveChangesAsync();
                RecurringJob.RemoveIfExists($"JobForFlight_{flight.FlightNumber}");
                return Ok();
            }
            catch
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while processing your request.");
            }
        }

        #endregion

        #region StatsEndpoints

        // GET: api/Flights/GetCheapestMostExpensiveDateWithFlexibility
        [HttpGet("GetCheapestMostExpensiveDateWithFlexibility")]
        public async Task<IActionResult> GetCheapestMostExpensiveDateWithFlexibility([FromQuery] int originId, [FromQuery] int destinationId, [FromQuery] DateTime flightDate, [FromQuery] string flightNumber, [FromQuery] int flexibility)
        {
            if (originId <= 0 || destinationId <= 0)
            {
                return BadRequest("Origin and destination Ids must be positive integers.");
            };

            if (flightNumber.IsNullOrEmpty())
            {
                return BadRequest("Flight number can't be null or empty.");
            }

            if (flexibility <= 0 || flexibility > 5)
            {
                return BadRequest("Flexibility must be between 1 and 5.");
            }

            if (flightDate <= DateTime.Today)
            {
                return BadRequest("Flight can't be today or in the past");
            }

            Airport? airportOrigin = await _context.Airports.FindAsync(originId);
            Airport? airportDestination = await _context.Airports.FindAsync(destinationId);

            if (airportOrigin is null || airportDestination is null)
            {
                return NotFound("One or both airports not found.");
            }

            try
            {
                List<DayPrice> dayPrices = _seleniumFlights.GetSpeGetCheapestMostExpensiveDateWithFlexibilitycificFlight(airportOrigin, airportDestination, flightDate, flightNumber, flexibility);

                return Ok(dayPrices);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while processing your request.");
            }
        }

        #endregion
    }
}

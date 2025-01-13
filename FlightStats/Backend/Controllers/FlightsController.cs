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

        #region GeneralEndpoints

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
                return Ok(flights.Select(f => FlightToDTO(f)).ToList());
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

                if (flight is null)
                {
                    return NotFound($"Flight with Id {id} not found.");
                }

                return Ok(FlightToDTO(flight));
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while processing your request.");
            }
        }

        #endregion

        #region StatsEndpoints

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

                if (flight is null)
                {
                    return NotFound($"Flight with Id {id} not found.");
                }

                List<FlightData> flightDatas = await _context.FlightData.Where(f => f.FlightId == id).ToListAsync();
                List<DayPrice> values = [];

                foreach (DayOfWeek day in Enum.GetValues(typeof(DayOfWeek)))
                {
                    List<FlightData> dayDatas = flightDatas.Where(_ => _.FetchedTime.DayOfWeek == day).OrderBy(_ => _.Price).ToList();

                    if (dayDatas.Count <= 0)
                    {
                        values.Add(new DayPrice() { Day = GetDateFromWeekDay(day), Min = 0, Avg = 0, Max = 0 });
                    }
                    else
                    {
                        int min = dayDatas.First().Price;
                        double avg = dayDatas.Select(_ => _.Price).Average();
                        int max = dayDatas.Last().Price;
                        values.Add(new DayPrice() { Day = GetDateFromWeekDay(day), Min = min, Avg = avg, Max = max });
                    }
                }

                return Ok(values);
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

                if (flight is null)
                {
                    return NotFound($"Flight with Id {id} not found.");
                }

                List<FlightData> values = await _context.FlightData.Where(f => f.FlightId == id).OrderBy(_ => _.Price).ToListAsync();

                FlightData? cheapest = values.FirstOrDefault();
                FlightData? mostExpensive = values.LastOrDefault();

                if (cheapest is null || mostExpensive is null)
                {
                    return NotFound();
                }

                List<DayPrice> lowestHighest =
                [
                    FlightDataToDayPrice(cheapest),
                    FlightDataToDayPrice(mostExpensive)
                ];

                return Ok(lowestHighest);
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

                if (flight is null)
                {
                    return NotFound($"Flight with Id {id} not found.");
                }

                List<FlightData> flightDatas = await _context.FlightData.Where(f => f.FlightId == id).ToListAsync();
                List<FlightData> flightDates = flightDatas.DistinctBy(_ => _.FetchedTime.Date).ToList();

                List<DayPrice> values = [];

                foreach (FlightData dayDatas in flightDates)
                {
                    int min = flightDatas.Where(_ => _.FetchedTime.Date.Equals(dayDatas.FetchedTime.Date)).Select(_ => _.Price).First();
                    double avg = flightDatas.Where(_ => _.FetchedTime.Date.Equals(dayDatas.FetchedTime.Date)).Select(_ => _.Price).Average();
                    int max = flightDatas.Where(_ => _.FetchedTime.Date.Equals(dayDatas.FetchedTime.Date)).Select(_ => _.Price).Last();

                    values.Add(new DayPrice() { Day = dayDatas.FetchedTime.Date, Min = min, Avg = avg, Max = max });
                }

                return Ok(values);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while processing your request.");
            }
        }

        #endregion

        #region HelperMethods

        private static FlightDTO FlightToDTO(Flight flight)
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

        private static FlightDataDTO FlightDataToDTO(FlightData flightData)
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

        public static AirportDTO AirportToDTO(Airport airport)
        {
            return new AirportDTO
            {
                AirportId = airport.AirportId,
                Name = airport.Name,
                Code = airport.IATA
            };
        }

        private static DayPrice FlightDataToDayPrice(FlightData flightData)
        {
            return new DayPrice
            {
                Day = flightData.FetchedTime.Date,
                Avg = flightData.Price
            };
        }

        public static DateTime GetDateFromWeekDay(DayOfWeek dayOfWeek)
        {
            DateTime date = DateTime.Today;
            int offset = date.DayOfWeek - dayOfWeek;
            return date.AddDays(-offset);
        }

        #endregion
    }
}

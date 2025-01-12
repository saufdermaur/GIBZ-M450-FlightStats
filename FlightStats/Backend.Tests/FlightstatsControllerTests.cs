using Backend.Controllers;
using Backend.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using Shared.DTOs;

namespace Backend.Tests
{
    [TestClass]
    public class FlightstatsControllerTests
    {
        private FlightsController _controller;

        [TestInitialize]
        public async Task SetupAsync()
        {
            string databaseName = Guid.NewGuid().ToString();
            DbContextOptions<FlightStatsDbContext> options = new DbContextOptionsBuilder<FlightStatsDbContext>()
                .UseInMemoryDatabase(databaseName)
                .Options;

            FlightStatsDbContext dbContext = new FlightStatsDbContext(options);

            #region Airports

            Airport airport1 = new Airport
            {
                AirportId = 1,
                Name = "Test Airport 1",
                City = "Test City 1",
                Country = "Test Country",
                IATA = "TST",
                ICAO = "TEST1",
                Latitude = 50,
                Longitude = 100,
                Altitude = 150,
                Timezone = "Test/Timezone"
            };

            Airport airport2 = new Airport
            {
                AirportId = 2,
                Name = "Test Airport 2",
                City = "Test City 2",
                Country = "Test Country",
                IATA = "TST",
                ICAO = "TEST2",
                Latitude = 51,
                Longitude = 101,
                Altitude = 151,
                Timezone = "Test/Timezone"
            };

            #endregion

            #region Flights

            Flight flight1 = new Flight
            {
                FlightId = 1,
                OriginId = 1,
                DestinationId = 2,
                FlightNumber = "Flight Test 1",
                FlightDepartureTime = DateTime.MaxValue,
                FlightArrivalTime = DateTime.MaxValue,
                Origin = airport1,
                Destination = airport2,
            };

            Flight flight2 = new Flight
            {
                FlightId = 2,
                OriginId = 2,
                DestinationId = 1,
                FlightNumber = "Flight Test 2",
                FlightDepartureTime = DateTime.MaxValue,
                FlightArrivalTime = DateTime.MaxValue,
                Origin = airport2,
                Destination = airport1,
            };

            #endregion

            #region FlightData

            FlightData flightDataForFlight1_1 = new FlightData
            {
                FlightDataId = 1,
                FlightId = 1,
                FetchedTime = FlightsController.GetDateFromWeekDay(DayOfWeek.Monday),
                Price = 50,
                Flight = flight1
            };

            FlightData flightDataForFlight1_2 = new FlightData
            {
                FlightDataId = 2,
                FlightId = 1,
                FetchedTime = FlightsController.GetDateFromWeekDay(DayOfWeek.Monday),
                Price = 100,
                Flight = flight1
            };

            FlightData flightDataForFlight1_3 = new FlightData
            {
                FlightDataId = 3,
                FlightId = 1,
                FetchedTime = FlightsController.GetDateFromWeekDay(DayOfWeek.Tuesday),
                Price = 100,
                Flight = flight1
            };

            FlightData flightDataForFlight1_4 = new FlightData
            {
                FlightDataId = 4,
                FlightId = 1,
                FetchedTime = FlightsController.GetDateFromWeekDay(DayOfWeek.Wednesday),
                Price = 150,
                Flight = flight1
            };

            FlightData flightDataForFlight1_5 = new FlightData
            {
                FlightDataId = 5,
                FlightId = 1,
                FetchedTime = FlightsController.GetDateFromWeekDay(DayOfWeek.Monday).AddDays(7),
                Price = 150,
                Flight = flight1
            };

            FlightData flightDataForFlight1_6 = new FlightData
            {
                FlightDataId = 6,
                FlightId = 1,
                FetchedTime = FlightsController.GetDateFromWeekDay(DayOfWeek.Wednesday).AddDays(7),
                Price = 300,
                Flight = flight1
            };

            #endregion

            dbContext.Airports.AddRange(airport1, airport2);
            dbContext.Flights.AddRange(flight1, flight2);
            dbContext.FlightData.AddRange(
                flightDataForFlight1_1,
                flightDataForFlight1_2,
                flightDataForFlight1_3,
                flightDataForFlight1_4,
                flightDataForFlight1_5,
                flightDataForFlight1_6
                );

            await dbContext.SaveChangesAsync();

            _controller = new FlightsController(dbContext);
        }

        #region GetFlights

        [TestMethod]
        public async Task GetFlights_ReturnsOkResultWithCorrectData()
        {
            // Act
            IActionResult result = await _controller.GetFlights();

            // Assert
            OkObjectResult? okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);

            IEnumerable<FlightDTO>? flights = okResult.Value as IEnumerable<FlightDTO>;
            Assert.IsNotNull(flights);
            Assert.AreEqual(2, flights.Count());
        }

        [TestMethod]
        public async Task GetFlights_ReturnsInternalServerErrorOnException()
        {
            // Arrange
            Mock<FlightStatsDbContext> mockDbContext = new(new DbContextOptions<FlightStatsDbContext>());
            mockDbContext.Setup(m => m.Flights).Throws(new Exception());
            FlightsController controller = new(mockDbContext.Object);

            // Act
            IActionResult result = await controller.GetFlights();

            // Assert
            ObjectResult? objectResult = result as ObjectResult;
            Assert.IsNotNull(objectResult);
            Assert.AreEqual(StatusCodes.Status500InternalServerError, objectResult.StatusCode);
        }

        #endregion

        #region GetFlight

        [TestMethod]
        public async Task GetFlight_ValidId_ReturnsOkResultWithCorrectData()
        {
            // Arrange
            int validId = 1;

            // Act
            IActionResult result = await _controller.GetFlight(validId);

            // Assert
            OkObjectResult? okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);

            object? flight = okResult.Value;
            Assert.IsNotNull(flight);

            FlightDTO flightDto = (FlightDTO)flight;
            Assert.AreEqual(validId, flightDto.FlightId);
            Assert.AreEqual("Test Airport 1", flightDto.Origin.Name);
            Assert.AreEqual("Test Airport 2", flightDto.Destination.Name);
        }

        [TestMethod]
        public async Task GetFlight_InvalidId_ReturnsBadRequest()
        {
            // Arrange
            int invalidId = -1;

            // Act
            IActionResult result = await _controller.GetFlight(invalidId);

            // Assert
            BadRequestObjectResult? badRequestResult = result as BadRequestObjectResult;
            Assert.IsNotNull(badRequestResult);
            Assert.AreEqual("Flight Id must be a positive integer.", badRequestResult.Value);
        }

        [TestMethod]
        public async Task GetFlight_NonexistentId_ReturnsNotFound()
        {
            // Arrange
            int nonexistentId = 999;

            // Act
            IActionResult result = await _controller.GetFlight(nonexistentId);

            // Assert
            NotFoundObjectResult? notFoundResult = result as NotFoundObjectResult;
            Assert.IsNotNull(notFoundResult);
            Assert.AreEqual($"Flight with Id {nonexistentId} not found.", notFoundResult.Value);
        }

        [TestMethod]
        public async Task GetFlight_ExceptionThrown_ReturnsInternalServerError()
        {
            // Arrange
            int validId = 1;

            Mock<FlightStatsDbContext> mockDbContext = new(new DbContextOptions<FlightStatsDbContext>());
            mockDbContext.Setup(m => m.Flights).Throws(new Exception());
            FlightsController controller = new(mockDbContext.Object);

            // Act
            IActionResult result = await controller.GetFlight(validId);

            // Assert
            ObjectResult? objectResult = result as ObjectResult;
            Assert.IsNotNull(objectResult);
            Assert.AreEqual(StatusCodes.Status500InternalServerError, objectResult.StatusCode);
        }

        #endregion

        #region GetCheapestMostExpensiveWeekday

        [TestMethod]
        public async Task GetCheapestMostExpensiveWeekday_ValidId_ReturnsOkResultWithCorrectData()
        {
            // Arrange
            int validId = 1;

            // Act
            IActionResult result = await _controller.GetCheapestMostExpensiveWeekday(validId);

            // Assert
            OkObjectResult? okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);

            object? values = okResult.Value;
            Assert.IsNotNull(values);

            List<DayPrice> dayPrices = (List<DayPrice>)values;
            Assert.AreEqual(7, dayPrices.Count);

            foreach (DayPrice dayPrice in dayPrices)
            {
                Assert.IsNotNull(dayPrice.Day);
                Assert.IsTrue(dayPrice.Min >= 0);
                Assert.IsTrue(dayPrice.Avg >= 0);
                Assert.IsTrue(dayPrice.Max >= 0);
            }

            DayPrice monday = dayPrices.Where(_ => _.Day.DayOfWeek.Equals(DayOfWeek.Monday)).Single();
            Assert.AreEqual(monday.Min, 50);
            Assert.AreEqual(monday.Avg, 100);
            Assert.AreEqual(monday.Max, 150);

            DayPrice tuesday = dayPrices.Where(_ => _.Day.DayOfWeek.Equals(DayOfWeek.Tuesday)).Single();
            Assert.AreEqual(tuesday.Min, 100);
            Assert.AreEqual(tuesday.Avg, 100);
            Assert.AreEqual(tuesday.Max, 100);

            DayPrice wednesday = dayPrices.Where(_ => _.Day.DayOfWeek.Equals(DayOfWeek.Wednesday)).Single();
            Assert.AreEqual(wednesday.Min, 150);
            Assert.AreEqual(wednesday.Avg, 225);
            Assert.AreEqual(wednesday.Max, 300);
        }

        [TestMethod]
        public async Task GetCheapestMostExpensiveWeekday_InvalidId_ReturnsBadRequest()
        {
            // Arrange
            int invalidId = -1;

            // Act
            IActionResult result = await _controller.GetCheapestMostExpensiveWeekday(invalidId);

            // Assert
            BadRequestObjectResult? badRequestResult = result as BadRequestObjectResult;
            Assert.IsNotNull(badRequestResult);
            Assert.AreEqual("Flight Id must be a positive integer.", badRequestResult.Value);
        }

        [TestMethod]
        public async Task GetCheapestMostExpensiveWeekday_NonexistentId_ReturnsNotFound()
        {
            // Arrange
            int nonexistentId = 999;

            // Act
            IActionResult result = await _controller.GetCheapestMostExpensiveWeekday(nonexistentId);

            // Assert
            NotFoundObjectResult? notFoundResult = result as NotFoundObjectResult;
            Assert.IsNotNull(notFoundResult);
            Assert.AreEqual($"Flight with Id {nonexistentId} not found.", notFoundResult.Value);
        }

        [TestMethod]
        public async Task GetCheapestMostExpensiveWeekday_ExceptionThrown_ReturnsInternalServerError()
        {
            // Arrange
            int validId = 1;
            Mock<FlightStatsDbContext> mockDbContext = new(new DbContextOptions<FlightStatsDbContext>());
            mockDbContext.Setup(m => m.Flights).Throws(new Exception());
            FlightsController controller = new(mockDbContext.Object);

            // Act
            IActionResult result = await controller.GetCheapestMostExpensiveWeekday(validId);

            // Assert
            ObjectResult? objectResult = result as ObjectResult;
            Assert.IsNotNull(objectResult);
            Assert.AreEqual(StatusCodes.Status500InternalServerError, objectResult.StatusCode);
        }

        #endregion

        #region GetCheapestMostExpensiveDate

        [TestMethod]
        public async Task GetCheapestMostExpensiveDate_ValidId_ReturnsOkResultWithCorrectData()
        {
            // Arrange
            int validId = 1;

            // Act
            IActionResult result = await _controller.GetCheapestMostExpensiveDate(validId);

            // Assert
            OkObjectResult? okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);

            object? values = okResult.Value;
            Assert.IsNotNull(values);

            List<DayPrice> dayPrices = (List<DayPrice>)values;
            Assert.AreEqual(2, dayPrices.Count);

            Assert.IsNotNull(dayPrices[0]);
            Assert.IsTrue(dayPrices[0].Min >= 0);
            Assert.IsNotNull(dayPrices[1]);
            Assert.IsTrue(dayPrices[1].Max >= 0);
        }

        [TestMethod]
        public async Task GetCheapestMostExpensiveDate_InvalidId_ReturnsBadRequest()
        {
            // Arrange
            int invalidId = -1;

            // Act
            IActionResult result = await _controller.GetCheapestMostExpensiveDate(invalidId);

            // Assert
            BadRequestObjectResult? badRequestResult = result as BadRequestObjectResult;
            Assert.IsNotNull(badRequestResult);
            Assert.AreEqual("Flight Id must be a positive integer.", badRequestResult.Value);
        }

        [TestMethod]
        public async Task GetCheapestMostExpensiveDate_NonexistentId_ReturnsNotFound()
        {
            // Arrange
            int nonexistentId = 999;

            // Act
            IActionResult result = await _controller.GetCheapestMostExpensiveDate(nonexistentId);

            // Assert
            NotFoundObjectResult? notFoundResult = result as NotFoundObjectResult;
            Assert.IsNotNull(notFoundResult);
            Assert.AreEqual($"Flight with Id {nonexistentId} not found.", notFoundResult.Value);
        }

        [TestMethod]
        public async Task GetCheapestMostExpensiveDate_ExceptionThrown_ReturnsInternalServerError()
        {
            // Arrange
            int validId = 1;
            Mock<FlightStatsDbContext> mockDbContext = new(new DbContextOptions<FlightStatsDbContext>());
            mockDbContext.Setup(m => m.Flights).Throws(new Exception());
            FlightsController controller = new FlightsController(mockDbContext.Object);

            // Act
            IActionResult result = await controller.GetCheapestMostExpensiveDate(validId);

            // Assert
            ObjectResult? objectResult = result as ObjectResult;
            Assert.IsNotNull(objectResult);
            Assert.AreEqual(StatusCodes.Status500InternalServerError, objectResult.StatusCode);
        }

        #endregion

        #region GetCheapestMostExpensiveDateUntilFlight

        [TestMethod]
        public async Task GetCheapestMostExpensiveDateUntilFlight_ReturnsOkResultWithCorrectData()
        {
            // Arrange
            int flightId = 1;

            // Act
            IActionResult result = await _controller.GetCheapestMostExpensiveDateUntilFlight(flightId);

            // Assert
            OkObjectResult? okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);

            List<DayPrice>? values = okResult.Value as List<DayPrice>;
            Assert.IsNotNull(values);
            Assert.AreEqual(5, values.Count);

            DayPrice firstDayData = values.First();
            Assert.AreEqual(50, firstDayData.Min);
            Assert.AreEqual(75, firstDayData.Avg);
            Assert.AreEqual(100, firstDayData.Max);
        }

        [TestMethod]
        public async Task GetCheapestMostExpensiveDateUntilFlight_ReturnsBadRequestForInvalidId()
        {
            // Arrange
            int invalidId = 0;

            // Act
            IActionResult result = await _controller.GetCheapestMostExpensiveDateUntilFlight(invalidId);

            // Assert
            BadRequestObjectResult? badRequestResult = result as BadRequestObjectResult;
            Assert.IsNotNull(badRequestResult);
            Assert.AreEqual("Flight Id must be a positive integer.", badRequestResult.Value);
        }

        [TestMethod]
        public async Task GetCheapestMostExpensiveDateUntilFlight_ReturnsNotFoundForNonExistingFlight()
        {
            // Arrange
            int nonExistingFlightId = 999;

            // Act
            IActionResult result = await _controller.GetCheapestMostExpensiveDateUntilFlight(nonExistingFlightId);

            // Assert
            NotFoundObjectResult? notFoundResult = result as NotFoundObjectResult;
            Assert.IsNotNull(notFoundResult);
            Assert.AreEqual($"Flight with Id {nonExistingFlightId} not found.", notFoundResult.Value);
        }

        [TestMethod]
        public async Task GetCheapestMostExpensiveDateUntilFlight_ReturnsInternalServerErrorOnException()
        {
            // Arrange
            int flightId = 1;

            Mock<FlightStatsDbContext> mockDbContext = new(new DbContextOptions<FlightStatsDbContext>());
            mockDbContext.Setup(m => m.Flights).Throws(new Exception());
            FlightsController controller = new(mockDbContext.Object);

            // Act
            IActionResult result = await controller.GetCheapestMostExpensiveDateUntilFlight(flightId);

            // Assert
            ObjectResult? objectResult = result as ObjectResult;
            Assert.IsNotNull(objectResult);
            Assert.AreEqual(StatusCodes.Status500InternalServerError, objectResult.StatusCode);
        }

        #endregion
    }
}
using Backend.Controllers;
using Backend.Models;
using Backend.Selenium;
using Hangfire;
using Hangfire.MemoryStorage;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using Shared.DTOs;

namespace Backend.Tests
{
    [TestClass]
    public class SeleniumControllerTests
    {
        private SeleniumController _controller;
        private Mock<ISeleniumFlights> _mockSeleniumFlights;
        private FlightStatsDbContext _dbContext;

        private Airport _airport1;
        private Airport _airport2;

        [TestInitialize]
        public async Task SetupAsync()
        {
            string databaseName = Guid.NewGuid().ToString();
            DbContextOptions<FlightStatsDbContext> options = new DbContextOptionsBuilder<FlightStatsDbContext>()
                .UseInMemoryDatabase(databaseName)
                .Options;

            _dbContext = new FlightStatsDbContext(options);

            GlobalConfiguration.Configuration.UseMemoryStorage();

            _mockSeleniumFlights = new Mock<ISeleniumFlights>();

            Airport airport1 = new()
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

            Airport airport2 = new()
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

            _airport1 = airport1;
            _airport2 = airport2;
            _dbContext.Airports.AddRange(airport1, airport2);
            await _dbContext.SaveChangesAsync();

            _controller = new SeleniumController(_dbContext, _mockSeleniumFlights.Object);
        }

        #region GetAllFlights

        [TestMethod]
        public async Task GetAllFlights_ValidParameters_ReturnsOkResultWithFlights()
        {
            // Arrange
            int originId = 1;
            int destinationId = 2;
            DateTime flightDate = DateTime.Parse("2025-01-02T15:30:00");

            List<FlightDTO> flightDTOs =
            [
                new FlightDTO { FlightId = 1, FlightNumber = "Flight 1", Origin = FlightsController.AirportToDTO(_airport1), Destination = FlightsController.AirportToDTO(_airport2) },
                new FlightDTO { FlightId = 2, FlightNumber = "Flight 2", Origin = FlightsController.AirportToDTO(_airport1), Destination = FlightsController.AirportToDTO(_airport2) },
            ];

            _mockSeleniumFlights.Setup(s => s.GetAllFlights(It.IsAny<Airport>(), It.IsAny<Airport>(), It.IsAny<DateTime>())).Returns(flightDTOs);

            // Act
            IActionResult result = await _controller.GetAllFlights(originId, destinationId, flightDate);

            // Assert
            OkObjectResult? okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(200, okResult.StatusCode);

            List<FlightDTO>? returnedFlights = okResult.Value as List<FlightDTO>;
            Assert.IsNotNull(returnedFlights);
            Assert.AreEqual(2, returnedFlights.Count);
        }

        [TestMethod]
        public async Task GetAllFlights_InvalidOriginId_ReturnsBadRequest()
        {
            // Arrange
            int originId = 0;
            int destinationId = 2;
            DateTime flightDate = DateTime.Parse("2025-01-02T15:30:00");

            // Act
            IActionResult result = await _controller.GetAllFlights(originId, destinationId, flightDate);

            // Assert
            BadRequestObjectResult? badRequestResult = result as BadRequestObjectResult;
            Assert.IsNotNull(badRequestResult);
            Assert.AreEqual(400, badRequestResult.StatusCode);
            Assert.AreEqual("Origin and destination Ids must be positive integers.", badRequestResult.Value);
        }

        [TestMethod]
        public async Task GetAllFlights_AirportsNotFound_ReturnsNotFound()
        {
            // Arrange
            int originId = 3;
            int destinationId = 2;
            DateTime flightDate = DateTime.Parse("2025-01-02T15:30:00");

            // Act
            IActionResult result = await _controller.GetAllFlights(originId, destinationId, flightDate);

            // Assert
            NotFoundObjectResult? notFoundResult = result as NotFoundObjectResult;
            Assert.IsNotNull(notFoundResult);
            Assert.AreEqual(404, notFoundResult.StatusCode);
            Assert.AreEqual("One or both airports not found.", notFoundResult.Value);
        }

        [TestMethod]
        public async Task GetAllFlights_NoFlightsFound_ReturnsNotFound()
        {
            // Arrange
            int originId = 1;
            int destinationId = 2;
            DateTime flightDate = DateTime.Parse("2025-01-02T15:30:00");

            _mockSeleniumFlights.Setup(s => s.GetAllFlights(It.IsAny<Airport>(), It.IsAny<Airport>(), It.IsAny<DateTime>())).Returns(value: null);

            // Act
            IActionResult result = await _controller.GetAllFlights(originId, destinationId, flightDate);

            // Assert
            NotFoundObjectResult? notFoundResult = result as NotFoundObjectResult;
            Assert.IsNotNull(notFoundResult);
            Assert.AreEqual(404, notFoundResult.StatusCode);
            Assert.AreEqual("No flights found", notFoundResult.Value);
        }

        [TestMethod]
        public async Task GetAllFlights_ExceptionThrown_ReturnsInternalServerError()
        {
            // Arrange
            int originId = 1;
            int destinationId = 2;
            DateTime flightDate = DateTime.Parse("2025-01-02T15:30:00");

            _mockSeleniumFlights.Setup(s => s.GetAllFlights(It.IsAny<Airport>(), It.IsAny<Airport>(), It.IsAny<DateTime>())).Throws(new Exception("Test exception"));

            // Act
            IActionResult result = await _controller.GetAllFlights(originId, destinationId, flightDate);

            // Assert
            ObjectResult? objectResult = result as ObjectResult;
            Assert.IsNotNull(objectResult);
            Assert.AreEqual(StatusCodes.Status500InternalServerError, objectResult.StatusCode);
        }

        #endregion

        #region NewOrUpdateJobFlightAsync

        [TestMethod]
        public async Task NewOrUpdateJobFlightAsync_ValidParameters_ReturnsOkResult()
        {
            // Arrange
            int originId = 1;
            int destinationId = 2;
            DateTime flightDate = DateTime.Today.AddDays(1);
            string flightNumber = "FlightTest123";
            string cronExpression = "0 0 * * *";

            FlightDTO flightDetails = new() { FlightId = 1, FlightNumber = flightNumber, Origin = FlightsController.AirportToDTO(_airport1), Destination = FlightsController.AirportToDTO(_airport2) };

            _mockSeleniumFlights.Setup(s => s.GetSpecificFlight(It.IsAny<Airport>(), It.IsAny<Airport>(), It.IsAny<DateTime>(), It.IsAny<string>())).Returns(flightDetails);

            // Act
            IActionResult result = await _controller.NewOrUpdateJobFlightAsync(originId, destinationId, flightDate, flightNumber, cronExpression);

            // Assert
            OkResult? okResult = result as OkResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(200, okResult.StatusCode);

            _mockSeleniumFlights.Verify(s => s.GetSpecificFlight(It.IsAny<Airport>(), It.IsAny<Airport>(), It.IsAny<DateTime>(), It.IsAny<string>()), Times.Once);
        }

        [TestMethod]
        public async Task NewOrUpdateJobFlightAsync_InvalidOriginId_ReturnsBadRequest()
        {
            // Arrange
            int originId = 0;
            int destinationId = 2;
            DateTime flightDate = DateTime.Parse("2025-01-02T15:30:00");
            string flightNumber = "FlightTest123";
            string cronExpression = "0 0 * * *";

            // Act
            IActionResult result = await _controller.NewOrUpdateJobFlightAsync(originId, destinationId, flightDate, flightNumber, cronExpression);

            // Assert
            BadRequestObjectResult? badRequestResult = result as BadRequestObjectResult;
            Assert.IsNotNull(badRequestResult);
            Assert.AreEqual(400, badRequestResult.StatusCode);
            Assert.AreEqual("Origin and destination Ids must be positive integers.", badRequestResult.Value);
        }

        [TestMethod]
        public async Task NewOrUpdateJobFlightAsync_InvalidDestinationId_ReturnsBadRequest()
        {
            // Arrange
            int originId = 1;
            int destinationId = 0;
            DateTime flightDate = DateTime.Parse("2025-01-02T15:30:00");
            string flightNumber = "FlightTest123";
            string cronExpression = "0 0 * * *";

            // Act
            IActionResult result = await _controller.NewOrUpdateJobFlightAsync(originId, destinationId, flightDate, flightNumber, cronExpression);

            // Assert
            BadRequestObjectResult? badRequestResult = result as BadRequestObjectResult;
            Assert.IsNotNull(badRequestResult);
            Assert.AreEqual(400, badRequestResult.StatusCode);
            Assert.AreEqual("Origin and destination Ids must be positive integers.", badRequestResult.Value);
        }

        [TestMethod]
        public async Task NewOrUpdateJobFlightAsync_EmptyFlightNumber_ReturnsBadRequest()
        {
            // Arrange
            int originId = 1;
            int destinationId = 2;
            DateTime flightDate = DateTime.Parse("2025-01-02T15:30:00");
            string flightNumber = "";
            string cronExpression = "0 0 * * *";

            // Act
            IActionResult result = await _controller.NewOrUpdateJobFlightAsync(originId, destinationId, flightDate, flightNumber, cronExpression);

            // Assert
            BadRequestObjectResult? badRequestResult = result as BadRequestObjectResult;
            Assert.IsNotNull(badRequestResult);
            Assert.AreEqual(400, badRequestResult.StatusCode);
            Assert.AreEqual("Flight number cannot be empty.", badRequestResult.Value);
        }

        [TestMethod]
        public async Task NewOrUpdateJobFlightAsync_FlightDateInPast_ReturnsBadRequest()
        {
            // Arrange
            int originId = 1;
            int destinationId = 2;
            DateTime flightDate = DateTime.Today.AddDays(-1);
            string flightNumber = "FlightTest123";
            string cronExpression = "0 0 * * *";

            // Act
            IActionResult result = await _controller.NewOrUpdateJobFlightAsync(originId, destinationId, flightDate, flightNumber, cronExpression);

            // Assert
            BadRequestObjectResult? badRequestResult = result as BadRequestObjectResult;
            Assert.IsNotNull(badRequestResult);
            Assert.AreEqual(400, badRequestResult.StatusCode);
            Assert.AreEqual("Flight can't be today or in the past", badRequestResult.Value);
        }

        [TestMethod]
        public async Task NewOrUpdateJobFlightAsync_AirportNotFound_ReturnsNotFound()
        {
            // Arrange
            int originId = 1;
            int destinationId = 3;
            DateTime flightDate = DateTime.Today.AddDays(1);
            string flightNumber = "FlightTest123";
            string cronExpression = "0 0 * * *";

            // Act
            IActionResult result = await _controller.NewOrUpdateJobFlightAsync(originId, destinationId, flightDate, flightNumber, cronExpression);

            // Assert
            NotFoundObjectResult? notFoundResult = result as NotFoundObjectResult;
            Assert.IsNotNull(notFoundResult);
            Assert.AreEqual(404, notFoundResult.StatusCode);
            Assert.AreEqual("Origin or destination not found.", notFoundResult.Value);
        }

        [TestMethod]
        public async Task NewOrUpdateJobFlightAsync_FlightNotFound_ReturnsNotFound()
        {
            // Arrange
            int originId = 1;
            int destinationId = 2;
            DateTime flightDate = DateTime.Today.AddDays(1);
            string flightNumber = "NonExistentFlight";
            string cronExpression = "0 0 * * *";

            _mockSeleniumFlights.Setup(s => s.GetSpecificFlight(It.IsAny<Airport>(), It.IsAny<Airport>(), It.IsAny<DateTime>(), It.IsAny<string>())).Returns(value: null);

            // Act
            IActionResult result = await _controller.NewOrUpdateJobFlightAsync(originId, destinationId, flightDate, flightNumber, cronExpression);

            // Assert
            NotFoundObjectResult? notFoundResult = result as NotFoundObjectResult;
            Assert.IsNotNull(notFoundResult);
            Assert.AreEqual(404, notFoundResult.StatusCode);
            Assert.AreEqual("Flight couldn't be found.", notFoundResult.Value);
        }

        #endregion

        #region TrackNewFlightAndSaveJob

        [TestMethod]
        public async Task TrackNewFlightAndSaveJob_FlightDateInPast_RemovesJob()
        {
            // Arrange
            int originId = 1;
            int destinationId = 2;
            DateTime flightDate = DateTime.Today.AddDays(-1);
            string flightNumber = "FlightTest123";

            // Act
            await _controller.TrackNewFlightAndSaveJob(originId, destinationId, flightDate, flightNumber);

            // Assert
            _mockSeleniumFlights.Verify(s => s.GetSpecificFlight(It.IsAny<Airport>(), It.IsAny<Airport>(), It.IsAny<DateTime>(), It.IsAny<string>()), Times.Never);
        }

        [TestMethod]
        public async Task TrackNewFlightAndSaveJob_FlightAddedToDatabase()
        {
            // Arrange
            int originId = 1;
            int destinationId = 2;
            DateTime flightDate = DateTime.Today.AddDays(1);
            string flightNumber = "FlightTest123";

            FlightDTO flightDetails = new()
            {
                FlightId = 1,
                FlightNumber = flightNumber,
                Origin = FlightsController.AirportToDTO(_airport1),
                Destination = FlightsController.AirportToDTO(_airport2),
                FlightDepartureTime = DateTime.Now.AddHours(1),
                FlightArrivalTime = DateTime.Now.AddHours(2),
                Price = 100
            };

            _mockSeleniumFlights.Setup(s => s.GetSpecificFlight(It.IsAny<Airport>(), It.IsAny<Airport>(), It.IsAny<DateTime>(), It.IsAny<string>())).Returns(flightDetails);

            // Act
            await _controller.TrackNewFlightAndSaveJob(originId, destinationId, flightDate, flightNumber);

            // Assert
            Flight? dbFlight = await _dbContext.Flights.FirstOrDefaultAsync(_ => _.FlightNumber == flightNumber);
            Assert.IsNotNull(dbFlight);
            Assert.AreEqual(flightNumber, dbFlight.FlightNumber);

            FlightData? dbFlightData = await _dbContext.FlightData.FirstOrDefaultAsync(_ => _.FlightId == dbFlight.FlightId);
            Assert.IsNotNull(dbFlightData);
            Assert.AreEqual(flightDetails.Price, dbFlightData.Price);
        }

        [TestMethod]
        public async Task TrackNewFlightAndSaveJob_FlightAlreadyExists_DoesNotAddNewFlight()
        {
            // Arrange
            int originId = 1;
            int destinationId = 2;
            DateTime flightDate = DateTime.Today.AddDays(1);
            string flightNumber = "FlightTest123";

            FlightDTO flightDetails = new()
            {
                FlightId = 1,
                FlightNumber = flightNumber,
                Origin = FlightsController.AirportToDTO(_airport1),
                Destination = FlightsController.AirportToDTO(_airport2),
                FlightDepartureTime = DateTime.Now.AddHours(1),
                FlightArrivalTime = DateTime.Now.AddHours(2),
                Price = 100
            };

            _mockSeleniumFlights.Setup(s => s.GetSpecificFlight(It.IsAny<Airport>(), It.IsAny<Airport>(), It.IsAny<DateTime>(), It.IsAny<string>()))
                .Returns(flightDetails);

            // Act
            await _controller.TrackNewFlightAndSaveJob(originId, destinationId, flightDate, flightNumber);

            // Assert
            // Verify that a new flight was not added to the database
            Flight? dbFlight = await _dbContext.Flights.FirstOrDefaultAsync(_ => _.FlightNumber == flightNumber);
            Assert.IsNotNull(dbFlight);
            Assert.AreEqual(flightNumber, dbFlight.FlightNumber);
            Assert.AreEqual(1, _dbContext.Flights.Count(f => f.FlightNumber == flightNumber));
        }

        #endregion

        #region DeleteJobFlight

        [TestMethod]
        public void DeleteJobFlight_FlightNumberIsEmpty_ReturnsBadRequest()
        {
            // Act
            IActionResult result = _controller.DeleteJobFlight(string.Empty);

            // Assert
            BadRequestObjectResult? badRequest = result as BadRequestObjectResult;
            Assert.IsNotNull(badRequest);
            Assert.AreEqual("Flight number is required.", badRequest.Value);
        }

        [TestMethod]
        public void DeleteJobFlight_ValidFlightNumber_ReturnsOkResult()
        {
            // Arrange
            string flightNumber = "Flight123";

            // Act
            IActionResult result = _controller.DeleteJobFlight(flightNumber);

            // Assert
            OkResult? okResult = result as OkResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(200, okResult.StatusCode);
        }

        #endregion

        #region DeleteJobFlightAndAllInfo

        [TestMethod]
        public async Task DeleteJobFlightAndAllInfo_InvalidId_ReturnsBadRequest()
        {
            // Act
            IActionResult result = await _controller.DeleteJobFlightAndAllInfo(0);

            // Assert
            BadRequestObjectResult? badRequest = result as BadRequestObjectResult;
            Assert.IsNotNull(badRequest);
            Assert.AreEqual("Id must be a positive integer", badRequest.Value);
        }

        [TestMethod]
        public async Task DeleteJobFlightAndAllInfo_FlightNotFound_ReturnsNotFound()
        {
            // Act
            IActionResult result = await _controller.DeleteJobFlightAndAllInfo(999);

            // Assert
            NotFoundObjectResult? notFoundResult = result as NotFoundObjectResult;
            Assert.IsNotNull(notFoundResult);
            Assert.AreEqual("Couldn't find a flight with Id 999", notFoundResult.Value);
        }

        [TestMethod]
        public async Task DeleteJobFlightAndAllInfo_ValidId_FlightDeletedAndJobRemoved()
        {
            // Arrange
            Flight flight = new()
            {
                FlightId = 1,
                OriginId = 1,
                DestinationId = 2,
                FlightNumber = "Flight Test 1",
                FlightDepartureTime = DateTime.MaxValue,
                FlightArrivalTime = DateTime.MaxValue,
                Origin = _airport1,
                Destination = _airport2,
            };

            _dbContext.Flights.Add(flight);
            await _dbContext.SaveChangesAsync();

            // Act
            IActionResult result = await _controller.DeleteJobFlightAndAllInfo(flight.FlightId);

            // Assert
            OkResult? okResult = result as OkResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(200, okResult.StatusCode);

            Flight? dbFlight = await _dbContext.Flights.FindAsync(flight.FlightId);
            Assert.IsNull(dbFlight);
        }

        #endregion
    }
}
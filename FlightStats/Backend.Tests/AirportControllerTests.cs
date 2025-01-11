using Backend.Controllers;
using Backend.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Backend.Tests
{
    [TestClass]
    public class AirportControllerTests
    {
        private AirportsController _controller;

        [TestInitialize]
        public async Task SetupAsync()
        {
            string databaseName = Guid.NewGuid().ToString();

            DbContextOptions<FlightStatsDbContext> options = new DbContextOptionsBuilder<FlightStatsDbContext>()
                .UseInMemoryDatabase(databaseName)
                .Options;

            FlightStatsDbContext dbContext = new FlightStatsDbContext(options);

            dbContext.Airports.AddRange(
                new Airport
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
                },
                new Airport
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
                });

            await dbContext.SaveChangesAsync();

            _controller = new AirportsController(dbContext);
        }

        [TestMethod]
        public async Task SearchAirports_ValidQuery_ReturnsAirports()
        {
            // Arrange
            string query = "Test";

            // Act
            OkObjectResult? result = await _controller.SearchAirports(query) as OkObjectResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(200, result.StatusCode);

            List<Airport>? airports = result.Value as List<Airport>;
            Assert.IsNotNull(airports);
            Assert.AreEqual(2, airports.Count);
        }

        [TestMethod]
        public async Task SearchAirports_EmptyQuery_ReturnsBadRequest()
        {
            // Arrange
            string query = string.Empty;

            // Act
            BadRequestObjectResult? result = await _controller.SearchAirports(query) as BadRequestObjectResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(400, result.StatusCode);
            Assert.AreEqual("Query parameter is required.", result.Value);
        }
    }
}

namespace Backend
{
    using Backend.Models;
    using System.Globalization;

    public static class AirportDataLoader
    {
        public static IEnumerable<Airport> LoadAirports(string filePath)
        {
            var airports = new List<Airport>();
            var lines = File.ReadAllLines(filePath);

            foreach (var line in lines)
            {
                var fields = line.Split(',');

                airports.Add(new Airport
                {
                    AirportId = int.Parse(fields[0].Trim()),
                    Name = fields[1].Trim(),
                    City = fields[2].Trim(),
                    Country = fields[3].Trim(),
                    IATA = fields[4].Trim(),
                    ICAO = fields[5].Trim(),
                    Latitude = float.Parse(fields[6].Trim(), CultureInfo.InvariantCulture),
                    Longitude = float.Parse(fields[7].Trim(), CultureInfo.InvariantCulture),
                    Altitude = int.Parse(fields[8].Trim()),
                    Timezone = fields[9].Trim()
                });
            }

            return airports;
        }
    }

}

namespace Backend.Models
{
    public class Airport
    {
        public int AirportId { get; set; }
        public required string Name { get; set; }
        public required string Code { get; set; }
    }
}

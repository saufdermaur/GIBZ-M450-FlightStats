namespace Shared.DTOs
{
    public class DayPrice
    {
        public DateTime Day { get; set; }
        public double Min { get; set; } = 0;
        public double Avg { get; set; } = 0;
        public double Max { get; set; } = 0;
    }
}

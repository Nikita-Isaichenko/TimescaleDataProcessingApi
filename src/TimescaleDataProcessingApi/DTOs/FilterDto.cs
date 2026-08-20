namespace TimescaleDataProcessingApi.DTOs
{
    public class FilterDto
    {
        public string? FileName { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public double? MinAvgValue { get; set; }
        public double? MaxAvgValue { get; set; }
        public double? MinAvgExecutionTime { get; set; }
        public double? MaxAvgExecutionTime { get; set; }
    }
}

using System.ComponentModel.DataAnnotations;

namespace TimescaleDataProcessingApi.DTOs.Response
{
    public class ResultResponseDto
    {
        public string FileName { get; set; } = string.Empty;

        public double DeltaTimeSeconds { get; set; }

        public DateTime FirstOperationStart { get; set; }

        public double AvgExecutionTime { get; set; }

        public double AvgValue { get; set; }

        public double MedianValue { get; set; }

        public double MaxValue { get; set; }

        public double MinValue { get; set; }
    }
}

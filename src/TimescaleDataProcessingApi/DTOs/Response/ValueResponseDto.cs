using System.ComponentModel.DataAnnotations;

namespace TimescaleDataProcessingApi.DTOs.Response
{
    public class ValueResponseDto
    {
        public int Id { get; set; }

        public DateTime Date { get; set; }

        public double ExecutionTime { get; set; }

        public double Value { get; set; }

        public string FileName { get; set; } = string.Empty;
    }
}

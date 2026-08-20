using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TimescaleDataProcessingApi.Models
{
    /// <summary>
    /// Хранит интегральные вычисления для значений из файла.
    /// </summary>
    [Table("results")]
    public class Result
    {
        [Key]
        public int Id { get; set; }

        public string FileName { get; set; } = string.Empty;

        [Required]
        public double DeltaTimeSeconds { get; set; }

        [Required]
        public DateTime FirstOperationStart { get; set; }

        [Required]
        public double AvgExecutionTime { get; set; }

        [Required]
        public double AvgValue { get; set; }

        [Required]
        public double MedianValue { get; set; }

        [Required]
        public double MaxValue { get; set; }

        [Required]
        public double MinValue { get; set; }

        public ICollection<ValueEntry> Values { get; set; }
    }
}

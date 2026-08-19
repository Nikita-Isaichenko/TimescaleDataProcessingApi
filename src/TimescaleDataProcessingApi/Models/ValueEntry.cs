using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TimescaleDataProcessingApi.Models
{
    [Table("values")]
    public class ValueEntry
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public DateTime Date { get; set; }

        [Required]
        public double ExecutionTime { get; set; }

        [Required]
        public double Value { get; set; }

        [Required]
        public string FileName { get; set; } = string.Empty;

        public int ResultId { get; set; }
        public Result Result { get; set; }
    }
}

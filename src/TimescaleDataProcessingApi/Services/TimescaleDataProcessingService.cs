using Microsoft.EntityFrameworkCore;
using System.Globalization;
using TimescaleDataProcessingApi.DTOs.Request;
using TimescaleDataProcessingApi.DTOs.Response;
using TimescaleDataProcessingApi.Models;

namespace TimescaleDataProcessingApi.Services
{
    /// <summary>
    /// Используется для загрузки и обработки файла, а также получения обработанных данных.
    /// </summary>
    public class TimescaleDataProcessingService
    {
        private const string DateFormat = "yyyy-MM-ddTHH-mm-ss.ffffZ";

        private readonly TimescaleDataDbContext _context;

        public TimescaleDataProcessingService(TimescaleDataDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Обрабатывает Csv файл и загружает обработанные данные в базу данных.
        /// </summary>
        /// <param name="file">Файл.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        public async Task ProcessCsvFile(IFormFile file)
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException("File is not provided");

            var fileName = Path.GetFileNameWithoutExtension(file.FileName);

            await using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var existingValue = await _context.Results
                    .Include(r => r.Values)
                    .FirstOrDefaultAsync(r => r.FileName == fileName);

                if (existingValue != null) 
                {
                    _context.Values.RemoveRange(existingValue.Values);
                    _context.Results.Remove(existingValue);
                    await _context.SaveChangesAsync();
                }

                var values = await TryParseCsvFileAsync(file);
                var result = CalculateData(values, fileName);

                _context.Results.Add(result);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                throw new InvalidOperationException($"File validation error: {ex.Message}");
            }

        }

        /// <summary>
        /// Возвращает подсчитанные интегральные данные из базы данных с использованием фильтров.
        /// </summary>
        /// <param name="filter">Объект, описывающий доступные фильтры.</param>
        /// <returns>Список отфильтрованных данных.</returns>
        public async Task<IEnumerable<ResultResponseDto>> GetFilteredResultsAsync(FilterDto filter)
        {
            IQueryable<Result> query = _context.Results.AsNoTracking();

            if (!string.IsNullOrEmpty(filter.FileName))
            {
                query = query.Where(r => r.FileName == filter.FileName);
            }

            if (filter.StartDate.HasValue)
            {
                query = query.Where(r => r.FirstOperationStart >= filter.StartDate.Value);
            }

            if (filter.EndDate.HasValue)
            {
                query = query.Where(r => r.FirstOperationStart <= filter.EndDate.Value);
            }

            if (filter.MinAvgValue.HasValue)
            {
                query = query.Where(r => r.AvgValue >= filter.MinAvgValue.Value);
            }

            if (filter.MaxAvgValue.HasValue)
            {
                query = query.Where(r => r.AvgValue <= filter.MaxAvgValue.Value);
            }

            if (filter.MinAvgExecutionTime.HasValue)
            {
                query = query.Where(r => r.AvgExecutionTime >= filter.MinAvgExecutionTime.Value);
            }

            if (filter.MaxAvgExecutionTime.HasValue)
            {
                query = query.Where(r => r.AvgExecutionTime <= filter.MaxAvgExecutionTime.Value);
            }

            var results = await query.ToListAsync();

            return results.Select(r => new ResultResponseDto
            {
                Id = r.Id,
                FileName = r.FileName,
                DeltaTimeSeconds = r.DeltaTimeSeconds,
                FirstOperationStart = r.FirstOperationStart,
                AvgExecutionTime = r.AvgExecutionTime,
                AvgValue = r.AvgValue,
                MedianValue = r.MedianValue,
                MaxValue = r.MaxValue,
                MinValue = r.MinValue
            });
        }

        /// <summary>
        /// Возвращает последние 10 значений по времени, которые сооответствуют названию файла.
        /// </summary>
        /// <param name="fileName">Название файла.</param>
        /// <returns>Список значений.</returns>
        /// <exception cref="ArgumentException"></exception>
        public async Task<IEnumerable<ValueResponseDto>> GetLastTenValuesAsync(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
                throw new ArgumentException("File name cannot be empty");

            var existFileName = await _context.Values.AnyAsync(v => v.FileName == fileName);

            if (!existFileName)
                throw new ArgumentException("File name not found");

            var values = await _context.Values
                .AsNoTracking()
                .Where(v => v.FileName == fileName)
                .OrderByDescending(v => v.Date)
                .Take(10)
                .ToListAsync();

            return values.Select(v => new ValueResponseDto
            {
                Id = v.Id,
                Date = v.Date,
                ExecutionTime = v.ExecutionTime,
                Value = v.Value,
                FileName = v.FileName
            });
        }

        /// <summary>
        /// Расчитывает интегральные значения для данных.
        /// </summary>
        /// <param name="values">Данные.</param>
        /// <param name="fileName">Имя файла.</param>
        /// <returns>Объект <see cref="Result"/> с результатами.</returns>
        private Result CalculateData(List<ValueEntry> values, string fileName)
        {
            var minDate = values.Min(v => v.Date);
            var maxDate = values.Max(v => v.Date);
            var deltaSeconds = (maxDate - minDate).TotalSeconds;
            var avgExecTime = values.Average(v => v.ExecutionTime);
            var avgValue = values.Average(v => v.Value);
            var maxValue = values.Max(v => v.Value);
            var minValue = values.Min(v => v.Value);
            var sortedValues = values.Select(v => v.Value).OrderBy(x => x).ToList();
            double median;
            int count = sortedValues.Count;
            if (count % 2 == 0)
                median = (sortedValues[count / 2 - 1] + sortedValues[count / 2]) / 2.0;
            else
                median = sortedValues[count / 2];

            return new Result
            {
                FileName = fileName,
                DeltaTimeSeconds = deltaSeconds,
                FirstOperationStart = minDate,
                AvgExecutionTime = avgExecTime,
                AvgValue = avgValue,
                MedianValue = median,
                MaxValue = maxValue,
                MinValue = minValue,
                Values = values
            };
        }

        /// <summary>
        /// Пробует распарсить файл с данными.
        /// </summary>
        /// <param name="file">Файл.</param>
        /// <returns>Список объектов <see cref="ValueEntry"/> полученных из файла.</returns>
        /// <exception cref="FormatException"></exception>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        private async Task<List<ValueEntry>> TryParseCsvFileAsync(IFormFile file)
        {
            var values = new List<ValueEntry>();
            var lines = new List<string>();
            var fileName = Path.GetFileNameWithoutExtension(file.FileName);

            using (var reader = new StreamReader(file.OpenReadStream()))
            {
                string line;
                await reader.ReadLineAsync();

                while ((line = await reader.ReadLineAsync()) != null)
                {
                    if (string.IsNullOrWhiteSpace(line)) continue;
                    lines.Add(line);
                }
            }

            if (lines.Count < 1 || lines.Count > 10000)
                throw new ArgumentOutOfRangeException($"Invalid row count: {lines.Count}. Must be between 1 and 10,000");

            foreach (var l in lines)
            {
                var parts = l.Split(';');
                if (parts.Length != 3)
                    throw new FormatException("Each row must contain exactly 3 values separated by semicolons");

                if (!DateTime.TryParseExact(
                    parts[0].Trim(),
                    DateFormat,
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal,
                    out var date))
                    throw new FormatException($"Invalid date format: {parts[0]}. Expected: yyyy-MM-ddTHH-mm-ss.ffffZ");

                if (!double.TryParse(parts[1].Trim(), NumberStyles.Any, CultureInfo.InvariantCulture, out var execTime))
                    throw new FormatException($"Invalid ExecutionTime: {parts[1]}");

                if (!double.TryParse(parts[2].Trim(), NumberStyles.Any, CultureInfo.InvariantCulture, out var value))
                    throw new FormatException($"Invalid Value: {parts[2]}");

                if (date < new DateTime(2000, 1, 1) || date > DateTime.UtcNow)
                    throw new ArgumentOutOfRangeException("Date", "Date must be between 01.01.2000 and now");

                if (execTime < 0 || value < 0)
                    throw new ArgumentOutOfRangeException("Value", "ExecutionTime and Value cannot be negative");

                values.Add(new ValueEntry
                {
                    Date = date,
                    ExecutionTime = execTime,
                    Value = value,
                    FileName = fileName
                });
            }

            return values;
        }
    }
}

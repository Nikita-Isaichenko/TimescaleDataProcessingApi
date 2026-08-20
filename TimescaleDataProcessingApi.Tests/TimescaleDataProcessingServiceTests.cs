using Microsoft.AspNetCore.Http;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using System.Text;
using TimescaleDataProcessingApi.Services;

namespace TimescaleDataProcessingApi.Tests
{

    public class TimescaleDataProcessingServiceTests
    {
        private TimescaleDataDbContext CreateContext()
        {
            var connection = new SqliteConnection("DataSource=:memory:");
            connection.Open();

            var options = new DbContextOptionsBuilder<TimescaleDataDbContext>()
                .UseSqlite(connection)
                .Options;

            var context = new TimescaleDataDbContext(options);

            context.Database.EnsureCreated();

            return context;
        }

        /// <summary>
        /// Проверяет корректность вычисления медианы.
        /// Подаются значения 10, 30, 20. Ожидается - 20.
        /// </summary>
        [Fact]
        public async Task ProcessCsv_CorrectMedian()
        {
            var context = CreateContext();
            var service = new TimescaleDataProcessingService(context);
            var csvContent = "Date;ExecutionTime;Value\n" +
                             "2024-01-01T10-00-00.0000Z;1.0;10.0\n" +
                             "2024-01-01T11-00-00.0000Z;1.0;30.0\n" +
                             "2024-01-01T12-00-00.0000Z;1.0;20.0";

            var file = CreateTestFile(csvContent);
            await service.ProcessCsvFile(file);


            var result = await context.Results.FirstOrDefaultAsync();
            Assert.NotNull(result);
            Assert.Equal(20.0, result.MedianValue);
        }

        /// <summary>
        /// Проверяет отработает ли исключение при негативном значение в данных.
        /// </summary>
        [Fact]
        public async Task ProcessCsv_ThrowExceptionWhenValueNegative()
        {
            var context = CreateContext();
            var service = new TimescaleDataProcessingService(context);
            var csvContent = "Date;ExecutionTime;Value\n" +
                     "2024-01-01T10-00-00.0000Z;1.0;-5.0";

            var file = CreateTestFile(csvContent);

            await Assert.ThrowsAsync<InvalidOperationException>(
                () => service.ProcessCsvFile(file)
            );

        }

        /// <summary>
        /// Проверяет, перезаписываются ли данные при одинаковом названии файла.
        /// </summary>
        [Fact]
        public async Task ProcessCsv_OverwriteData()
        {
            var context = CreateContext();
            var service = new TimescaleDataProcessingService(context);
            var fileName = "overwrite_test.csv";

            var csv1 = "Date;ExecutionTime;Value\n2024-01-01T10-00-00.0000Z;1.0;10.0";
            await service.ProcessCsvFile(CreateTestFile(csv1, fileName));


            var csv2 = "Date;ExecutionTime;Value\n2024-02-02T10-00-00.0000Z;2.0;20.0\n2024-02-03T10-00-00.0000Z;3.0;30.0";
            await service.ProcessCsvFile(CreateTestFile(csv2, fileName));

            var resultsCount = await context.Results.CountAsync();
            var valuesCount = await context.Values.CountAsync();


            Assert.Equal(1, resultsCount);
            Assert.Equal(2, valuesCount);
        }

        /// <summary>
        /// Создает тестовый файл.
        /// </summary>
        /// <param name="csvContent">Данные для файла.</param>
        /// <returns>Файл.</returns>
        private FormFile CreateTestFile(string csvContent, string fileName = "test.csv")
        {
            var stream = new MemoryStream(Encoding.UTF8.GetBytes(csvContent));
            var file = new FormFile(stream, 0, stream.Length, "file", fileName)
            {
                Headers = new HeaderDictionary(),
                ContentType = "text/csv"
            };

            return file;
        }
    }
}

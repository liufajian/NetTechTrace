using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.ComponentModel.DataAnnotations;

namespace EFCoreTest.MySQL
{
    internal class MyDbContext : DbContext
    {
        public DbSet<Config> Config { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseMySQL("server=localhost;port=20306;database=unit_test;user=roottest;password=YieldChain!@#$2020");
        }

        protected override void ConfigureConventions(ModelConfigurationBuilder builder)
        {
            builder.Properties<DateOnly>()
                .HaveConversion<DateOnlyConverter>()
                .HaveColumnType("date");

            builder.Properties<DateOnly?>()
                .HaveConversion<NullableDateOnlyConverter>()
                .HaveColumnType("date");
        }
    }

    class Config
    {
        public int id { get; set; }

        [Required]
        public string name { get; set; }

        public string value { get; set; }

        public double? value_double { get; set; }

        public DateOnly value_date { get; set; }
    }

    class MySqlLogFactory : ILoggerFactory
    {
        public void AddProvider(ILoggerProvider provider)
        {
            throw new NotImplementedException();
        }

        public ILogger CreateLogger(string categoryName)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }

    class MySqlLoggerProvider : ILoggerProvider
    {
        public ILogger CreateLogger(string categoryName)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }

    class MySqlLogger : ILogger
    {
        public IDisposable BeginScope<TState>(TState state) => default!;

        public bool IsEnabled(LogLevel logLevel)
        {
            return AppContext.TryGetSwitch("SetDebugSqlLog", out var isEnabled) && isEnabled && logLevel > LogLevel.Trace;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            System.Diagnostics.Debug.WriteLine($"[{logLevel}]");
        }
    }
}

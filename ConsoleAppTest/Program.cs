// See https://aka.ms/new-console-template for more information
using Microsoft.Extensions.Configuration;

Console.WriteLine("Hello, World!");

// Build a config object, using env vars and JSON providers.
IConfiguration config = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json")
    .AddEnvironmentVariables()
    .Build();

// Get values from the config given their key and their target type.
Settings settings = config.GetRequiredSection("Settings").Get<Settings>();

// Write the values to the console.
Console.WriteLine($"KeyOne = {settings.KeyOne}");
Console.WriteLine($"KeyTwo = {settings.KeyTwo}");
Console.WriteLine($"KeyThree:Message = {settings.KeyThree.Message}");

var ServiceName = config.GetValue<string>("ServiceName");
Console.WriteLine($"ServiceName = {ServiceName}");

var logger = NLog.LogManager.GetLogger("测试LOG");

logger.Info("hello");

logger.Debug("no no no");
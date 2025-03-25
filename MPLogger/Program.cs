using MongoDB.Driver;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

// Configuração do ambiente --> Injetando o AppSettings.json a partir do Environment
var configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json")
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName ?? ""}.json", optional: true)
    .AddEnvironmentVariables()
    .Build();

builder.Services.AddOptions<MongoConfig>()
    .Bind(configuration.GetSection("MongoDb"));

Console.WriteLine($"Environment started: {builder.Environment.EnvironmentName}");

builder.Services.AddHostedService<RabbitMqConsumer>();

builder.Services.AddSingleton<IMongoClient>(serviceProvider =>
{
    var mongoConfig = serviceProvider.GetRequiredService<IOptions<MongoConfig>>().Value;
    return new MongoClient(mongoConfig.ConnectionString);
});

builder.Services.AddSingleton(serviceProvider =>
{
    var client = serviceProvider.GetRequiredService<IMongoClient>();
    var database = client.GetDatabase(serviceProvider.GetRequiredService<IOptions<MongoConfig>>().Value.DatabaseName);
    return database.GetCollection<LogEntry>("logs");
});

var app = builder.Build();

app.Run();
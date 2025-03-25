using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using MongoDB.Driver;
using Microsoft.Extensions.Options;

public class RabbitMqConsumer : BackgroundService
{
    private readonly IConfiguration _configuration;
    private readonly IMongoCollection<LogEntry> _logCollection;
    private readonly MongoConfig _mongoConfig;

    public RabbitMqConsumer(IConfiguration configuration, IMongoCollection<LogEntry> logCollection, IOptions<MongoConfig> mongoConfig)
    {
        _configuration = configuration;
        _logCollection = logCollection;
        _mongoConfig = mongoConfig.Value;
        
        var client = new MongoClient(_mongoConfig.ConnectionString);
        var database = client.GetDatabase(_mongoConfig.DatabaseName);
        _logCollection = database.GetCollection<LogEntry>("logs");
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var rabbitMqServer = _configuration.GetConnectionString("RabbitMQServer") ?? "localhost";
        var factory = new ConnectionFactory() { HostName = rabbitMqServer };

        // Conectando de forma assíncrona
        var connection = await factory.CreateConnectionAsync();
        var channel = await connection.CreateChannelAsync();

        // Declarando a fila que ele ficará lendo sem parar
        await channel.QueueDeclareAsync(queue: "logQueue", durable: false, exclusive: false, autoDelete: false, arguments: null);

        var consumer = new AsyncEventingBasicConsumer(channel);

        // Usando o consumidor para processar mensagens
        consumer.ReceivedAsync += async (sender, ea) =>
        {
            var body = ea.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);
        
            try
            {
                var log = new LogEntry(message, "logQueue");

                await _logCollection.InsertOneAsync(log);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao validar a mensagem: {ex.Message}");
                return;
            }

            // Console.WriteLine($"Mensagem recebida: {log.ToJson()}");
            Console.WriteLine($"Mensagem salva: {message}");
        };

        // Pegando a mensagem consumindo e descartando ela
        await channel.BasicConsumeAsync(queue: "logQueue", autoAck: true, consumer: consumer);

        // Esperando indefinidamente até que o token de cancelamento seja acionado
        await Task.Delay(Timeout.Infinite, stoppingToken);
    }
}

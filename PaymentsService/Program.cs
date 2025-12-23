using Confluent.Kafka;
using Microsoft.EntityFrameworkCore;
using PaymentsService.Common.ProcessOrder;
using PaymentsService.Common.SendPaymentResult;
using PaymentsService.Database;
using PaymentsService.UseCases.CreateAccount;
using PaymentsService.UseCases.DepositFunds;
using PaymentsService.UseCases.GetBalance;
using PaymentsService.UseCases.ProcessPayment;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

Console.WriteLine("=== Starting Payments Service ===");

// Add services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Database
Console.WriteLine("Configuring database...");
builder.Services.AddDbContext<PaymentContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("Default")));

// Background services - ВРЕМЕННО отключаем миграции
// builder.Services.AddHostedService<MigrationRunner>();
builder.Services.AddHostedService<MigrationRunner>();

// Kafka Producer
Console.WriteLine("Configuring Kafka Producer...");
try
{
    _ = builder.Services.AddSingleton<IProducer<string, string>>(sp =>
    {
        Console.WriteLine("Creating Kafka Producer...");
        ProducerConfig config = new()
        {
            BootstrapServers = builder.Configuration["Kafka:BootstrapServers"],
            Acks = Acks.All,
            EnableIdempotence = true
        };
        return new ProducerBuilder<string, string>(config).Build();
    });
    Console.WriteLine("Kafka Producer registered successfully");
}
catch (Exception ex)
{
    Console.WriteLine($"ERROR registering Kafka Producer: {ex.Message}");
    throw;
}

// Kafka Consumer
Console.WriteLine("Configuring Kafka Consumer...");
try
{
    _ = builder.Services.AddSingleton<IConsumer<string, string>>(sp =>
    {
        Console.WriteLine("Creating Kafka Consumer...");
        ConsumerConfig config = new()
        {
            BootstrapServers = builder.Configuration["Kafka:BootstrapServers"],
            GroupId = "payments-service-group",
            AutoOffsetReset = AutoOffsetReset.Earliest,
            EnableAutoCommit = false,
            EnableAutoOffsetStore = false
        };
        return new ConsumerBuilder<string, string>(config).Build();
    });
    Console.WriteLine("Kafka Consumer registered successfully");
}
catch (Exception ex)
{
    Console.WriteLine($"ERROR registering Kafka Consumer: {ex.Message}");
    throw;
}

// Services
Console.WriteLine("Registering services...");
builder.Services.AddScoped<ICreateAccountService, CreateAccountService>();
builder.Services.AddScoped<IDepositFundsService, DepositFundsService>();
builder.Services.AddScoped<IGetBalanceService, GetBalanceService>();
builder.Services.AddScoped<IProcessPaymentService, ProcessPaymentService>();
builder.Services.AddScoped<ISendPaymentResultService, SendPaymentResultService>();

// Background services with debugging
Console.WriteLine("Registering hosted services...");

// 1. OrderConsumer
try
{
    Console.WriteLine("Registering OrderConsumer...");
    _ = builder.Services.AddHostedService<OrderConsumer>(sp =>
    {
        Console.WriteLine("Creating OrderConsumer instance...");
        IConsumer<string, string> consumer = sp.GetRequiredService<IConsumer<string, string>>();
        ILogger<OrderConsumer> logger = sp.GetRequiredService<ILogger<OrderConsumer>>();
        IServiceScopeFactory scopeFactory = sp.GetRequiredService<IServiceScopeFactory>();
        string topic = builder.Configuration["Kafka:Topic:OrderCreated"] ?? "order-created";

        Console.WriteLine($"OrderConsumer created with topic: {topic}");
        return new OrderConsumer(scopeFactory, logger, consumer, topic);
    });
    Console.WriteLine("OrderConsumer registered successfully");
}
catch (Exception ex)
{
    Console.WriteLine($"ERROR registering OrderConsumer: {ex.Message}");
    Console.WriteLine($"Stack trace: {ex.StackTrace}");
    throw;
}

// 2. OrderProcessor - ИСПРАВЛЕНО: теперь 2 аргумента вместо 3
try
{
    Console.WriteLine("Registering OrderProcessor...");
    _ = builder.Services.AddHostedService<OrderProcessor>(sp =>
    {
        Console.WriteLine("Creating OrderProcessor instance...");
        IServiceScopeFactory scopeFactory = sp.GetRequiredService<IServiceScopeFactory>();
        ILogger<OrderProcessor> logger = sp.GetRequiredService<ILogger<OrderProcessor>>();

        // Только 2 аргумента: scopeFactory и logger
        return new OrderProcessor(scopeFactory, logger);
    });
    Console.WriteLine("OrderProcessor registered successfully");
}
catch (Exception ex)
{
    Console.WriteLine($"ERROR registering OrderProcessor: {ex.Message}");
    Console.WriteLine($"Stack trace: {ex.StackTrace}");
    throw;
}

// 3. PaymentResultSender
try
{
    Console.WriteLine("Registering PaymentResultSender...");
    _ = builder.Services.AddHostedService<PaymentResultSender>(sp =>
    {
        Console.WriteLine("Creating PaymentResultSender instance...");
        IProducer<string, string> producer = sp.GetRequiredService<IProducer<string, string>>();
        ILogger<PaymentResultSender> logger = sp.GetRequiredService<ILogger<PaymentResultSender>>();
        IServiceScopeFactory scopeFactory = sp.GetRequiredService<IServiceScopeFactory>();

        Dictionary<string, string> topics = new()
        {
            ["PaymentProcessed"] = builder.Configuration["Kafka:Topic:PaymentProcessed"] ?? "payment-processed",
            ["PaymentFailed"] = builder.Configuration["Kafka:Topic:PaymentFailed"] ?? "payment-failed"
        };

        Console.WriteLine($"PaymentResultSender created with topics: {string.Join(", ", topics.Keys)}");
        return new PaymentResultSender(scopeFactory, logger, producer, topics);
    });
    Console.WriteLine("PaymentResultSender registered successfully");
}
catch (Exception ex)
{
    Console.WriteLine($"ERROR registering PaymentResultSender: {ex.Message}");
    Console.WriteLine($"Stack trace: {ex.StackTrace}");
    throw;
}

Console.WriteLine("All services registered successfully");

WebApplication app = builder.Build();

// Configure pipeline
if (app.Environment.IsDevelopment())
{
    _ = app.UseSwagger();
    _ = app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

Console.WriteLine("Payments Service starting...");
app.Run();

// Временный класс для отключения миграций
public class NoOpMigrationRunner(ILogger<NoOpMigrationRunner> logger) : IHostedService
{
    private readonly ILogger<NoOpMigrationRunner> _logger = logger;

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Migrations temporarily disabled for testing");
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
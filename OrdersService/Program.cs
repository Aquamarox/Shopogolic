using Confluent.Kafka;
using Microsoft.EntityFrameworkCore;
using OrdersService.Common;
using OrdersService.Common.SendOrderEvent;
using OrdersService.Database;
using OrdersService.UseCases.CreateOrder;
using OrdersService.UseCases.GetOrders;
using OrdersService.UseCases.GetOrderStatus;
using Microsoft.AspNetCore.SignalR; // Убедитесь, что этот using есть

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// --- 1. SignalR ---
builder.Services.AddSignalR();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// --- 2. Database ---
builder.Services.AddDbContext<OrderContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("Default")));

builder.Services.AddHostedService<MigrationRunner>();

// --- 3. Kafka Producer ---
builder.Services.AddSingleton<IProducer<string, string>>(sp =>
{
    ProducerConfig config = new()
    {
        BootstrapServers = builder.Configuration["Kafka:BootstrapServers"],
        Acks = Acks.All,
        EnableIdempotence = true
    };
    return new ProducerBuilder<string, string>(config).Build();
});

// --- 4. Kafka Consumer ---
builder.Services.AddSingleton<IConsumer<string, string>>(sp =>
{
    ConsumerConfig config = new()
    {
        BootstrapServers = builder.Configuration["Kafka:BootstrapServers"],
        GroupId = "orders-service-payments-group",
        AutoOffsetReset = AutoOffsetReset.Earliest,
        EnableAutoCommit = false,
        EnableAutoOffsetStore = false
    };
    return new ConsumerBuilder<string, string>(config).Build();
});

// --- 5. PaymentResultConsumer (ИСПРАВЛЕНО) ---
builder.Services.AddHostedService<PaymentResultConsumer>(sp =>
{
    IConsumer<string, string> consumer = sp.GetRequiredService<IConsumer<string, string>>();
    ILogger<PaymentResultConsumer> logger = sp.GetRequiredService<ILogger<PaymentResultConsumer>>();
    IServiceScopeFactory scopeFactory = sp.GetRequiredService<IServiceScopeFactory>();
    IConfiguration configuration = sp.GetRequiredService<IConfiguration>();
    IHubContext<OrderHub> hubContext = sp.GetRequiredService<IHubContext<OrderHub>>(); // ДОБАВЛЕНО

    // Передаем все 5 аргументов
    return new PaymentResultConsumer(scopeFactory, logger, consumer, configuration, hubContext);
});

// --- 6. Services ---
builder.Services.AddScoped<ISendOrderEventService, SendOrderEventService>();
builder.Services.AddScoped<ICreateOrderService, CreateOrderService>();
builder.Services.AddScoped<IGetOrdersService, GetOrdersService>();
builder.Services.AddScoped<IGetOrderStatusService, GetOrderStatusService>();

// --- 7. OrderEventSender ---
builder.Services.AddHostedService<OrderEventSender>(sp =>
{
    IProducer<string, string> producer = sp.GetRequiredService<IProducer<string, string>>();
    ILogger<OrderEventSender> logger = sp.GetRequiredService<ILogger<OrderEventSender>>();
    IServiceScopeFactory scopeFactory = sp.GetRequiredService<IServiceScopeFactory>();
    string topic = builder.Configuration["Kafka:Topic:OrderCreated"] ?? "order-created";

    return new OrderEventSender(scopeFactory, logger, producer, topic);
});

WebApplication app = builder.Build();

// --- 8. Pipeline ---
if (app.Environment.IsDevelopment())
{
    _ = app.UseSwagger();
    _ = app.UseSwaggerUI();
}

// Порядок важен: StaticFiles -> MapHub -> MapControllers
app.UseStaticFiles();

app.UseHttpsRedirection();
app.UseAuthorization();

app.MapControllers();
app.MapHub<OrderHub>("/orderHub"); // Регистрация маршрута WebSocket

app.Run();
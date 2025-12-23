using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace PaymentsService.Database
{
    /// <summary>
    /// Фоновый сервис, отвечающий за автоматическое применение миграций к базе данных платежей.
    /// Содержит логику ожидания готовности БД и повторных попыток подключения.
    /// </summary>
    public class MigrationRunner(IServiceProvider serviceProvider, ILogger<MigrationRunner> logger) : IHostedService
    {
        private readonly IServiceProvider _serviceProvider = serviceProvider;
        private readonly ILogger<MigrationRunner> _logger = logger;
        private static readonly ManualResetEventSlim _migrationsCompleted = new(false);
        private static bool _migrationsFailed = false;
        private static string? _failureReason;

        /// <summary>
        /// Позволяет другим сервисам (например, потребителям Kafka) дождаться успешного завершения миграций 
        /// перед началом работы, чтобы избежать ошибок отсутствия таблиц.
        /// </summary>
        /// <param name="timeout">Максимальное время ожидания.</param>
        /// <param name="logger">Экземпляр логгера для записи состояния.</param>
        /// <returns>True, если миграции успешно применены; иначе false.</returns>
        public static bool WaitForMigrations(TimeSpan timeout, ILogger logger)
        {
            logger.LogInformation("Waiting for migrations to complete (timeout: {Timeout}s)...", timeout.TotalSeconds);

            bool completed = _migrationsCompleted.Wait(timeout);

            if (completed && !_migrationsFailed)
            {
                logger.LogInformation("Migrations are ready, proceeding...");
                return true;
            }
            else if (_migrationsFailed)
            {
                logger.LogError("Migrations failed: {Reason}", _failureReason);
                return false;
            }
            else
            {
                logger.LogError("Timeout waiting for migrations");
                return false;
            }
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Starting migrations for PaymentsService...");

            int maxRetries = 15; // Увеличим количество попыток
            int retryDelaySeconds = 3;

            for (int attempt = 1; attempt <= maxRetries; attempt++)
            {
                try
                {
                    using IServiceScope scope = _serviceProvider.CreateScope();
                    PaymentContext context = scope.ServiceProvider.GetRequiredService<PaymentContext>();

                    _logger.LogInformation("Migration attempt {Attempt}/{MaxRetries}", attempt, maxRetries);

                    // Проверяем, доступна ли база данных
                    if (await context.Database.CanConnectAsync(cancellationToken))
                    {
                        _logger.LogInformation("Database connection successful, applying migrations...");
                        await context.Database.MigrateAsync(cancellationToken);

                        _logger.LogInformation("Migrations applied successfully!");
                        _migrationsCompleted.Set();
                        return;
                    }
                    else
                    {
                        _logger.LogWarning("Database not yet available, retrying...");
                    }
                }
                catch (Exception ex) when (attempt < maxRetries)
                {
                    _logger.LogWarning(ex, "Migration attempt {Attempt} failed. Retrying in {Delay}s...",
                        attempt, retryDelaySeconds);

                    await Task.Delay(TimeSpan.FromSeconds(retryDelaySeconds), cancellationToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "All migration attempts failed after {MaxRetries} retries", maxRetries);
                    _failureReason = ex.Message;
                    _migrationsFailed = true;
                    _migrationsCompleted.Set(); // Все равно сигнализируем, чтобы не зависнуть
                    throw;
                }
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("MigrationRunner stopping");
            return Task.CompletedTask;
        }
    }
}
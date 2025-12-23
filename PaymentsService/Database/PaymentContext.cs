using Microsoft.EntityFrameworkCore;
using PaymentsService.Models;

namespace PaymentsService.Database
{
    /// <summary>
    /// Контекст базы данных Entity Framework для сервиса платежей.
    /// Управляет таблицами счетов (Accounts), транзакций (Transactions) и очередей сообщений (Inbox/Outbox).
    /// </summary>
    public class PaymentContext(DbContextOptions<PaymentContext> options) : DbContext(options)
    {
        public DbSet<Account> Accounts { get; set; }
        public DbSet<Transaction> Transactions { get; set; }
        public DbSet<InboxMessage> InboxMessages { get; set; }
        public DbSet<OutboxMessage> OutboxMessages { get; set; }

        /// <summary>
        /// Настройка конфигурации моделей и связей в базе данных при её создании.
        /// Включает описание ограничений, индексов для идемпотентности и типов колонок.
        /// </summary>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            _ = modelBuilder.Entity<Account>(entity =>
            {
                _ = entity.HasKey(e => e.Id);
                _ = entity.Property(e => e.Id).ValueGeneratedOnAdd();
                _ = entity.Property(e => e.UserId).IsRequired();
                _ = entity.Property(e => e.Balance).IsRequired().HasColumnType("decimal(18,2)");
                _ = entity.Property(e => e.HeldAmount).IsRequired().HasColumnType("decimal(18,2)");
                _ = entity.Property(e => e.Version).IsRequired();
                _ = entity.Property(e => e.CreatedAt).IsRequired();
                _ = entity.Property(e => e.UpdatedAt).IsRequired();
                _ = entity.HasIndex(e => e.UserId).IsUnique();
            });

            _ = modelBuilder.Entity<Transaction>(entity =>
            {
                _ = entity.HasKey(e => e.Id);
                _ = entity.Property(e => e.Id).ValueGeneratedOnAdd();
                _ = entity.Property(e => e.OrderId).IsRequired();
                _ = entity.Property(e => e.UserId).IsRequired();
                _ = entity.Property(e => e.Amount).IsRequired().HasColumnType("decimal(18,2)");
                _ = entity.Property(e => e.Type).IsRequired();
                _ = entity.Property(e => e.Status).IsRequired();
                _ = entity.Property(e => e.CreatedAt).IsRequired();
                _ = entity.HasIndex(e => e.OrderId).IsUnique(); // Идемпотентность по OrderId
            });

            _ = modelBuilder.Entity<InboxMessage>(entity =>
            {
                _ = entity.HasKey(e => e.Id);
                _ = entity.Property(e => e.Id).ValueGeneratedOnAdd();
                _ = entity.Property(e => e.MessageId).IsRequired().HasMaxLength(200);
                _ = entity.Property(e => e.EventType).IsRequired().HasMaxLength(100);
                _ = entity.Property(e => e.Payload).IsRequired();
                _ = entity.Property(e => e.IsProcessed).IsRequired();
                _ = entity.Property(e => e.ReceivedAt).IsRequired();
                _ = entity.HasIndex(e => e.MessageId).IsUnique(); // Уникальный ключ для идемпотентности
                _ = entity.HasIndex(e => new { e.IsProcessed, e.ReceivedAt });
            });

            _ = _ = modelBuilder.Entity<OutboxMessage>(entity =>
            {
                _ = entity.HasKey(e => e.Id);
                _ = entity.Property(e => e.Id).ValueGeneratedOnAdd();
                _ = entity.Property(e => e.EventType).IsRequired().HasMaxLength(100);
                _ = entity.Property(e => e.Payload).IsRequired();
                _ = entity.Property(e => e.IsSent).IsRequired();
                _ = entity.Property(e => e.CreatedAt).IsRequired();
                _ = entity.HasIndex(e => new { e.IsSent, e.CreatedAt });
            });
        }
    }

}

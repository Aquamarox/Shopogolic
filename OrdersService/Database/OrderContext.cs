using Microsoft.EntityFrameworkCore;
using OrdersService.Models;

namespace OrdersService.Database
{
    public class OrderContext(DbContextOptions<OrderContext> options) : DbContext(options)
    {
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<OutboxMessage> OutboxMessages { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            _ = modelBuilder.Entity<Order>(entity =>
            {
                _ = entity.HasKey(e => e.Id);
                _ = entity.Property(e => e.Id).ValueGeneratedOnAdd();
                _ = entity.Property(e => e.UserId).IsRequired();
                _ = entity.Property(e => e.TotalAmount).IsRequired().HasColumnType("decimal(18,2)");
                _ = entity.Property(e => e.Status).IsRequired();
                _ = entity.Property(e => e.CreatedAt).IsRequired();
                _ = entity.HasMany(e => e.Items).WithOne().HasForeignKey(i => i.OrderId);
            });

            _ = modelBuilder.Entity<OrderItem>(entity =>
            {
                _ = entity.HasKey(e => e.Id);
                _ = entity.Property(e => e.Id).ValueGeneratedOnAdd();
                _ = entity.Property(e => e.ProductId).IsRequired();
                _ = entity.Property(e => e.ProductName).IsRequired().HasMaxLength(200);
                _ = entity.Property(e => e.Quantity).IsRequired();
                _ = entity.Property(e => e.Price).IsRequired().HasColumnType("decimal(18,2)");
            });

            _ = modelBuilder.Entity<OutboxMessage>(entity =>
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

using System.Text.Json;
using Confluent.Kafka;
using Contracts;
using Microsoft.EntityFrameworkCore;
using OrderWorker.Data;
using StackExchange.Redis;
using Order = Domain.Entities.Order;

namespace OrderWorker.Services
{
    public class KafkaConsumerService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IConnectionMultiplexer _redis;

        public KafkaConsumerService(
            IServiceScopeFactory scopeFactory,
            IConnectionMultiplexer redis)
        {
            _scopeFactory = scopeFactory;
            _redis = redis;
        }


        protected override async Task ExecuteAsync(
            CancellationToken stoppingToken)
        {
            var config = new ConsumerConfig
            {
                BootstrapServers = "localhost:29092",

                GroupId = "order-workers",

                AutoOffsetReset =
                    AutoOffsetReset.Earliest
            };

            using var consumer =
                new ConsumerBuilder<string, string>(config)
                .Build();

            consumer.Subscribe("car-orders");

            while (!stoppingToken.IsCancellationRequested)
            {
                var result =
                    consumer.Consume(stoppingToken);

                var request =
                    JsonSerializer.Deserialize<OrderRequest>(
                        result.Message.Value);

                await ProcessOrder(request!);
                consumer.Commit(result);
            }

        }

        private async Task ProcessOrder(
            OrderRequest request)
        {

            var redis =
                _redis.GetDatabase();

            // کم کردن ظرفیت
            var remaining =
                await redis.StringDecrementAsync(
                    $"sale:{request.SaleId}:remaining");

            if (remaining < 0)
            {
                Console.WriteLine(
                    "Sold out");

                return;
            }

            using var scope =
                _scopeFactory.CreateScope();

            var db =
                scope.ServiceProvider
                .GetRequiredService<AppDbContext>();

            var sale =
                await db.CarSales
                .FirstAsync(x =>
                    x.Id == request.SaleId);

            sale.SoldCount++;

            db.Orders.Add(new Order
            {
                Id = Guid.NewGuid(),
                UserId = request.UserId,
                SaleId = request.SaleId,
                CreatedAt = DateTime.UtcNow
            });

            await db.SaveChangesAsync();

            Console.WriteLine(
                $"Order saved {request.UserId}");

        }
    }
}

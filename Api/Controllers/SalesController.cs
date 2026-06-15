using Confluent.Kafka;
using Contracts;
using Microsoft.AspNetCore.Mvc;
using StackExchange.Redis;
using System.Text.Json;

namespace CarSaleDemo.Api.Controllers;

[ApiController]
[Route("sales")]
public class SalesController : ControllerBase
{
    private readonly IConnectionMultiplexer _redis;
    private readonly IProducer<string, string> _producer;

    public SalesController(
        IConnectionMultiplexer redis,
        IProducer<string, string> producer)
    {
        _redis = redis;
        _producer = producer;
    }

    [HttpPost("{saleId:guid}/buy")]
    public async Task<IActionResult> Buy(Guid saleId)
    {
        var userId = Request.Headers["X-UserId"].FirstOrDefault();

        if (string.IsNullOrWhiteSpace(userId))
            return BadRequest("UserId is required.");

        var db = _redis.GetDatabase();

        var rateLimitKey = $"rate:{userId}";
        var currentCount = await db.StringIncrementAsync(rateLimitKey);

        if (currentCount == 1)
        {
            await db.KeyExpireAsync(rateLimitKey, TimeSpan.FromSeconds(1));
        }

        if (currentCount > 5)
        {
            return StatusCode(
                StatusCodes.Status429TooManyRequests,
                "Too many requests");
        }

        var orderRequest = new OrderRequest
        {
            UserId = Guid.Parse(userId),
            SaleId = saleId
        };

        var payload = JsonSerializer.Serialize(orderRequest);

        await _producer.ProduceAsync(
            "car-orders",
            new Message<string, string>
            {
                Key = saleId.ToString(),
                Value = payload
            });

        return Accepted(new
        {
            Message = "Order queued"
        });
    }
}


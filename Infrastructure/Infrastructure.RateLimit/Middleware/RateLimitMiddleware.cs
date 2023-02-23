using System.Net;
using Infrastructure.RateLimit.Configurations;
using Infrastructure.RateLimit.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace Infrastructure.RateLimit.Middleware;

public class RateLimitMiddleware : IMiddleware
{
    private readonly ILogger<RateLimitMiddleware> _logger;
    private readonly IDatabase _redis;
    private readonly int _limit;
    private readonly TimeSpan _timeFrame;

    public RateLimitMiddleware(
        ILogger<RateLimitMiddleware> logger,
        IRedisCacheConnectionService cacheConnection,
        IOptions<RedisConfig> options)
    {
        _logger = logger;
        _redis = cacheConnection.Connection.GetDatabase();
        _limit = options.Value.RequestsLimit;
        _timeFrame = options.Value.RequestsTimeout;
    }

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        var ipAddress = context.Connection.RemoteIpAddress?.ToString();

        if (ipAddress != null)
        {
            var endpoint = context.Request.Path.ToString();

            var key = $"{ipAddress}:{endpoint}";

            var count = await _redis.StringIncrementAsync(key);

            _logger.LogInformation($"Filter works: key -> {key}, value -> {count}");

            if (count == 1)
            {
                // If this is the first request for this key, set its expiration time.
                await _redis.KeyExpireAsync(key, _timeFrame);
            }

            if (count > _limit)
            {
                _logger.LogWarning($"Rate limit exceeded for {key}");

                context.Response.StatusCode = (int)HttpStatusCode.TooManyRequests;
                await context.Response.WriteAsync("Rate limit exceeded. Try again later.");
                return;
            }
        }

        await next(context);
    }
}

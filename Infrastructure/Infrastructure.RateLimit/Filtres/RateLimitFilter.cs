using System.Net;
using Infrastructure.RateLimit.Configurations;
using Infrastructure.RateLimit.Services.Interfaces;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace Infrastructure.RateLimit.Filtres;

public class RateLimitFilter : IAsyncActionFilter
{
    private readonly ILogger<RateLimitFilter> _logger;
    private readonly IDatabase _redis;
    private readonly int _limit;
    private readonly TimeSpan _timeFrame;

    public RateLimitFilter(
        ILogger<RateLimitFilter> logger,
        IRedisCacheConnectionService cacheConnection,
        IOptions<RedisConfig> options)
    {
        _logger = logger;
        _redis = cacheConnection.Connection.GetDatabase();
        _limit = options.Value.RequestsLimit;
        _timeFrame = options.Value.RequestsTimeout;
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var ipAddress = context.HttpContext.Connection.RemoteIpAddress?.ToString();

        if (ipAddress != null)
        {
            var endpoint = context.HttpContext.Request.Path.ToString();

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

                context.HttpContext.Response.StatusCode = (int)HttpStatusCode.TooManyRequests;
                return;
            }
        }

        await next();
    }
}

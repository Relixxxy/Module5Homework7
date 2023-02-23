using StackExchange.Redis;

namespace Infrastructure.RateLimit.Services.Interfaces;

public interface IRedisCacheConnectionService
{
    public IConnectionMultiplexer Connection { get; }
}

namespace Infrastructure.RateLimit.Configurations;

public class RedisConfig
{
    public string Host { get; set; } = null!;
    public TimeSpan CacheTimeout { get; set; }
    public TimeSpan RequestsTimeout { get; set; }
    public int RequestsLimit { get; set; }
}

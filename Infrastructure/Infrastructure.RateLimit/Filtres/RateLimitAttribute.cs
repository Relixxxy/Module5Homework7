using Microsoft.AspNetCore.Mvc;

namespace Infrastructure.RateLimit.Filtres;

[AttributeUsage(AttributeTargets.Method)]
public class RateLimitAttribute : TypeFilterAttribute
{
    public RateLimitAttribute(int limit, int timeFrameSeconds)
        : base(typeof(RateLimitFilter))
    {
        Arguments = new object[] { limit, TimeSpan.FromSeconds(timeFrameSeconds) };
    }
}
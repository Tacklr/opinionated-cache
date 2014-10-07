using OpinionatedCache.Policy;

namespace OpinionatedCache.API
{
    public interface IBaseCacheKey
    {
        string Key { get; }
        string PolicyKey { get; }
        CachePolicy DefaultPolicy { get; }
        CachePolicy Policy { get; }
    }
}

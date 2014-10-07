namespace OpinionatedCache.API
{
    public interface IBaseCacheKey
    {
        string Key { get; }
        string PolicyKey { get; }
        ICachePolicy DefaultPolicy { get; }
        ICachePolicy Policy { get; }
    }
}

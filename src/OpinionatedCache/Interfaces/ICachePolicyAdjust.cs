using OpinionatedCache.Policy;
namespace OpinionatedCache.API
{
    public interface ICachePolicyAdjust
    {
        CachePolicy Adjust(CachePolicy policy);
    }
}

using OpinionatedCache.Policy;
namespace OpinionatedCache.API
{
    public interface ICachePolicyRepository
    {
        CachePolicy ComputePolicy(string policyKey, CachePolicy defaultPolicy);
    }
}

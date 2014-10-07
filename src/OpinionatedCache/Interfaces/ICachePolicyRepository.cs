namespace OpinionatedCache.API
{
    public interface ICachePolicyRepository
    {
        ICachePolicy ComputePolicy(string policyKey, ICachePolicy defaultPolicy);
    }
}

using OpinionatedCache.API;

namespace OpinionatedCache.Policy
{
    public class DefaultCachePolicyRepository : ICachePolicyRepository
    {
        public CachePolicy ComputePolicy(string policyKey, CachePolicy defaultPolicy)
        {
            // do nothing, it's easy
            return UnchangedPolicyAdjust.Instance.Adjust(defaultPolicy);
        }
    }

    public class UnchangedPolicyAdjust : ICachePolicyAdjust
    {
        private static ICachePolicyAdjust s_Instance = new UnchangedPolicyAdjust();

        public static ICachePolicyAdjust Instance { get { return s_Instance; } }

        public CachePolicy Adjust(CachePolicy policy)
        {
            return policy;
        }
    }
}

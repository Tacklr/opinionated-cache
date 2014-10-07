﻿using OpinionatedCache.API;

namespace OpinionatedCache.Policy
{
    public class DefaultCachePolicyRepository : ICachePolicyRepository
    {
        public static readonly ICachePolicyRepository Instance = new DefaultCachePolicyRepository();

        public ICachePolicy ComputePolicy(string policyKey, ICachePolicy defaultPolicy)
        {
            // do nothing, it's easy
            return UnchangedPolicyAdjust.Instance.Adjust(defaultPolicy);
        }
    }

    public class UnchangedPolicyAdjust : ICachePolicyAdjust
    {
        public static readonly ICachePolicyAdjust Instance = new UnchangedPolicyAdjust();

        public ICachePolicy Adjust(ICachePolicy policy)
        {
            return policy;
        }
    }
}

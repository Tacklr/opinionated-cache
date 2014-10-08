﻿// Licensed under the MIT License. See LICENSE.md in the project root for more information.

using OpinionatedCache.API;

namespace OpinionatedCache.Policy
{
    public class DefaultCachePolicyRepository : ICachePolicyRepository
    {
        public static readonly ICachePolicyRepository Instance = new DefaultCachePolicyRepository();

        public ICachePolicy DefaultPolicy()
        {
            return new DefaultCachePolicy { AbsoluteSeconds = 10 };    // every 10 seconds should pepper the backing store quite nicely
        }

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

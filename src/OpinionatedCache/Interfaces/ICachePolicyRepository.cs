﻿// Licensed under the MIT License. See LICENSE.md in the project root for more information.

namespace OpinionatedCache.API
{
    public interface ICachePolicyRepository
    {
        ICachePolicy DefaultPolicy();
        ICachePolicy ComputePolicy(string policyKey, ICachePolicy defaultPolicy);
    }
}

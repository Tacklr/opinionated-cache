﻿// Licensed under the MIT License. See LICENSE.md in the project root for more information.

namespace OpinionatedCache.API
{
    public interface IBaseCacheKey
    {
        string Prefix { get; set; }
        string SubKey { get; set; }
        string Key { get; }
        string PolicyKey { get; }
        ICachePolicy DefaultPolicy { get; }
        ICachePolicy Policy { get; }

        string BuildKey();
        string BuildKey(params string[] vals);
        string BuildPolicyKey();
        string BuildPolicyKey(string[] vals);
        /*static*/ ICachePolicyRepository PolicyRepository { get; }
    }
}

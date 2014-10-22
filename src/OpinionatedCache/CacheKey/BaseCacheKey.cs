﻿// Licensed under the MIT License. See LICENSE.md in the project root for more information.

using System;
using OpinionatedCache.Policy;

namespace OpinionatedCache.API.CacheKey
{
    public abstract class BaseCacheKey : IBaseCacheKey
    {
        public static ICachePolicyRepository PolicyRepository { get; set; }

        static BaseCacheKey()
        {
            PolicyRepository = DefaultCachePolicyRepository.Instance;   // allow someone to override the policy management, but the default does nothing.
        }

        public string Prefix { get; set; }
        public string SubKey { get; set; }

        public BaseCacheKey(string prefix)
        {
            Prefix = prefix;
        }

        public virtual string Key
        {
            get
            {
                return BuildKey();
            }
        }

        public virtual string PolicyKey
        {
            get
            {
                return BuildPolicyKey();    // by default...
            }
        }

        public virtual ICachePolicy DefaultPolicy
        {
            get
            {
                return PolicyRepository.DefaultPolicy();
            }
        }

        public virtual ICachePolicy Policy
        {
            get
            {
                // lookup in the config the policy for the official key-and-parameters given
                var policyKey = PolicyKey;
                var defaultPolicy = DefaultPolicy;
                return PolicyRepository.ComputePolicy(policyKey, defaultPolicy); // lookup the policy via the provider
            }
        }

        // some helper methods for derived classes to use

        /// <summary>
        /// Generates a clone of the provider's DefaultPolicy and sets policy to the requested options
        /// </summary>
        /// <param name="absoluteSeconds">Number of seconds that cached items should be retained or <see cref="ICachePolicyOptions.Unused"/> if no absolute timeout eviction.</param>
        /// <param name="slidingSeconds">Number of seconds that cached items should be retained after each access or <see cref="ICachePolicyOptions.Unused"/> if no sliding timeout eviction.</param>
        /// <param name="RefillCount">Number of times the cached item should be reloaded automatically after it expires. Defaults to zero automatic refills.</param>
        /// <returns>an <see cref="ICachePolicy"/> policy that can be modified</returns>
        /// <remarks>The returned policy can be modified if need. Tis </remarks>
        protected static ICachePolicy BuildDefaultPolicy(int absoluteSeconds = ICachePolicyOptions.Unused, int slidingSeconds = ICachePolicyOptions.Unused, int refillCount = 0)
        {
            var defaultPolicy = PolicyRepository.DefaultPolicy().Clone();
            defaultPolicy.AbsoluteSeconds = absoluteSeconds;
            defaultPolicy.SlidingSeconds = slidingSeconds;
            defaultPolicy.RefillCount = refillCount;
            return defaultPolicy;
        }

        public string BuildKey()
        {
            return String.IsNullOrEmpty(SubKey)
                            ? Prefix
                            : Prefix + PolicyRepository.KeySeparator + SubKey;
        }

        public string BuildKey(params string[] vals)
        {
            var sep = PolicyRepository.KeySeparator;
            return BuildKey() + sep + String.Join(sep, vals);
        }

        public string BuildPolicyKey()
        {
            return String.IsNullOrEmpty(SubKey)
                           ? Prefix
                           : Prefix + PolicyRepository.PolicyKeySeparator + SubKey;
        }

        public string BuildPolicyKey(string[] vals)
        {
            var sep = PolicyRepository.PolicyKeySeparator;
            return BuildPolicyKey() + sep + String.Join(sep, vals);
        }
    }
}

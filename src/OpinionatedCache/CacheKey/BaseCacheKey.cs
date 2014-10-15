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
        protected string BuildKey()
        {
            return String.IsNullOrEmpty(SubKey)
                            ? Prefix
                            : Prefix + PolicyRepository.KeySeparator + SubKey;
        }

        protected string BuildKey(string val)
        {
            var sep = PolicyRepository.KeySeparator;
            return BuildKey() + sep + val;
        }

        protected string BuildKey(string val1, string val2)
        {
            var sep = PolicyRepository.KeySeparator;
            return BuildKey() + sep + val1 + sep + val2;
        }

        protected string BuildKey(string val1, string val2, string val3)
        {
            var sep = PolicyRepository.KeySeparator;
            return BuildKey() + sep + val1 + sep + val2 + sep + val3;
        }

        protected string BuildKey(string val1, string val2, string val3, string val4)
        {
            var sep = PolicyRepository.KeySeparator;
            return BuildKey() + sep + val1 + sep + val2 + sep + val3 + sep + val4;
        }

        protected string BuildPolicyKey()
        {
            return String.IsNullOrEmpty(SubKey)
                           ? Prefix
                           : Prefix + PolicyRepository.PolicyKeySeparator + SubKey;
        }

        protected string BuildPolicyKey(string val)
        {
            var sep = PolicyRepository.PolicyKeySeparator;
            return BuildPolicyKey() + sep + val;
        }

        protected string BuildPolicyKey(string val1, string val2)
        {
            var sep = PolicyRepository.PolicyKeySeparator;
            return BuildPolicyKey() + sep + val1 + sep + val2;
        }

        protected string BuildPolicyKey(string val1, string val2, string val3)
        {
            var sep = PolicyRepository.PolicyKeySeparator;
            return BuildPolicyKey() + sep + val1 + sep + val2 + sep + val3;
        }

        protected string BuildPolicyKey(string val1, string val2, string val3, string val4)
        {
            var sep = PolicyRepository.PolicyKeySeparator;
            return BuildPolicyKey() + sep + val1 + sep + val2 + sep + val3 + sep + val4;
        }
    }
}

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

        private string Prefix;

        public BaseCacheKey(string prefix)
        {
            Prefix = prefix;
        }

        public virtual string Key
        {
            get
            {
                return Prefix;
            }
        }

        protected string BuildKey(string baseKey, string subKey)
        {
            return baseKey + "." + subKey;
        }

        protected string BuildKey(string baseKey, string subKey, string val)
        {
            return BuildKey(baseKey, subKey) + "." + val;
        }

        protected string BuildKey(string baseKey, string subKey, string val1, string val2)
        {
            return BuildKey(baseKey, subKey) + "." + val1 + "." + val2;
        }

        public virtual string PolicyKey
        {
            get
            {
                return Prefix + "/";
            }
        }

        public virtual ICachePolicy DefaultPolicy
        {
            get
            {
                return new CachePolicy() { AbsoluteSeconds = 10 };    // every ten second should pepper the backing server nicely
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
    }
}

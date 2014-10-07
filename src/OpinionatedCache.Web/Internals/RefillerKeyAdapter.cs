using OpinionatedCache.API;
using OpinionatedCache.Policy;

namespace OpinionatedCache.Web
{
    internal class RefillerKeyAdapter : IBaseCacheKey
    {
        public IBaseCacheKey WrappedKey { get; private set; }

        public RefillerKeyAdapter(IBaseCacheKey wrappedKey)
        {
            WrappedKey = wrappedKey;
        }

        public string Key
        {
            get { return "Refiller" + WrappedKey.Key; }
        }

        public string PolicyKey
        {
            get { return WrappedKey.PolicyKey; }
        }

        public CachePolicy DefaultPolicy
        {
            get { return WrappedKey.DefaultPolicy; }
        }

        public CachePolicy Policy
        {
            get { return WrappedKey.Policy; }
        }
    }
}

using System.Configuration;

namespace OpinionatedCache.Settings
{
    public class CachePolicyConfigurationElement : ConfigurationElement
    {
        public CachePolicyConfigurationElement()
        {
        }

        public CachePolicyConfigurationElement(string key)
        {
            Key = key;
        }

        [ConfigurationProperty("key", IsKey = true, IsRequired = true)]
        public string Key
        {
            get
            {
                return (string)base["key"];
            }
            set
            {
                base["key"] = value;
            }
        }

        [ConfigurationProperty("absoluteSeconds", IsKey = false, IsRequired = false)]
        public int? AbsoluteSeconds
        {
            get
            {
                return (int?)base["absoluteSeconds"];
            }
            set
            {
                base["absoluteSeconds"] = value;
            }
        }

        [ConfigurationProperty("slidingSeconds", IsKey = false, IsRequired = false)]
        public int? SlidingSeconds
        {
            get
            {
                return (int?)base["slidingSeconds"];
            }
            set
            {
                base["slidingSeconds"] = value;
            }
        }

        [ConfigurationProperty("refillCount", IsKey = false, IsRequired = false)]
        public int? RefillCount
        {
            get
            {
                return (int?)base["refillCount"];
            }
            set
            {
                base["refillCount"] = value;
            }
        }
    }
}

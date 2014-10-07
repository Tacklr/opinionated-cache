using System.Configuration;

namespace OpinionatedCache.Settings
{
    public class CachePolicyConfigConfigCollection : ConfigurationElementCollection
    {
        protected override ConfigurationElement CreateNewElement()
        {
            return new CachePolicyConfigurationElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((CachePolicyConfigurationElement)element).Key;
        }
    }
}

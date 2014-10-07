using System.Configuration;

namespace OpinionatedCache.Settings
{
    public class CachePolicySection : ConfigurationSection
    {
        [ConfigurationProperty("policies", IsDefaultCollection = true)]
        [ConfigurationCollection(typeof(CachePolicyConfigConfigCollection),
            AddItemName = "add",
            ClearItemsName = "clear",
            RemoveItemName = "remove")]
        public CachePolicyConfigConfigCollection Policies
        {
            get
            {
                return (CachePolicyConfigConfigCollection)base["policies"];
            }
        }
    }
}

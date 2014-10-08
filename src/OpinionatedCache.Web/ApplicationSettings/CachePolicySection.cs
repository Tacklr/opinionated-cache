﻿// Licensed under the MIT License. See LICENSE.md in the project root for more information.

using System.Configuration;

namespace OpinionatedCache.Settings
{
    public class CachePolicySection : ConfigurationSection
    {
        [ConfigurationProperty("policies", IsDefaultCollection = true)]
        [ConfigurationCollection(typeof(CachePolicyConfigurationCollection),
            AddItemName = "add",
            ClearItemsName = "clear",
            RemoveItemName = "remove")]
        public CachePolicyConfigurationCollection Policies
        {
            get
            {
                return (CachePolicyConfigurationCollection)base["policies"];
            }
        }
    }
}

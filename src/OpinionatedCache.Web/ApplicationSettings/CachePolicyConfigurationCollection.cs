﻿// Licensed under the MIT License. See LICENSE.md in the project root for more information.

using System.Configuration;

namespace OpinionatedCache.Settings
{
    public class CachePolicyConfigurationCollection : ConfigurationElementCollection
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

﻿// Licensed under the MIT License. See LICENSE.md in the project root for more information.

using OpinionatedCache.API;

namespace OpinionatedCache.Policy
{
    public class DefaultCachePolicy : ICachePolicy
    {
        public int AbsoluteSeconds { get; set; }
        public int SlidingSeconds { get; set; }
        public int RefillCount { get; set; }

        public DefaultCachePolicy()
        {
            AbsoluteSeconds = ICachePolicyOptions.Unused;
            SlidingSeconds = ICachePolicyOptions.Unused;
            RefillCount = ICachePolicyOptions.Unused;
        }

        public virtual ICachePolicy Clone()
        {
            return new DefaultCachePolicy
                {
                    AbsoluteSeconds = this.AbsoluteSeconds,
                    SlidingSeconds = this.SlidingSeconds,
                    RefillCount = this.RefillCount
                };
        }

        public static ICachePolicy Sliding(int seconds)
        {
            return new DefaultCachePolicy { SlidingSeconds = seconds };
        }

        public static ICachePolicy Absolute(int seconds)
        {
            return new DefaultCachePolicy { AbsoluteSeconds = seconds };
        }
    }
}

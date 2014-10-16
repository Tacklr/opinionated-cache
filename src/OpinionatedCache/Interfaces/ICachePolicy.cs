﻿// Licensed under the MIT License. See LICENSE.md in the project root for more information.

namespace OpinionatedCache.API
{
    public static class ICachePolicyOptions
    {
        public const int Infinite = -2;
        public const int Unused = -1;
    };

    public interface ICachePolicy
    {
        int AbsoluteSeconds { get; set; }
        int SlidingSeconds { get; set; }
        int RefillCount { get; set; }

        ICachePolicy Clone();
    }
}

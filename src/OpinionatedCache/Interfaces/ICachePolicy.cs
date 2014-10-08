﻿// Licensed under the MIT License. See LICENSE.md in the project root for more information.

namespace OpinionatedCache.API
{
    public interface ICachePolicy
    {
        int AbsoluteSeconds { get; set; }
        ICachePolicy Clone();
        int RefillCount { get; set; }
        int SlidingSeconds { get; set; }
    }
}

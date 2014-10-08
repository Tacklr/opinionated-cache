﻿// Licensed under the MIT License. See LICENSE.md in the project root for more information.

namespace OpinionatedCache.API
{
    public enum FreshnessRequest
    {
        Peek,           // ask the cache, but DO NOT consult the backing store if not present.
        AsyncBackfill,  // we're back-filling the cache for expired entries
        Normal,         // normal access means read the cache and if miss, fall-through to the backing store
        Committed       // bypass the cache and hit the backing-store directly
    }
}

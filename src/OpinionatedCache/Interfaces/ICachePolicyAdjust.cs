﻿// Licensed under the MIT License. See LICENSE.md in the project root for more information.

namespace OpinionatedCache.API
{
    public interface ICachePolicyAdjust
    {
        ICachePolicy Adjust(ICachePolicy policy);
    }
}

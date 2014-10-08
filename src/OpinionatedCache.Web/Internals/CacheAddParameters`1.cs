using System;
using OpinionatedCache.API;

﻿// Licensed under the MIT License. See LICENSE.md in the project root for more information.

namespace OpinionatedCache.Web
{
    internal class CacheAddParameters<T> : BaseCacheAddParameters
        where T : class
    {
        public CacheAddParameters(
            string name
            , ICachePolicy policy
            , Func<FreshnessRequest, T> filler
            , Action<CacheAddParameters<T>, T> putter)
            : base(
                name
                 , policy
                 , (freshness) => filler(freshness)
                 , (parms, value) => putter(parms as CacheAddParameters<T>, (T)value))
        {
        }

        public new Tuple<T, bool> Fill(FreshnessRequest freshness)
        {
            var result = base.Fill(freshness);
            return Tuple.Create<T, bool>(result.Item1 as T, result.Item2);
        }

        public void Put(T value)
        {
            base.Put(value);
        }
    }
}

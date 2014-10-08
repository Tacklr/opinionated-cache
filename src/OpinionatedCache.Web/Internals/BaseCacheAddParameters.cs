﻿// Licensed under the MIT License. See LICENSE.md in the project root for more information.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Web.Caching;
using OpinionatedCache.API;
using OpinionatedCache.Caches;
using OpinionatedCache.Policy;

namespace OpinionatedCache.Web
{
    internal class BaseCacheAddParameters
    {
        private static readonly ConcurrentDictionary<string, DateTime> s_RunningQueries = new ConcurrentDictionary<string, DateTime>();

        public string Name { get; private set; }
        public TimeSpan SlidingTimeout { get; private set; }
        public int NumberOfRefillsRemaining { get; set; }
        public Func<FreshnessRequest, object> Filler { get; private set; }
        public Action<BaseCacheAddParameters, object> Putter { get; private set; }

        readonly int _absoluteSeconds;
        public DateTime AbsoluteExpiration
        {
            get
            {
                if (_absoluteSeconds == CachePolicy.Unused)
                    return Cache.NoAbsoluteExpiration;

                var absolute = DateTime.UtcNow.AddSeconds(_absoluteSeconds);
                return absolute;
            }
        }

        public bool ShouldScheduleRefresh
        {
            get
            {
                // Check if infinite refills are allowed
                if (NumberOfRefillsRemaining == CachePolicy.Infinite)
                    return true;

                // if no more refills are allowed (started at <= 0), or used them up
                if (NumberOfRefillsRemaining <= 0)
                    return false;

                return true;
            }
        }

        public BaseCacheAddParameters(
            string name
            , ICachePolicy policy
            , Func<FreshnessRequest, object> filler
            , Action<BaseCacheAddParameters, object> putter)
        {
            _absoluteSeconds = policy.AbsoluteSeconds;

            Name = name;
            Filler = filler;
            Putter = putter;
            SlidingTimeout = policy.SlidingSeconds == CachePolicy.Unused
                ? Cache.NoSlidingExpiration
                : TimeSpan.FromSeconds(policy.SlidingSeconds);
            NumberOfRefillsRemaining = policy.RefillCount;
        }

        public virtual Tuple<object, bool> Fill(FreshnessRequest freshness)
        {
            HttpCacheShim.Log(Name, "Fill" + freshness);

            var keyString = Name;
            var startTime = DateTime.UtcNow;
            var started = s_RunningQueries.TryAdd(keyString, startTime);

            try
            {
                if (started)
                {
                    var backfilling = freshness == FreshnessRequest.AsyncBackfill;

                    if (backfilling)
                    {
                        if (NumberOfRefillsRemaining > 0)
                            NumberOfRefillsRemaining--;

                        HttpCacheShim.Log("backfilling " + Name + " remaining " + NumberOfRefillsRemaining, "Refiller");
                    }

                    var result = Filler(freshness);
                    return Tuple.Create(result, false);
                }
                else
                {
                    HttpCacheShim.Log(keyString + " already inflight", "Refiller");
                    return Tuple.Create(default(object), true);
                }
            }
            finally
            {
                if (started)
                {
                    var endTime = DateTime.UtcNow;
                    s_RunningQueries.TryRemove(keyString, out startTime);
                    HttpCacheShim.Log(keyString + "@" + startTime + " [" + (endTime - startTime) + "]", "Refiller");
                }
            }
        }

        public virtual void Put(object value)
        {
            HttpCacheShim.Log(value, "Put " + Name);
            Putter(this, value);
        }

        public class NamedComparer : IEqualityComparer<BaseCacheAddParameters>
        {
            private readonly static NamedComparer s_instance = new NamedComparer();
            public static NamedComparer Instance { get { return s_instance; } }

            public bool Equals(BaseCacheAddParameters x, BaseCacheAddParameters y)
            {
                if (x == null)
                    return y == null;
                else if (x.Name == null)
                    return y.Name == null;
                else
                    return x.Name.Equals(y.Name, StringComparison.Ordinal);
            }

            public int GetHashCode(BaseCacheAddParameters obj)
            {
                if (obj == null)
                    return 0;
                else if (obj.Name == null)
                    return -1;
                else
                    return obj.Name.GetHashCode();
            }
        }
    }
}

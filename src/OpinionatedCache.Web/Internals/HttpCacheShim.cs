using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Web;
using System.Web.Caching;
using OpinionatedCache.API;
using OpinionatedCache.SQL;
using OpinionatedCache.Web;

namespace OpinionatedCache.Caches
{
    public class HttpCacheShim : ICache
    {
        public static bool DebugLog { get; set; }
        public static bool DisableBackfill { get; set; }

        private static readonly ManualResetEventSlim s_Shutdown = new ManualResetEventSlim(false, 10);  // drop to kernel quickly as this is used for sleeping

        internal static void Log(object value, string left)
        {
            if (DebugLog)
                Debug.WriteLine(value, left);
        }

        public void Shutdown()
        {
            s_Shutdown.Set();
            BackgroundQueue.WaitForShutdown();
        }

        public void Snooze(int milliseconds)
        {
            s_Shutdown.Wait(milliseconds);
        }

        public T Get<T>(string key)
            where T : class
        {
            return (T)HttpRuntime.Cache.Get(key);
        }

        public IList<TElement> GetCollection<TCollectionKey, TElement>(TCollectionKey collectionKey)
            where TCollectionKey : IBaseCacheKey
            where TElement : class
        {
            var keys = Get<IList<IBaseCacheKey>>(collectionKey.Key);

            if (keys == null)
                return null;

            var elements = new List<TElement>(keys.Count);

            foreach (var key in keys)
            {
                var element = Get<TElement>(key.Key);

                if (element == null)
                    return null;

                elements.Add(element);
            }

            return elements;
        }

        public void Clear()
        {
        }

        public void Clear(IBaseCacheKey key)
        {
            // null key mean NOTHING to clear
            if (key == null)
                return;

            var staleKeysInCache = new List<string>();
            var keyAsString = key.Key;
            var keyAsBaseString = keyAsString + ".";
            var enumerator = HttpRuntime.Cache.GetEnumerator();

            while (enumerator.MoveNext())
            {
                string keyInCache = enumerator.Key.ToString();

                if (keyInCache == keyAsString || keyInCache.StartsWith(keyAsBaseString))
                {
                    staleKeysInCache.Add(keyInCache);
                }
            }

            staleKeysInCache.ForEach(staleKeyInCache => { HttpRuntime.Cache.Remove(staleKeyInCache); Log("Removed stale key from cache: " + staleKeyInCache, "Clear"); });
        }

        public Tuple<TElement, bool> EnrollSingle<TElement>(
                IBaseCacheKey key,
                Func<FreshnessRequest, TElement> filler)
            where TElement : class
        {
            var parameters = BuildParameters(key, filler);
            return MemoizeElement(parameters);
        }

        public Tuple<IList<TElement>, bool> EnrollCollection<TElement>(
                IBaseCacheKey key,
                Func<FreshnessRequest, IList<TElement>> filler,
                params Func<IBaseCacheKey, TElement, IBaseCacheKey>[] keyProjections)
            where TElement : class
        {
            var parameters = BuildParameters(key,
                    (freshness) =>
                    {
                        var curriedKey = new RefillerKeyAdapter(key);
                        var curriedFiller = filler;
                        var curriedProjections = keyProjections;
                        return Refiller(freshness, curriedKey, curriedFiller, curriedProjections);
                    });

            var memo = MemoizeCollection<TElement>(parameters);

            if (memo != null && memo.Item2)
                return memo;

            if (memo == null || memo.Item1 == null)
            {
                // something went wrong doing a memoization of the results, so lets just ask the 
                // filler to synthesize and return directly
                memo = Tuple.Create(filler(FreshnessRequest.Normal), false);
            }

            return memo;
        }

        private Tuple<IList<TElement>, bool> MemoizeCollection<TElement>(
                CacheAddParameters<IList<IBaseCacheKey>> parameters)
            where TElement : class
        {
            IList<TElement> collection = null;
            var result = parameters.Fill(FreshnessRequest.Normal);

            if (result.Item2 || result.Item1 == null)
                return Tuple.Create(collection, result.Item2);

            var cachedKeys = result.Item1;
            collection = new List<TElement>(cachedKeys.Count);
            foreach (var cachedKey in cachedKeys)
            {
                var element = this.Get<TElement>(cachedKey.Key);

                if (element == null)
                    return null;   // failed to find one... declare failure and go to backing store

                collection.Add(element);
            }

            // shove the answer-set into the cache only AFTER we validate all the entries.
            parameters.Put(cachedKeys);
            return Tuple.Create(collection, false);
        }

        private static Tuple<T, bool> MemoizeElement<T>(CacheAddParameters<T> parameters)
            where T : class
        {
            var result = parameters.Fill(FreshnessRequest.Normal);

            if (result != null && !result.Item2)
            {
                var value = result.Item1;
                parameters.Put(value);
            }

            return result;
        }

        private IList<IBaseCacheKey> Refiller<TElement>(
                FreshnessRequest freshness,
                IBaseCacheKey collectionKey,
                Func<FreshnessRequest, IList<TElement>> filler,
                Func<IBaseCacheKey, TElement, IBaseCacheKey>[] keyProjections)
             where TElement : class
        {
            IList<IBaseCacheKey> keys = null;
            var parameters = BuildParameters(collectionKey, filler);
            var result = parameters.Fill(freshness);

            if (result.Item2 || result.Item1 == null)
                return keys;

            var list = result.Item1;
            // clear ALL cache if null return from backing store used to be here...

            // do NOT store in cache yet, this is not the correct answer-set type yet.
            var elementPolicies = new ICachePolicy[keyProjections.Length];
            keys = new List<IBaseCacheKey>(list.Count);

            foreach (var element in list)
            {
                var collectionPolicy = collectionKey.Policy;
                int whichProjection = 0;

                foreach (var projection in keyProjections)
                {
                    var elementKey = projection(collectionKey, element);
                    if (elementKey == null) // ignore null key projections
                        continue;

                    var elementPolicy = elementPolicies[whichProjection];

                    if (elementPolicy == null)
                    {
                        elementPolicy = elementKey.Policy.Clone();
                        elementPolicy.RefillCount = 0;  // we never refill from underneath

                        if (elementPolicy.AbsoluteSeconds > 0)
                        {
                            if (elementPolicy.AbsoluteSeconds <= collectionPolicy.AbsoluteSeconds)
                                elementPolicy.AbsoluteSeconds = collectionPolicy.AbsoluteSeconds + 10; // to help serialized expiriation!
                        }
                        else if (elementPolicy.SlidingSeconds > 0)
                        {
                            if (elementPolicy.SlidingSeconds <= collectionPolicy.SlidingSeconds)
                                elementPolicy.SlidingSeconds = collectionPolicy.SlidingSeconds + 10; // to help serialized expiriation!
                        }

                        elementPolicies[whichProjection] = elementPolicy;
                    }

                    // TODO still not right, Marc
                    InternalPut(BuildParameters(
                            elementKey.Key
                            , elementPolicy
                            , ICacheHelper.Once(element))
                        , element);

                    if (whichProjection++ == 0)
                        keys.Add(elementKey);
                }
            }

            return keys;
        }

        internal virtual CacheAddParameters<T> BuildParameters<T>(IBaseCacheKey key, Func<FreshnessRequest, T> filler)
           where T : class
        {
            return BuildParameters<T>(
                key.Key
                , key.Policy
                , filler);
        }

        internal virtual CacheAddParameters<T> BuildParameters<T>(string name, ICachePolicy policy, Func<FreshnessRequest, T> filler)
           where T : class
        {
            return new CacheAddParameters<T>(
                name
                , policy
                , (freshness) => filler(freshness)
                , (parameters, value) => InternalPut(parameters, value));
        }

        private void InternalPut<T>(CacheAddParameters<T> parameters, T value)
           where T : class
        {
            string key = parameters.Name;
            if (value == null)
            {
                Log(key + " {REMOVE}", "InternalPut");
                HttpRuntime.Cache.Remove(key);
            }
            else
            {
                var callback = MakeCallback(parameters);
                var absoluteExpiration = parameters.AbsoluteExpiration;
                var slidingTimeout = parameters.SlidingTimeout;
                Log(key + "@" + absoluteExpiration + " [" + slidingTimeout + "]", "InternalPut");
                HttpRuntime.Cache.Insert(key, value, null, absoluteExpiration, slidingTimeout, callback);
            }
        }

        private CacheItemUpdateCallback MakeCallback<T>(CacheAddParameters<T> parameters)
            where T : class
        {
            CacheItemUpdateCallback callback = (string updatedKey, CacheItemUpdateReason reason, out object expensiveObject, out CacheDependency dependency, out DateTime absoluteExpiration, out TimeSpan slidingExpiration) =>
            {
                var parms = parameters;
                HandleExpiration(this, parms, updatedKey, reason, out expensiveObject, out dependency, out absoluteExpiration, out slidingExpiration);
            };

            return callback;
        }

        private static void HandleExpiration<T>(HttpCacheShim that
            , CacheAddParameters<T> parameters
            , string cacheKey
            , CacheItemUpdateReason reason
            , out object expensiveObject
            , out CacheDependency dependency
            , out DateTime absoluteExpiration
            , out TimeSpan slidingExpiration)
           where T : class
        {
            Log(cacheKey, "Cache(" + reason + ")");

            expensiveObject = null;
            dependency = null;
            absoluteExpiration = Cache.NoAbsoluteExpiration;
            slidingExpiration = Cache.NoSlidingExpiration;

            // if we were not shutting down, might want to handle the reuse/refresh
            if (reason == CacheItemUpdateReason.Expired
                && !AppDomain.CurrentDomain.IsFinalizingForUnload())
            {
                if (parameters.ShouldScheduleRefresh
                    && HttpCacheShim.DisableBackfill == false
                    && !BackgroundQueue.IsBacklogged())
                {
                    // we need queue a request to the underlying store to get more current data into the cache so it stays primed.
                    BackgroundQueue.Enqueue(parameters);
                }
            }
        }

        internal class BackgroundQueue
        {
            public static void WaitForShutdown()
            {
                while (s_BackgroundQueue.Count > 0)
                    SpinWait.SpinUntil(AppDomain.CurrentDomain.IsFinalizingForUnload, 500);    // just so the other threads get a chance

                while (s_BackgroundThread != null && s_BackgroundThread.IsAlive)
                    SpinWait.SpinUntil(AppDomain.CurrentDomain.IsFinalizingForUnload, 500);    // just so the other threads get a chance
            }

            public static bool IsBacklogged()
            {
                return s_BackgroundQueue.Count > 10;    // TODO seems quite arbitrary
            }

            private static readonly SetQueue<BaseCacheAddParameters> s_BackgroundQueue =
                new SetQueue<BaseCacheAddParameters>(BaseCacheAddParameters.NamedComparer.Instance);

            internal static void Enqueue(BaseCacheAddParameters parameters)
            {
                Log(parameters.Name, "RefillQueue");

                lock (s_BackgroundQueue)
                {
                    s_BackgroundQueue.Enqueue(parameters);
                    EnsureBackgroundThread();
                }
            }

            private static Thread s_BackgroundThread;
            private static void EnsureBackgroundThread()
            {
                if (s_BackgroundThread == null || !s_BackgroundThread.IsAlive)
                {
                    var thread = new Thread(BackgroundThreadProc)
                    {
                        Name = "BackgroundQueue",
                        IsBackground = true /* don't want them to stay alive just due to me!*/
                    };
                    Thread.MemoryBarrier();
                    s_BackgroundThread = thread;
                    thread.Start(thread);
                }
            }

            private static void BackgroundThreadProc(object myThread)
            {
                var lastAwake = DateTime.UtcNow;

                while (true)
                {
                    var job = default(BaseCacheAddParameters);
                    lock (s_BackgroundQueue)
                    {
                        if (s_BackgroundQueue.Count > 0)
                            job = s_BackgroundQueue.Dequeue();
                    }

                    if (job != null)
                    {
                        lastAwake = DateTime.UtcNow;

                        try
                        {
                            var result = job.Fill(FreshnessRequest.AsyncBackfill);
                            var wasRunning = result.Item2;

                            if (!wasRunning)
                            {
                                var value = result.Item1;
                                job.Put(value);
                                continue; // processed, don't requeue
                            }
                        }
                        catch (Exception ex)
                        {
                            Log("Background for " + job.Name + " exception: " + (ex.Message ?? ex.GetType().ToString()), "Refiller");

                            if (!ex.IsWorthRetry())
                            {
                                if (!(ex is ThreadAbortException))
                                {
                                    //ExceptionManager.Publish(ex, ...);
                                }

                                continue; // fatal, do not requeue
                            }
                        }

                        // it's worth another try... put it on the end of the queue
                        Enqueue(job);
                        SpinWait.SpinUntil(AppDomain.CurrentDomain.IsFinalizingForUnload, 500);    // just so the other threads get a chance
                    }
                    else
                    {
                        // wait to see if we should stay alive...
                        if (lastAwake.AddSeconds(60) < DateTime.UtcNow)
                        {
                            lock (s_BackgroundQueue)
                            {
                                if (s_BackgroundQueue.Count == 0)
                                {
                                    s_BackgroundThread = null; // drop the static thread...
                                    Thread.MemoryBarrier();
                                    return; // we're dead, jim
                                }
                            }
                        }
                        else
                            SpinWait.SpinUntil(AppDomain.CurrentDomain.IsFinalizingForUnload, 5000);    // just so the other threads get a chance
                    }
                }
            }
        }
    }
}

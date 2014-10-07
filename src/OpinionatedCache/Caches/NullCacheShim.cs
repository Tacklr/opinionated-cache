using System;
using System.Collections.Generic;
using System.Threading;
using OpinionatedCache.API;

namespace OpinionatedCache.Caches
{
    public class NullCacheShim : ICache
    {
        private static readonly ManualResetEventSlim s_Shutdown = new ManualResetEventSlim(false, 10);  // drop to kernel quickly as this is used for sleeping

        public void Shutdown()
        {
        }

        public void Snooze(int milliseconds)
        {
            s_Shutdown.Wait(milliseconds);
        }   
     
        public T Get<T>(string key)
            where T : class
        {
            return null;
        }

        public IList<TElement> GetCollection<TCollectionKey, TElement>(TCollectionKey collectionKey)
            where TCollectionKey : IBaseCacheKey
            where TElement : class
        {
            return null;
        }

        public void Clear()
        {
        }

        public void Clear(IBaseCacheKey key)
        {
        }

        public Tuple<TElement, bool> EnrollSingle<TElement>(
                IBaseCacheKey key,
                Func<FreshnessRequest, TElement> filler) 
            where TElement : class
        {
            var element = filler(FreshnessRequest.Normal);
            return Tuple.Create(element, false);
        }

        public Tuple<IList<TElement>, bool> EnrollCollection<TElement>(
                IBaseCacheKey key,
                Func<FreshnessRequest, IList<TElement>> filler,
                params Func<IBaseCacheKey, TElement, IBaseCacheKey>[] keyProjections)
            where TElement : class
        {
            var list = filler(FreshnessRequest.Normal);
            var keys = MemoizeCollection(key, list, keyProjections);
            return Tuple.Create(list, false);
        }

        private IList<TElement> MemoizeCollection<TElement>(
                IBaseCacheKey key,
                IList<TElement> list,
                params Func<IBaseCacheKey, TElement, IBaseCacheKey>[] keyProjections)
         {
            if (list != null)
            {
                var keys = new List<IBaseCacheKey>(list.Count);

                foreach (var element in list)
                {
                    foreach (var projection in keyProjections)
                    {
                        var elementKey = projection(key, element);
                        keys.Add(elementKey);
                    }
                }
            }

            return list;
        }
    }
}

using System;
using System.Collections.Generic;

namespace OpinionatedCache.API
{
    public static partial class ICacheHelper
    {
        // returns the specified value one time to enque values returned
        // from action methods.
        public static Func<FreshnessRequest, T> Once<T>(T value)
        {
            return (FreshnessRequest freshness) =>
            {
                var memo = value;
                try
                {
                    return freshness == FreshnessRequest.AsyncBackfill ? default(T) : memo;
                }
                finally
                {
                    memo = default(T);   // release the reference!
                }
            };
        }

        public static TRet GetBy<TRet>(
            this ICache cache,
            Func<IBaseCacheKey> keyMaker,
            Func<FreshnessRequest, TRet> gettor)
            where TRet : class
        {
            var cacheKey = keyMaker();
            var cachedResult = cache.Get<TRet>(cacheKey.Key);

            if (cachedResult != null)
                return cachedResult;

            return cache.EnrollRetry(cacheKey, gettor);
        }

        public static TRet GetBy<TKey, TRet>(
            this ICache cache,
            TKey keyValue,
            Func<TKey, IBaseCacheKey> keyMaker,
            Func<FreshnessRequest, TRet> gettor)
            where TRet : class
        {
            var cacheKey = keyMaker(keyValue);
            var cachedResult = cache.Get<TRet>(cacheKey.Key);

            if (cachedResult != null)
                return cachedResult;

            return cache.EnrollRetry(cacheKey, gettor);
        }

        public static TRet GetBy<TKey1, TKey2, TRet>(
            this ICache cache,
            TKey1 keyOneValue,
            TKey2 keyTwoValue,
            Func<TKey1, TKey2, IBaseCacheKey> keyMaker,
            Func<FreshnessRequest, TRet> gettor)
            where TRet : class
        {
            var cacheKey = keyMaker(keyOneValue, keyTwoValue);
            var cachedResult = cache.Get<TRet>(cacheKey.Key);

            if (cachedResult != null)
                return cachedResult;

            return cache.EnrollRetry(cacheKey, gettor);
        }

        public static TRet GetBy<TKey1, TKey2, TKey3, TRet>(
            this ICache cache,
            TKey1 keyOneValue,
            TKey2 keyTwoValue,
            TKey3 keyThreeValue,
            Func<TKey1, TKey2, TKey3, IBaseCacheKey> keyMaker,
            Func<FreshnessRequest, TRet> gettor)
            where TRet : class
        {
            var cacheKey = keyMaker(keyOneValue, keyTwoValue, keyThreeValue);
            var cachedResult = cache.Get<TRet>(cacheKey.Key);

            if (cachedResult != null)
                return cachedResult;

            return cache.EnrollRetry(cacheKey, gettor);
        }

        public static TRet GetBy<TKey1, TKey2, TKey3, TKey4, TRet>(
            this ICache cache,
            TKey1 keyOneValue,
            TKey2 keyTwoValue,
            TKey3 keyThreeValue,
            TKey4 keyFourValue,
            Func<TKey1, TKey2, TKey3, TKey4, IBaseCacheKey> keyMaker,
            Func<FreshnessRequest, TRet> gettor)
            where TRet : class
        {
            var cacheKey = keyMaker(keyOneValue, keyTwoValue, keyThreeValue, keyFourValue);
            var cachedResult = cache.Get<TRet>(cacheKey.Key);

            if (cachedResult != null)
                return cachedResult;

            return cache.EnrollRetry(cacheKey, gettor);
        }

        public static TRet GetBy<TKey1, TKey2, TKey3, TKey4, TKey5, TRet>(
            this ICache cache,
            TKey1 keyOneValue,
            TKey2 keyTwoValue,
            TKey3 keyThreeValue,
            TKey4 keyFourValue,
            TKey5 keyFiveValue,
            Func<TKey1, TKey2, TKey3, TKey4, TKey5, IBaseCacheKey> keyMaker,
            Func<FreshnessRequest, TRet> gettor)
            where TRet : class
        {
            var cacheKey = keyMaker(keyOneValue, keyTwoValue, keyThreeValue, keyFourValue, keyFiveValue);
            var cachedResult = cache.Get<TRet>(cacheKey.Key);

            if (cachedResult != null)
                return cachedResult;

            return cache.EnrollRetry(cacheKey, gettor);
        }

        public static IList<TElement> GetAll<TCollectionKey, TElement>(
            this ICache cache
            , Func<TCollectionKey> collectionKeyMaker
            , Func<FreshnessRequest, IList<TElement>> gettor
            , params Func<IBaseCacheKey, TElement, IBaseCacheKey>[] keyProjections)
            where TCollectionKey : IBaseCacheKey
            where TElement : class
        {
            var cacheKey = collectionKeyMaker();
            do
            {
                var cachedResult = cache.GetCollection<TCollectionKey, TElement>(cacheKey);

                if (cachedResult != null)
                    return cachedResult;

                var result = cache.EnrollCollection(cacheKey, gettor, keyProjections);

                if (result != null && false == result.Item2)
                    return result.Item1;

                cache.Snooze(100);
            }
            while (true);
        }

        public static TElement AddUpdate<TElement>(
            this ICache cache
            , Func<TElement> item
            , Func<IBaseCacheKey> collectionKeyMaker
            , params Func<TElement, IBaseCacheKey>[] keyProjections)
            where TElement : class
        {
            var element = item();

            if (element != null)
            {
                foreach (var projection in keyProjections)
                    cache.EnrollRetry(projection(element), ICacheHelper.Once(element));

                if (collectionKeyMaker != null)
                    cache.Clear(collectionKeyMaker());
            }

            return element;
        }

        public static TElement Delete<TElement>(
            this ICache cache
            , Func<TElement> item
            , Func<IBaseCacheKey> collectionKeyMaker
            , params Func<TElement, IBaseCacheKey>[] keyProjections)
            where TElement : class
        {
            var element = item();

            if (element != null)
            {
                foreach (var projection in keyProjections)
                    cache.Clear(projection(element));

                if (collectionKeyMaker != null)
                    cache.Clear(collectionKeyMaker());
            }

            return element;
        }

        public static TRet EnrollRetry<TRet>(
            this ICache cache,
            IBaseCacheKey cacheKey,
            Func<FreshnessRequest, TRet> filler) where TRet : class
        {
            // if you can't give me a key, I can't save it for you!
            if (cacheKey == null)
                return default(TRet);

            do
            {
                var result = cache.EnrollSingle<TRet>(cacheKey, filler);

                if (false == result.Item2)
                    return result.Item1;

                // we we're running, snooze for a second then check the cache..
                cache.Snooze(100);
                var value = cache.Get<TRet>(cacheKey.Key);

                if (value != null)
                    return value;
            }
            while (true);
        }

        public static void Handle<TElement>(
            this ICache cache
            , Action handle
            , Func<IBaseCacheKey> collectionKeyMaker)
            where TElement : class
        {
            handle();
            cache.Clear(collectionKeyMaker());
        }

        public static IChild AddNested<IParent, IChild>(
            this ICache cache
            , IChild born
            , Func<IChild, string> cacheKey
            , Func<IParent, Lazy<IList<IChild>>> lister)
            where IParent : class
        {
            if (born != null)
            {
                // peek only, we don't want to load things we don't already have
                var parent = cache.Get<IParent>(cacheKey(born));

                if (parent != null)
                {
                    var list = lister(parent);

                    if (list != null && list.IsValueCreated)
                        list.Value.Add(born);
                }
            }

            return born;
        }

        public static IChild RemoveNested<IParent, IChild>(
            this ICache cache
            , IChild died
            , Func<IChild, string> cacheKey
            , Func<IParent, Lazy<IList<IChild>>> lister)
            where IParent : class
        {
            if (died != null)
            {
                // peek only, we don't want to load things we don't already have
                var parent = cache.Get<IParent>(cacheKey(died));

                if (parent != null)
                {
                    var list = lister(parent);

                    if (list != null && list.IsValueCreated)
                        list.Value.Remove(died);
                }
            }

            return died;
        }
    }
}

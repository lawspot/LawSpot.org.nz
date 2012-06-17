using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Lawspot.Backend;

namespace Lawspot.Shared
{
    public static class CacheProvider
    {
        private class CacheEntry
        {
            public DateTime Loaded;
            public TimeSpan Longevity;
            public object Data;
            public Task Task;
        }

        private static ConcurrentDictionary<object, CacheEntry> cache = new ConcurrentDictionary<object,CacheEntry>();

        /// <summary>
        /// Caches the result of a database query.
        /// </summary>
        /// <typeparam name="T"> The type of result to cache. </typeparam>
        /// <param name="key"> The key to use to identify the query.  Must implement GetHashCode(). </param>
        /// <param name="callback"> The callback to call to compute the result. </param>
        /// <param name="longevity"> The duration to cache the value. </param>
        /// <returns> The result of the database query. </returns>
        public static T CacheDatabaseQuery<T>(object key, Func<LawspotDataContext, T> callback, TimeSpan longevity)
        {
            // Retrieve the value from the cache.
            CacheEntry cacheEntry;
            if (cache.TryGetValue(key, out cacheEntry))
            {
                var task = cacheEntry.Task;

                // The value exists in the cache.  Check if it is out of date.
                if (DateTime.Now.Subtract(cacheEntry.Loaded) < cacheEntry.Longevity)
                {
                    // The value is still fresh.
                    return (T)cacheEntry.Data;
                }

                // The value is stale.
                if (task == null || task.IsFaulted)
                {
                    // Asynchronously refresh the cache.
                    cacheEntry.Task = Task.Factory.StartNew(() =>
                        {
                            using (var connection = new LawspotDataContext())
                            {
                                // Use the callback to determine the value.
                                var value = callback(connection);

                                // Update the cache.
                                var newCacheEntry = new CacheEntry();
                                newCacheEntry.Loaded = DateTime.Now;
                                newCacheEntry.Longevity = longevity;
                                newCacheEntry.Data = value;
                                cache[key] = newCacheEntry;
                            }
                        });
                }

                // Return the stale value.
                return (T)cacheEntry.Data;
            }
            else
            {
                // The value does not exist in the cache.
                using (var connection = new LawspotDataContext())
                {
                    // Use the callback to determine the value.
                    var value = callback(connection);

                    // Update the cache.
                    var newCacheEntry = new CacheEntry();
                    newCacheEntry.Loaded = DateTime.Now;
                    newCacheEntry.Longevity = longevity;
                    newCacheEntry.Data = value;
                    cache[key] = newCacheEntry;

                    // Return the loaded value.
                    return value;
                }
            }
        }
    }
}
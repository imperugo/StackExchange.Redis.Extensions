namespace StackExchange.Redis.Extensions.Newtonsoft
{
    /// <summary>
    /// This class is used a wrapper for serialized content.
    /// </summary>
    /// <typeparam name="T"></typeparam>
	public class CachedObject<T>
	{
        /// <summary>
        /// Constructor for CacheObject wraper
        /// </summary>
        /// <param name="cachedValue">The cache value</param>
		public CachedObject(T cachedValue)
		{
			CachedValue = cachedValue;
		}

        /// <summary>
        /// The cache content
        /// </summary>
		public T CachedValue { get; set; }
	}
}
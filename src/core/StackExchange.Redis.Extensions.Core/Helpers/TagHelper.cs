namespace StackExchange.Redis.Extensions.Core.Helpers
{
    /// <summary>
    /// Helper for generating ta key
    /// </summary>
    public static class TagHelper
    {
        /// <summary>
        ///     Generate key associated with tag
        /// </summary>
        /// <param name="tag">Tag</param>
        /// <returns>Return key associated with tag</returns>
        public static string GenerateTagKey(string tag) => "tag:" + tag;
    }
}

namespace StackExchange.Redis.Extensions.Core.Helpers
{
    public static class TagHelper
    {
        public static string GenerateTagKey(string tag) =>
            $"tag:{tag}";

        public static string GenerateTagHashKey(string tag) =>
            $"tag_hash:{tag}";

        public static string GenerateTagSetKey(string tag) =>
            $"tag_set:{tag}";
    }
}

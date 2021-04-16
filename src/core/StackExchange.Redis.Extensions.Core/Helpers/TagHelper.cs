namespace StackExchange.Redis.Extensions.Core.Helpers
{
    public static class TagHelper
    {
        public static string GenerateTagKey(string tag) =>
            $"tag:{tag}";
    }
}

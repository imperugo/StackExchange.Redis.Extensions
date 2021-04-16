namespace StackExchange.Redis.Extensions.Core.Helpers
{
    internal static class TagHelper
    {
        internal static string GenerateTagKey(string tag) =>
            $"tag:{tag}";
    }
}

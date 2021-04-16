using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("StackExchange.Redis.Extensions.Core.Tests")]

namespace StackExchange.Redis.Extensions.Core.Helpers
{
    internal static class TagHelper
    {
        internal static string GenerateTagKey(string tag) =>
            $"tag:{tag}";
    }
}

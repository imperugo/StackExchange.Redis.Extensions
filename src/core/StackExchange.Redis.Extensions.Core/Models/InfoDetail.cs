namespace StackExchange.Redis.Extensions.Core.Models
{
    /// <summary>
    /// A class that contains redis info.
    /// </summary>
    public class InfoDetail
    {
        /// <summary>
        /// Gets or sets the category name
        /// </summary>
        public string Category { get; set; }

        /// <summary>
        /// Gets or sets the redis key.
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        /// Gets or sets the informations.
        /// </summary>
        public string InfoValue { get; set; }
    }
}

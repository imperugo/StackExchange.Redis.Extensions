using System;
using System.Collections.Generic;
using System.Text;

namespace StackExchange.Redis.Extensions.Core.Models
{
    /// <summary>
    /// A class that contains redis info.
    /// </summary>
    public class InfoDetail
    {
        /// <summary>
        /// The category name
        /// </summary>
        public string Category { get; set; }

        /// <summary>
        /// The redis key.
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        /// The informations.
        /// </summary>
        /// <value></value>
        public string InfoValue { get; set; }
    }
}

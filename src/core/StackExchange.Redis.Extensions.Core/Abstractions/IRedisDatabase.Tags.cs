using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace StackExchange.Redis.Extensions.Core.Abstractions
{
    public partial interface IRedisDatabase
    {
        /// <summary>
        /// Method for retrieving all entities of the same type tagged with a specific tag
        /// </summary>
        /// <typeparam name="T">Type of the retrieved entity</typeparam>
        /// <param name="tag">The serached tag</param>
        /// <returns>An enumerable of results, or an empty enumerable of no results found</returns>
        Task<IEnumerable<T>> GetByTag<T>(string tag);
    }
}

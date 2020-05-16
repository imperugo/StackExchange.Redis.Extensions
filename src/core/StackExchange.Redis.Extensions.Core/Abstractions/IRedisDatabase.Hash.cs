using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using StackExchange.Redis;
using StackExchange.Redis.Extensions.Core.Models;

namespace StackExchange.Redis.Extensions.Core.Abstractions
{
    /// <summary>
    /// The Redis Database
    /// </summary>
    public partial interface IRedisDatabase
    {
        /// <summary>
        ///     Removes the specified fields from the hash stored at key.
        ///     Specified fields that do not exist within this hash are ignored.
        /// </summary>
        /// <remarks>
        ///     Time complexity: O(1)
        /// </remarks>
        /// <param name="hashKey">Key of the hash</param>
        /// <param name="key">Key of the entry</param>
        /// <param name="flag">Behaviour markers associated with a given command</param>
        /// <returns>
        ///     If key is deleted returns true.
        ///     If key does not exist, it is treated as an empty hash and this command returns false.
        /// </returns>
        Task<bool> HashDeleteAsync(string hashKey, string key, CommandFlags flag = CommandFlags.None);

        /// <summary>
        ///     Removes the specified fields from the hash stored at key.
        ///     Specified fields that do not exist within this hash are ignored.
        ///     If key does not exist, it is treated as an empty hash and this command returns 0.
        /// </summary>
        /// <remarks>
        ///     Time complexity: O(N) where N is the number of fields to be removed.
        /// </remarks>
        /// <param name="hashKey">Key of the hash</param>
        /// <param name="keys">Keys to retrieve from the hash</param>
        /// <param name="flag">Behaviour markers associated with a given command</param>
        /// <returns>Tthe number of fields that were removed from the hash, not including specified but non existing fields.</returns>
        Task<long> HashDeleteAsync(string hashKey, IEnumerable<string> keys, CommandFlags flag = CommandFlags.None);

        /// <summary>
        ///     Returns if field is an existing field in the hash stored at key.
        /// </summary>
        /// <remarks>
        ///     Time complexity: O(1)
        /// </remarks>
        /// <param name="hashKey">The key of the hash in redis</param>
        /// <param name="key">The key of the field in the hash</param>
        /// <param name="flag">Behaviour markers associated with a given command</param>
        Task<bool> HashExistsAsync(string hashKey, string key, CommandFlags flag = CommandFlags.None);

        /// <summary>
        ///     Returns the value associated with field in the hash stored at key.
        /// </summary>
        /// <remarks>
        ///     Time complexity: O(1)
        /// </remarks>
        /// <typeparam name="T">Type of the returned value</typeparam>
        /// <param name="hashKey">Key of the hash</param>
        /// <param name="key">Key of the entry</param>
        /// <param name="flag">Behaviour markers associated with a given command</param>
        /// <returns>the value associated with field, or nil when field is not present in the hash or key does not exist.</returns>
        Task<T> HashGetAsync<T>(string hashKey, string key, CommandFlags flag = CommandFlags.None);

        /// <summary>
        ///     Returns the values associated with the specified fields in the hash stored at key.
        ///     For every field that does not exist in the hash, a nil value is returned.
        ///     Because a non-existing keys are treated as empty hashes, running HMGET against a non-existing key will return a list of nil values.
        /// </summary>
        /// <remarks>
        ///     Time complexity: O(N) where N is the number of fields being requested.
        /// </remarks>
        /// <typeparam name="T">Type of the returned value</typeparam>
        /// <param name="hashKey">Key of the hash</param>
        /// <param name="keys">Keys to retrieve from the hash</param>
        /// <param name="flag">Behaviour markers associated with a given command</param>
        /// <returns>list of values associated with the given fields, in the same order as they are requested.</returns>
        Task<Dictionary<string, T>> HashGetAsync<T>(string hashKey, IList<string> keys, CommandFlags flag = CommandFlags.None);

        /// <summary>
        ///     Returns all fields and values of the hash stored at key. In the returned value,
        ///     every field name is followed by its value, so the length of the reply is twice the size of the hash.
        /// </summary>
        /// <remarks>
        ///     Time complexity: O(N) where N is the size of the hash.
        /// </remarks>
        /// <typeparam name="T">Type of the returned value</typeparam>
        /// <param name="hashKey">Key of the hash</param>
        /// <param name="flag">Behaviour markers associated with a given command</param>
        /// <returns>list of fields and their values stored in the hash, or an empty list when key does not exist.</returns>
        Task<Dictionary<string, T>> HashGetAllAsync<T>(string hashKey, CommandFlags flag = CommandFlags.None);

        /// <summary>
        ///     Increments the number stored at field in the hash stored at key by increment.
        ///     If key does not exist, a new key holding a hash is created.
        ///     If field does not exist the value is set to 0 before the operation is performed.
        ///     The range of values supported by HINCRBY is limited to 64 bit signed integers.
        /// </summary>
        /// <remarks>
        ///     Time complexity: O(1)
        /// </remarks>
        /// <param name="hashKey">Key of the hash</param>
        /// <param name="key">Key of the entry</param>
        /// <param name="value">the value at field after the increment operation</param>
        /// <param name="flag">Behaviour markers associated with a given command</param>
        Task<long> HashIncerementByAsync(string hashKey, string key, long value, CommandFlags flag = CommandFlags.None);

        /// <summary>
        ///     Increment the specified field of an hash stored at key, and representing a floating point number, by the specified increment.
        ///     If the field does not exist, it is set to 0 before performing the operation.
        /// </summary>
        /// <remarks>
        ///     <para>
        ///         An error is returned if one of the following conditions occur:
        ///         * The field contains a value of the wrong type (not a string).
        ///         * The current field content or the specified increment are not parsable as a double precision floating point number.
        ///     </para>
        ///     <para>
        ///         Time complexity: O(1)
        ///     </para>
        /// </remarks>
        /// <param name="hashKey">Key of the hash</param>
        /// <param name="key">Key of the entry</param>
        /// <param name="value">the value at field after the increment operation</param>
        /// <param name="flag">Behaviour markers associated with a given command</param>
        /// <returns>the value at field after the increment operation.</returns>
        Task<double> HashIncerementByAsync(string hashKey, string key, double value, CommandFlags flag = CommandFlags.None);

        /// <summary>
        ///     Returns all field names in the hash stored at key.
        /// </summary>
        /// <remarks>
        ///     Time complexity: O(N) where N is the size of the hash.
        /// </remarks>
        /// <param name="hashKey">Key of the hash</param>
        /// <param name="flag">Behaviour markers associated with a given command</param>
        /// <returns>list of fields in the hash, or an empty list when key does not exist.</returns>
        Task<IEnumerable<string>> HashKeysAsync(string hashKey, CommandFlags flag = CommandFlags.None);

        /// <summary>
        ///     Returns the number of fields contained in the hash stored at key.
        /// </summary>
        /// <remarks>
        ///     Time complexity: O(1)
        /// </remarks>
        /// <param name="hashKey">Key of the hash</param>
        /// <param name="flag">Behaviour markers associated with a given command</param>
        /// <returns>number of fields in the hash, or 0 when key does not exist.</returns>
        Task<long> HashLengthAsync(string hashKey, CommandFlags flag = CommandFlags.None);

        /// <summary>
        ///     Sets field in the hash stored at key to value. If key does not exist, a new key holding a hash is created. If field already exists in the hash, it is overwritten.
        /// </summary>
        /// <typeparam name="T">Type of the returned value</typeparam>
        /// <param name="hashKey">The key of the hash in redis</param>
        /// <param name="key">The key of the field in the hash</param>
        /// <param name="value">The value to be inserted</param>
        /// <param name="nx">Behave like hsetnx - set only if not exists</param>
        /// <param name="flag">Behaviour markers associated with a given command</param>
        /// <param name="tags">Tags</param>
        /// <returns>
        ///     <c>true</c> if field is a new field in the hash and value was set.
        ///     <c>false</c> if field already exists in the hash and no operation was performed.
        /// </returns>
        Task<bool> HashSetAsync<T>(string hashKey, string key, T value, bool nx = false, CommandFlags flag = CommandFlags.None, HashSet<string> tags = null);

        /// <summary>
        ///     Sets the specified fields to their respective values in the hash stored at key.
        ///     This command overwrites any existing fields in the hash.
        ///     If key does not exist, a new key holding a hash is created.
        /// </summary>
        /// <remarks>
        ///     Time complexity: O(N) where N is the number of fields being set.
        /// </remarks>
        /// <typeparam name="T">Type of the returned value</typeparam>
        /// <param name="hashKey">Key of the hash</param>
        /// <param name="values">The values to be inserted</param>
        /// <param name="flag">Behaviour markers associated with a given command</param>
        Task HashSetAsync<T>(string hashKey, IDictionary<string, T> values, CommandFlags flag = CommandFlags.None);

        /// <summary>
        ///     Returns all values in the hash stored at key.
        /// </summary>
        /// <remarks>
        ///     Time complexity: O(N) where N is the size of the hash.
        /// </remarks>
        /// <typeparam name="T">Type of the returned value</typeparam>
        /// <param name="hashKey">Key of the hash</param>
        /// <param name="flag">Behaviour markers associated with a given command</param>
        /// <returns>list of values in the hash, or an empty list when key does not exist.</returns>
        Task<IEnumerable<T>> HashValuesAsync<T>(string hashKey, CommandFlags flag = CommandFlags.None);

        /// <summary>
        ///     iterates fields of Hash types and their associated values.
        /// </summary>
        /// <remarks>
        ///     Time complexity: O(1) for every call. O(N) for a complete iteration, including enough command calls for the cursor to return back to 0.
        ///     N is the number of elements inside the collection.
        /// </remarks>
        /// <typeparam name="T">Type of the returned value</typeparam>
        /// <param name="hashKey">Key of the hash</param>
        /// <param name="pattern">GLOB search pattern</param>
        /// <param name="pageSize">Number of elements to retrieve from the redis server in the cursor</param>
        /// <param name="flag">Behaviour markers associated with a given command</param>
        Dictionary<string, T> HashScan<T>(string hashKey, string pattern, int pageSize = 10, CommandFlags flag = CommandFlags.None);
    }
}

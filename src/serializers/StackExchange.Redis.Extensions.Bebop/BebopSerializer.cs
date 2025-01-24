using System;
using Bebop.Runtime;

using StackExchange.Redis.Extensions.Core;

namespace StackExchange.Redis.Extensions.Bebop
{
    /// <summary>
    /// Bebop implementation of <see cref="ISerializer"/>
    /// </summary>
    public class BebopSerializer : ISerializer
    {
        /// <inheritdoc/>
        public byte[] Serialize(object item)
        {
            var record = BebopMirror.FindRecordFromType(item.GetType());
            return record.Encode(item);
        }

        /// <inheritdoc/>
        public T Deserialize<T>(byte[] serializedObject)
        {
            var record = BebopMirror.FindRecordFromType(typeof(T));
            if (record.Decode(serializedObject) is T value)
                return value;

            throw new InvalidOperationException($"Unable to cast {typeof(T)} to {record.Type}");
        }
    }
}

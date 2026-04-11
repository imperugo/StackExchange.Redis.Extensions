// Copyright (c) Ugo Lattanzi.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

using System;

namespace StackExchange.Redis.Extensions.Core;

/// <summary>
/// A decorator that wraps an <see cref="ISerializer"/> with compression/decompression.
/// Data is first serialized by the inner serializer, then compressed before being stored in Redis.
/// On read, data is decompressed before being deserialized.
/// </summary>
/// <remarks>
/// Enabling compression on an existing dataset will make previously stored (uncompressed) data unreadable.
/// Consider using a migration strategy or a magic-byte prefix to distinguish compressed from uncompressed data.
/// </remarks>
public class CompressedSerializer : ISerializer
{
    private readonly ISerializer inner;
    private readonly ICompressor compressor;

    /// <summary>
    /// Initializes a new instance of the <see cref="CompressedSerializer"/> class.
    /// </summary>
    /// <param name="inner">The inner serializer to delegate serialization to.</param>
    /// <param name="compressor">The compressor to use for compression/decompression.</param>
    public CompressedSerializer(ISerializer inner, ICompressor compressor)
    {
        if (inner is CompressedSerializer)
            throw new ArgumentException("Cannot wrap a CompressedSerializer inside another CompressedSerializer. This would cause double-compression and corrupt data.", nameof(inner));

        this.inner = inner ?? throw new ArgumentNullException(nameof(inner));
        this.compressor = compressor ?? throw new ArgumentNullException(nameof(compressor));
    }

    /// <inheritdoc/>
    public byte[] Serialize<T>(T? item)
    {
        var raw = inner.Serialize(item);

        return raw.Length == 0
            ? raw
            : compressor.Compress(raw);
    }

    /// <inheritdoc/>
    public T? Deserialize<T>(byte[]? serializedObject)
    {
        if (serializedObject is not { Length: > 0 })
            return default;

        byte[] decompressed;

        try
        {
            decompressed = compressor.Decompress(serializedObject);
        }
#pragma warning disable CA1031 // Intentional catch-all to wrap with actionable context
        catch (Exception ex)
#pragma warning restore CA1031
        {
            throw new InvalidOperationException("Failed to decompress data from Redis. This may indicate the data was stored before compression was enabled. Consider migrating existing data or disabling compression temporarily to read legacy entries.", ex);
        }

        return inner.Deserialize<T>(decompressed);
    }
}

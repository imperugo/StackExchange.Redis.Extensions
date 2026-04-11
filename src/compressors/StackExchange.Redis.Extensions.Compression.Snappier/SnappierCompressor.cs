// Copyright (c) Ugo Lattanzi.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

using System;

using Snappier;

namespace StackExchange.Redis.Extensions.Core;

/// <summary>
/// An <see cref="ICompressor"/> implementation using Snappy compression via Snappier.
/// Snappy prioritizes speed over compression ratio, similar to LZ4.
/// </summary>
public class SnappierCompressor : ICompressor
{
    /// <inheritdoc/>
    public byte[] Compress(byte[] data)
    {
        var maxLength = Snappy.GetMaxCompressedLength(data.Length);
        var buffer = new byte[maxLength];
        var compressedLength = Snappy.Compress(data, buffer);

        return buffer.AsSpan(0, compressedLength).ToArray();
    }

    /// <inheritdoc/>
    public byte[] Decompress(byte[] compressedData)
    {
        var decompressedLength = Snappy.GetUncompressedLength(compressedData);
        var buffer = new byte[decompressedLength];
        var actualLength = Snappy.Decompress(compressedData, buffer);

        return buffer.AsSpan(0, actualLength).ToArray();
    }
}

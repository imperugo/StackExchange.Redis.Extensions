// Copyright (c) Ugo Lattanzi.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

using ZstdSharp;

namespace StackExchange.Redis.Extensions.Core;

/// <summary>
/// An <see cref="ICompressor"/> implementation using Zstandard compression via ZstdSharp.
/// Zstd offers an excellent balance between compression ratio and speed.
/// </summary>
public class ZstdSharpCompressor : ICompressor
{
    private readonly int compressionLevel;

    /// <summary>
    /// Initializes a new instance of the <see cref="ZstdSharpCompressor"/> class.
    /// </summary>
    /// <param name="compressionLevel">The Zstd compression level (1-22). Defaults to 3 (fast).</param>
    public ZstdSharpCompressor(int compressionLevel = 3)
    {
        this.compressionLevel = compressionLevel;
    }

    /// <inheritdoc/>
    public byte[] Compress(byte[] data)
    {
        using var compressor = new Compressor(compressionLevel);

        return compressor.Wrap(data).ToArray();
    }

    /// <inheritdoc/>
    public byte[] Decompress(byte[] compressedData)
    {
        using var decompressor = new Decompressor();

        return decompressor.Unwrap(compressedData).ToArray();
    }
}

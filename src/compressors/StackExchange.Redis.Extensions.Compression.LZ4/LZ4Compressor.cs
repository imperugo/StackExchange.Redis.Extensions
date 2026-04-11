// Copyright (c) Ugo Lattanzi.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

using K4os.Compression.LZ4;

namespace StackExchange.Redis.Extensions.Core;

/// <summary>
/// An <see cref="ICompressor"/> implementation using LZ4 compression.
/// Extremely fast compression and decompression — ideal for caching scenarios
/// where latency matters more than compression ratio.
/// </summary>
public class LZ4Compressor : ICompressor
{
    private readonly LZ4Level level;

    /// <summary>
    /// Initializes a new instance of the <see cref="LZ4Compressor"/> class.
    /// </summary>
    /// <param name="level">The LZ4 compression level. Defaults to <see cref="LZ4Level.L00_FAST"/>.</param>
    public LZ4Compressor(LZ4Level level = LZ4Level.L00_FAST)
    {
        this.level = level;
    }

    /// <inheritdoc/>
    public byte[] Compress(byte[] data) =>
        LZ4Pickler.Pickle(data, level);

    /// <inheritdoc/>
    public byte[] Decompress(byte[] compressedData) =>
        LZ4Pickler.Unpickle(compressedData);
}

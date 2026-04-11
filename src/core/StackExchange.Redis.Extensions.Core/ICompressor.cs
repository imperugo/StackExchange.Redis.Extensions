// Copyright (c) Ugo Lattanzi.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

namespace StackExchange.Redis.Extensions.Core;

/// <summary>
/// Contract for compression implementations used by <see cref="CompressedSerializer"/>.
/// </summary>
public interface ICompressor
{
    /// <summary>
    /// Compresses the specified data.
    /// </summary>
    /// <param name="data">The uncompressed data.</param>
    /// <returns>The compressed data.</returns>
    byte[] Compress(byte[] data);

    /// <summary>
    /// Decompresses the specified data.
    /// </summary>
    /// <param name="compressedData">The compressed data.</param>
    /// <returns>The decompressed data.</returns>
    byte[] Decompress(byte[] compressedData);
}

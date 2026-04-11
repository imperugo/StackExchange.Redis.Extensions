// Copyright (c) Ugo Lattanzi.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

using System.IO;
using System.IO.Compression;

namespace StackExchange.Redis.Extensions.Core;

/// <summary>
/// An <see cref="ICompressor"/> implementation using Brotli compression.
/// Higher compression ratio than GZip, especially for text-like data. No external dependencies.
/// </summary>
public class BrotliCompressor : ICompressor
{
    private readonly CompressionLevel compressionLevel;

    /// <summary>
    /// Initializes a new instance of the <see cref="BrotliCompressor"/> class.
    /// </summary>
    /// <param name="compressionLevel">The compression level to use. Defaults to <see cref="CompressionLevel.Fastest"/>.</param>
    public BrotliCompressor(CompressionLevel compressionLevel = CompressionLevel.Fastest)
    {
        this.compressionLevel = compressionLevel;
    }

    /// <inheritdoc/>
    public byte[] Compress(byte[] data)
    {
        using var output = new MemoryStream();

        using (var brotli = new BrotliStream(output, compressionLevel))
            brotli.Write(data, 0, data.Length);

        return output.ToArray();
    }

    /// <inheritdoc/>
    public byte[] Decompress(byte[] compressedData)
    {
        using var input = new MemoryStream(compressedData);
        using var brotli = new BrotliStream(input, CompressionMode.Decompress);
        using var output = new MemoryStream();

        brotli.CopyTo(output);

        return output.ToArray();
    }
}

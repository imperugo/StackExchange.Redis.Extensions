// Copyright (c) Ugo Lattanzi.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

using System.IO;
using System.IO.Compression;

namespace StackExchange.Redis.Extensions.Core;

/// <summary>
/// An <see cref="ICompressor"/> implementation using GZip compression.
/// Good compression ratio, widely supported. No external dependencies.
/// </summary>
public class GZipCompressor : ICompressor
{
    private readonly CompressionLevel compressionLevel;

    /// <summary>
    /// Initializes a new instance of the <see cref="GZipCompressor"/> class.
    /// </summary>
    /// <param name="compressionLevel">The compression level to use. Defaults to <see cref="CompressionLevel.Fastest"/>.</param>
    public GZipCompressor(CompressionLevel compressionLevel = CompressionLevel.Fastest)
    {
        this.compressionLevel = compressionLevel;
    }

    /// <inheritdoc/>
    public byte[] Compress(byte[] data)
    {
        using var output = new MemoryStream();

        using (var gzip = new GZipStream(output, compressionLevel))
            gzip.Write(data, 0, data.Length);

        return output.ToArray();
    }

    /// <inheritdoc/>
    public byte[] Decompress(byte[] compressedData)
    {
        using var input = new MemoryStream(compressedData);
        using var gzip = new GZipStream(input, CompressionMode.Decompress);
        using var output = new MemoryStream();

        gzip.CopyTo(output);

        return output.ToArray();
    }
}

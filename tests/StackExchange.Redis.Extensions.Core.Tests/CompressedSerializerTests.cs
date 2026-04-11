// Copyright (c) Ugo Lattanzi.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

using System;
using System.IO;
using System.IO.Compression;

using StackExchange.Redis.Extensions.System.Text.Json;

using Xunit;

namespace StackExchange.Redis.Extensions.Core.Tests;

public class CompressedSerializerTests
{
    [Fact]
    public void RoundTrip_ShouldSerializeCompressDecompressDeserialize()
    {
        var inner = new SystemTextJsonSerializer();
        var compressor = new GZipTestCompressor();
        var sut = new CompressedSerializer(inner, compressor);

        var original = new TestPayload { Name = "test", Value = 42 };

        var compressed = sut.Serialize(original);
        var result = sut.Deserialize<TestPayload>(compressed);

        Assert.NotNull(result);
        Assert.Equal(original.Name, result.Name);
        Assert.Equal(original.Value, result.Value);
    }

    [Fact]
    public void Serialize_NullItem_ShouldReturnEmptyArray()
    {
        var inner = new SystemTextJsonSerializer();
        var compressor = new GZipTestCompressor();
        var sut = new CompressedSerializer(inner, compressor);

        var result = sut.Serialize<string>(null);

        Assert.Empty(result);
    }

    [Fact]
    public void Deserialize_NullOrEmpty_ShouldReturnDefault()
    {
        var inner = new SystemTextJsonSerializer();
        var compressor = new GZipTestCompressor();
        var sut = new CompressedSerializer(inner, compressor);

        Assert.Null(sut.Deserialize<string>(null));
        Assert.Null(sut.Deserialize<string>(Array.Empty<byte>()));
    }

    [Fact]
    public void CompressedData_ShouldBeSmallerThanRaw_ForLargePayloads()
    {
        var inner = new SystemTextJsonSerializer();
        var compressor = new GZipTestCompressor();
        var sut = new CompressedSerializer(inner, compressor);

        var largeString = new string('A', 10000);

        var raw = inner.Serialize(largeString);
        var compressed = sut.Serialize(largeString);

        Assert.True(compressed.Length < raw.Length, $"Compressed ({compressed.Length}) should be smaller than raw ({raw.Length})");
    }

    private sealed class TestPayload
    {
        public string? Name { get; set; }

        public int Value { get; set; }
    }

    private sealed class GZipTestCompressor : ICompressor
    {
        public byte[] Compress(byte[] data)
        {
            using var output = new MemoryStream();

            using (var gzip = new GZipStream(output, CompressionLevel.Fastest))
                gzip.Write(data, 0, data.Length);

            return output.ToArray();
        }

        public byte[] Decompress(byte[] compressedData)
        {
            using var input = new MemoryStream(compressedData);
            using var gzip = new GZipStream(input, CompressionMode.Decompress);
            using var output = new MemoryStream();

            gzip.CopyTo(output);

            return output.ToArray();
        }
    }
}

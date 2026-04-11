// Copyright (c) Ugo Lattanzi.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Text;

using StackExchange.Redis.Extensions.Core;
using StackExchange.Redis.Extensions.System.Text.Json;

using Xunit;

namespace StackExchange.Redis.Extensions.Compression.Tests;

public class CompressorTests
{
    public static IEnumerable<object[]> AllCompressors()
    {
        yield return ["GZip", new GZipCompressor()];
        yield return ["Brotli", new BrotliCompressor()];
        yield return ["LZ4", new LZ4Compressor()];
        yield return ["Snappier", new SnappierCompressor()];
        yield return ["ZstdSharp", new ZstdSharpCompressor()];
    }

    [Theory]
    [MemberData(nameof(AllCompressors))]
    public void RoundTrip_Bytes_ShouldBeIdentical(string _, ICompressor compressor)
    {
        var original = Encoding.UTF8.GetBytes("Hello, Redis compression test!");

        var compressed = compressor.Compress(original);
        var decompressed = compressor.Decompress(compressed);

        Assert.Equal(original, decompressed);
    }

    [Theory]
    [MemberData(nameof(AllCompressors))]
    public void RoundTrip_LargePayload_ShouldCompressAndDecompress(string _, ICompressor compressor)
    {
        var original = Encoding.UTF8.GetBytes(new string('A', 100_000));

        var compressed = compressor.Compress(original);
        var decompressed = compressor.Decompress(compressed);

        Assert.Equal(original, decompressed);
        Assert.True(compressed.Length < original.Length, $"Compressed ({compressed.Length}) should be smaller than original ({original.Length})");
    }

    [Theory]
    [MemberData(nameof(AllCompressors))]
    public void RoundTrip_EmptyArray_ShouldHandleGracefully(string _, ICompressor compressor)
    {
        var original = Array.Empty<byte>();

        var compressed = compressor.Compress(original);
        var decompressed = compressor.Decompress(compressed);

        Assert.Empty(decompressed);
    }

    [Theory]
    [MemberData(nameof(AllCompressors))]
    public void CompressedSerializer_RoundTrip_WithAllCompressors(string _, ICompressor compressor)
    {
        var inner = new SystemTextJsonSerializer();
        var sut = new CompressedSerializer(inner, compressor);

        var original = new TestPayload { Name = "test-compressor", Value = 42 };

        var compressed = sut.Serialize(original);
        var result = sut.Deserialize<TestPayload>(compressed);

        Assert.NotNull(result);
        Assert.Equal(original.Name, result.Name);
        Assert.Equal(original.Value, result.Value);
    }

    [Theory]
    [MemberData(nameof(AllCompressors))]
    public void CompressedSerializer_NullValue_ReturnsDefault(string _, ICompressor compressor)
    {
        var inner = new SystemTextJsonSerializer();
        var sut = new CompressedSerializer(inner, compressor);

        var serialized = sut.Serialize<string>(null);

        Assert.Empty(serialized);
        Assert.Null(sut.Deserialize<string>(null));
        Assert.Null(sut.Deserialize<string>(Array.Empty<byte>()));
    }

    [Fact]
    public void CompressedSerializer_DoubleWrap_ShouldThrow()
    {
        var inner = new SystemTextJsonSerializer();
        var compressed = new CompressedSerializer(inner, new GZipCompressor());

        Assert.Throws<ArgumentException>(() => new CompressedSerializer(compressed, new GZipCompressor()));
    }

    public class TestPayload
    {
        public string? Name { get; set; }

        public int Value { get; set; }
    }
}

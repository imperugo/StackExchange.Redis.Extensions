// Copyright (c) Ugo Lattanzi.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

using System;
using System.Threading.Tasks;

using Xunit;

namespace StackExchange.Redis.Extensions.Core.Tests;

public abstract partial class CacheClientTestBase
{
    [Fact]
    public async Task StreamAdd_Typed_ShouldAddAndRead_Async()
    {
        var key = Guid.NewGuid().ToString();

        var messageId = await Sut.GetDefaultDatabase().StreamAddAsync(key, "payload", "test-value");

        Assert.False(messageId.IsNull);

        var entries = await Sut.GetDefaultDatabase().StreamRangeAsync(key);

        Assert.Single(entries);
        Assert.Equal(messageId, entries[0].Id);
    }

    [Fact]
    public async Task StreamAdd_RawEntries_ShouldAddMultipleFields_Async()
    {
        var key = Guid.NewGuid().ToString();

        var entries = new[]
        {
            new NameValueEntry("field1", "value1"),
            new NameValueEntry("field2", "value2"),
        };

        var messageId = await Sut.GetDefaultDatabase().StreamAddAsync(key, entries);

        Assert.False(messageId.IsNull);

        var result = await Sut.GetDefaultDatabase().StreamRangeAsync(key);

        Assert.Single(result);
        Assert.Equal(2, result[0].Values.Length);
    }

    [Fact]
    public async Task StreamLength_ShouldReturnCorrectCount_Async()
    {
        var key = Guid.NewGuid().ToString();

        await Sut.GetDefaultDatabase().StreamAddAsync(key, "f", "v1");
        await Sut.GetDefaultDatabase().StreamAddAsync(key, "f", "v2");
        await Sut.GetDefaultDatabase().StreamAddAsync(key, "f", "v3");

        var length = await Sut.GetDefaultDatabase().StreamLengthAsync(key);

        Assert.Equal(3, length);
    }

    [Fact]
    public async Task StreamTrim_ShouldReduceLength_Async()
    {
        var key = Guid.NewGuid().ToString();

        for (var i = 0; i < 10; i++)
            await Sut.GetDefaultDatabase().StreamAddAsync(key, "f", $"v{i}");

        var trimmed = await Sut.GetDefaultDatabase().StreamTrimAsync(key, 5);

        Assert.True(trimmed >= 5);

        var length = await Sut.GetDefaultDatabase().StreamLengthAsync(key);

        Assert.True(length <= 5);
    }

    [Fact]
    public async Task StreamDelete_ShouldRemoveEntry_Async()
    {
        var key = Guid.NewGuid().ToString();

        var id = await Sut.GetDefaultDatabase().StreamAddAsync(key, "f", "v");

        var deleted = await Sut.GetDefaultDatabase().StreamDeleteAsync(key, new[] { id.ToString()! });

        Assert.Equal(1, deleted);
    }

    [Fact]
    public async Task StreamRead_ShouldReadFromPosition_Async()
    {
        var key = Guid.NewGuid().ToString();

        await Sut.GetDefaultDatabase().StreamAddAsync(key, "f", "v1");
        await Sut.GetDefaultDatabase().StreamAddAsync(key, "f", "v2");

        var entries = await Sut.GetDefaultDatabase().StreamReadAsync(key, "0-0");

        Assert.Equal(2, entries.Length);
    }

    [Fact]
    public async Task StreamRange_Descending_ShouldReverseOrder_Async()
    {
        var key = Guid.NewGuid().ToString();

        var id1 = await Sut.GetDefaultDatabase().StreamAddAsync(key, "f", "v1");
        var id2 = await Sut.GetDefaultDatabase().StreamAddAsync(key, "f", "v2");

        var entries = await Sut.GetDefaultDatabase().StreamRangeAsync(key, messageOrder: Order.Descending);

        Assert.Equal(2, entries.Length);
        Assert.Equal(id2, entries[0].Id);
        Assert.Equal(id1, entries[1].Id);
    }

    [Fact]
    public async Task StreamConsumerGroup_FullWorkflow_Async()
    {
        var key = Guid.NewGuid().ToString();
        const string group = "test-group";
        const string consumer = "test-consumer";

        // Add some messages first
        await Sut.GetDefaultDatabase().StreamAddAsync(key, "f", "v1");
        await Sut.GetDefaultDatabase().StreamAddAsync(key, "f", "v2");

        // Create group from beginning
        var created = await Sut.GetDefaultDatabase().StreamCreateConsumerGroupAsync(key, group, "0-0");

        Assert.True(created);

        // Read as consumer
        var entries = await Sut.GetDefaultDatabase().StreamReadGroupAsync(key, group, consumer, ">", count: 10);

        Assert.Equal(2, entries.Length);

        // Acknowledge
        var acked = await Sut.GetDefaultDatabase().StreamAcknowledgeAsync(key, group, entries[0].Id!);

        Assert.Equal(1, acked);

        // Check pending (1 should still be pending)
        var pending = await Sut.GetDefaultDatabase().StreamPendingAsync(key, group);

        Assert.Equal(1, pending.PendingMessageCount);

        // Acknowledge the second one
        await Sut.GetDefaultDatabase().StreamAcknowledgeAsync(key, group, new[] { entries[1].Id.ToString()! });

        // Delete consumer
        var pendingRemoved = await Sut.GetDefaultDatabase().StreamDeleteConsumerAsync(key, group, consumer);

        Assert.Equal(0, pendingRemoved);

        // Delete group
        var deleted = await Sut.GetDefaultDatabase().StreamDeleteConsumerGroupAsync(key, group);

        Assert.True(deleted);
    }
}

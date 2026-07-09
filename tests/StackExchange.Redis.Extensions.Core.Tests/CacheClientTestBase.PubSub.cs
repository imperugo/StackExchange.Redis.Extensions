// Copyright (c) Ugo Lattanzi.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

using System;
using System.Threading;
using System.Threading.Tasks;

using Xunit;

namespace StackExchange.Redis.Extensions.Core.Tests;

public abstract partial class CacheClientTestBase
{
    [Fact]
    public async Task SubscribeAsync_ValidMessage_ShouldInvokeHandler_Async()
    {
        var channel = new RedisChannel(Guid.NewGuid().ToString(), RedisChannel.PatternMode.Literal);
        var received = new TaskCompletionSource<string?>();

        await Sut.GetDefaultDatabase().SubscribeAsync<string>(channel, msg =>
        {
            received.TrySetResult(msg);
            return Task.CompletedTask;
        });

        await Sut.GetDefaultDatabase().PublishAsync(channel, "hello");

        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
        cts.Token.Register(() => received.TrySetCanceled());

        var result = await received.Task;

        Assert.Equal("hello", result);

        await Sut.GetDefaultDatabase().UnsubscribeAllAsync();
    }

    [Fact]
    public async Task UnsubscribeAllAsync_ShouldStopReceivingMessages_Async()
    {
        var channel = new RedisChannel(Guid.NewGuid().ToString(), RedisChannel.PatternMode.Literal);
        var received = new TaskCompletionSource<string?>();

        await Sut.GetDefaultDatabase().SubscribeAsync<string>(channel, msg =>
        {
            received.TrySetResult(msg);
            return Task.CompletedTask;
        });

        await Sut.GetDefaultDatabase().PublishAsync(channel, "before");

        using var cts1 = new CancellationTokenSource(TimeSpan.FromSeconds(5));
        cts1.Token.Register(() => received.TrySetCanceled());
        Assert.Equal("before", await received.Task);

        await Sut.GetDefaultDatabase().UnsubscribeAllAsync();

        var receivedAfter = new TaskCompletionSource<string?>();

        await Sut.GetDefaultDatabase().SubscribeAsync<string>(channel, msg =>
        {
            receivedAfter.TrySetResult(msg);
            return Task.CompletedTask;
        });

        await Sut.GetDefaultDatabase().PublishAsync(channel, "after");

        using var cts2 = new CancellationTokenSource(TimeSpan.FromSeconds(5));
        cts2.Token.Register(() => receivedAfter.TrySetCanceled());

        Assert.Equal("after", await receivedAfter.Task);

        await Sut.GetDefaultDatabase().UnsubscribeAllAsync();
    }

    [Fact]
    public async Task UnsubscribeAsync_ShouldStopReceivingMessages_Async()
    {
        var channel = new RedisChannel(Guid.NewGuid().ToString(), RedisChannel.PatternMode.Literal);
        var messageCount = 0;
        var firstReceived = new TaskCompletionSource<bool>();

        Func<string?, Task> handler = msg =>
        {
            Interlocked.Increment(ref messageCount);
            firstReceived.TrySetResult(true);
            return Task.CompletedTask;
        };

        await Sut.GetDefaultDatabase().SubscribeAsync(channel, handler);

        await Sut.GetDefaultDatabase().PublishAsync(channel, "msg1");

        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
        cts.Token.Register(() => firstReceived.TrySetCanceled());
        await firstReceived.Task;

        await Sut.GetDefaultDatabase().UnsubscribeAsync<string>(channel, handler);

        await Sut.GetDefaultDatabase().PublishAsync(channel, "msg2");
        await Task.Delay(500);

        Assert.Equal(1, messageCount);
    }

    [Fact]
    public async Task SubscribeAsync_HandlerThrows_ShouldNotCrashProcess_Async()
    {
        var channel = new RedisChannel(Guid.NewGuid().ToString(), RedisChannel.PatternMode.Literal);
        var secondMessageReceived = new TaskCompletionSource<bool>();
        var firstHandlerCompleted = new TaskCompletionSource<bool>();
        var callCount = 0;

        await Sut.GetDefaultDatabase().SubscribeAsync<string>(channel, _ =>
        {
            var count = Interlocked.Increment(ref callCount);

            if (count == 1)
            {
                firstHandlerCompleted.TrySetResult(true);
                throw new InvalidOperationException("Simulated handler failure");
            }

            secondMessageReceived.TrySetResult(true);
            return Task.CompletedTask;
        });

        // First message — handler throws, should be logged not crash
        await Sut.GetDefaultDatabase().PublishAsync(channel, "msg1");

        // Wait for first handler to complete before sending second message
        using var cts1 = new CancellationTokenSource(TimeSpan.FromSeconds(5));
        cts1.Token.Register(() => firstHandlerCompleted.TrySetCanceled());
        await firstHandlerCompleted.Task;

        // Second message — handler should still be active
        await Sut.GetDefaultDatabase().PublishAsync(channel, "msg2");

        using var cts2 = new CancellationTokenSource(TimeSpan.FromSeconds(5));
        cts2.Token.Register(() => secondMessageReceived.TrySetCanceled());

        var result = await secondMessageReceived.Task;

        Assert.True(result, "Second message should have been received even after first handler threw");
        Assert.True(callCount >= 2);

        await Sut.GetDefaultDatabase().UnsubscribeAllAsync();
    }
}

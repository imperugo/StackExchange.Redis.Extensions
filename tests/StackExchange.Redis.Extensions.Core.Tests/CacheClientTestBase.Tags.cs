// Copyright (c) Ugo Lattanzi.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using StackExchange.Redis.Extensions.Core.Helpers;

using Xunit;

namespace StackExchange.Redis.Extensions.Core.Tests;

public abstract partial class CacheClientTestBase
{
    [Fact]
    [Trait("Category", "Tags")]
    public async Task AddAsync_WithTags_ShouldAdd_Async()
    {
        const string testKey = "test_key";
        const string testValue = "test_value";
        var testClass = new Helpers.TestClass<string>(testKey, testValue);
        var testTags = new HashSet<string> { "test_tag" };

        var addResult = await Sut.GetDefaultDatabase().AddAsync(testKey, testClass, tags: testTags);

        Assert.True(addResult);
    }

    [Fact]
    [Trait("Category", "Tags")]
    public async Task AddAsync_WithTags_TagExists_Async()
    {
        const string testKey = "test_key";
        const string testValue = "test_value";
        var testClass = new Helpers.TestClass<string>(testKey, testValue);
        const string testTag = "test_tag";

        await Sut.GetDefaultDatabase().AddAsync(testKey, testClass, tags: [testTag]);

        var tagExists = await db.KeyExistsAsync(TagHelper.GenerateTagKey(testTag));

        Assert.True(tagExists);
    }

    [Fact]
    [Trait("Category", "Tags")]
    public async Task AddAsync_WithTags_CorrectTaggedKey_Async()
    {
        const string testKey = "test_key";
        const string testValue = "test_value";
        var testClass = new Helpers.TestClass<string>(testKey, testValue);
        const string testTag = "test_tag";

        await Sut.GetDefaultDatabase().AddAsync(testKey, testClass, tags: [testTag]);

        var tags = await db.SetMembersAsync(TagHelper.GenerateTagKey(testTag));
        var deserialized = tags.Length > 0 ? serializer.Deserialize<string>(tags[0]!) : string.Empty;

        Assert.Equal(testKey, deserialized);
    }

    [Fact]
    [Trait("Category", "Tags")]
    public async Task AddAsyncTimeSpan_WithTags_ShouldAdd_Async()
    {
        const string testKey = "test_key";
        const string testValue = "test_value";
        var testClass = new Helpers.TestClass<string>(testKey, testValue);
        var testTags = new HashSet<string> { "test_tag" };

        var addResult = await Sut.GetDefaultDatabase().AddAsync(testKey, testClass, TimeSpan.FromSeconds(1), tags: testTags);

        Assert.True(addResult);
    }

    [Fact]
    [Trait("Category", "Tags")]
    public async Task AddAsyncTimeSpan_WithTags_TagExists_Async()
    {
        const string testKey = "test_key";
        const string testValue = "test_value";
        var testClass = new Helpers.TestClass<string>(testKey, testValue);
        const string testTag = "test_tag";

        await Sut.GetDefaultDatabase().AddAsync(testKey, testClass, TimeSpan.FromSeconds(1), tags: [testTag]);

        var tagExists = await db.KeyExistsAsync(TagHelper.GenerateTagKey(testTag));

        Assert.True(tagExists);
    }

    [Fact]
    [Trait("Category", "Tags")]
    public async Task AddAsyncTimeSpan_WithTags_CorrectTaggedKey_Async()
    {
        const string testKey = "test_key";
        const string testValue = "test_value";
        var testClass = new Helpers.TestClass<string>(testKey, testValue);
        const string testTag = "test_tag";

        await Sut.GetDefaultDatabase().AddAsync(testKey, testClass, TimeSpan.FromSeconds(1), tags: [testTag]);

        var tags = await db.SetMembersAsync(TagHelper.GenerateTagKey(testTag));
        var deserialized = serializer.Deserialize<string>(tags[0]!);

        Assert.Equal(testKey, deserialized);
    }

    [Fact]
    [Trait("Category", "Tags")]
    public async Task AddAsyncDateTimeOffset_WithTags_ShouldAdd_Async()
    {
        const string testKey = "test_key";
        const string testValue = "test_value";
        var testClass = new Helpers.TestClass<string>(testKey, testValue);
        var testTags = new HashSet<string> { "test_tag" };

        var addResult = await Sut.GetDefaultDatabase().AddAsync(testKey, testClass, DateTimeOffset.UtcNow, tags: testTags);

        Assert.True(addResult);
    }

    [Fact]
    [Trait("Category", "Tags")]
    public async Task AddAsyncDateTimeOffset_WithTags_TagExists_Async()
    {
        const string testKey = "test_key";
        const string testValue = "test_value";
        var testClass = new Helpers.TestClass<string>(testKey, testValue);
        const string testTag = "test_tag";

        await Sut.GetDefaultDatabase().AddAsync(testKey, testClass, DateTimeOffset.UtcNow, tags: [testTag]);

        var tagExists = await db.KeyExistsAsync(TagHelper.GenerateTagKey(testTag));

        Assert.True(tagExists);
    }

    [Fact]
    [Trait("Category", "Tags")]
    public async Task AddAsyncDateTimeOffset_WithTags_CorrectTaggedKey_Async()
    {
        const string testKey = "test_key";
        const string testValue = "test_value";
        var testClass = new Helpers.TestClass<string>(testKey, testValue);
        const string testTag = "test_tag";

        await Sut.GetDefaultDatabase().AddAsync(testKey, testClass, DateTimeOffset.UtcNow, tags: [testTag]);

        var tags = await db.SetMembersAsync(TagHelper.GenerateTagKey(testTag));
        var deserialized = serializer.Deserialize<string>(tags[0]!);

        Assert.Equal(testKey, deserialized);
    }

    [Fact]
    [Trait("Category", "Tags")]
    public async Task GetByTagAsync_ShouldReturnSomeValues_Async()
    {
        const string testKey = "test_key";
        const string testValue = "test_value";
        var testClass = new Helpers.TestClass<string>(testKey, testValue);
        const string testTag = "test_tag";

        await Sut.GetDefaultDatabase().AddAsync(testKey, testClass, tags: [testTag]);

        var result = await Sut.GetDefaultDatabase().GetByTagAsync<Helpers.TestClass<string>>(testTag);

        Assert.Single(result);
    }

    [Fact]
    [Trait("Category", "Tags")]
    public async Task GetByTagAsync_ShouldReturnCorrectValue_Async()
    {
        const string testKey = "test_key";
        const string testValue = "test_value";
        var testClass = new Helpers.TestClass<string>(testKey, testValue);
        const string testTag = "test_tag";

        await Sut.GetDefaultDatabase().AddAsync(testKey, testClass, tags: [testTag]);

        var result = await Sut.GetDefaultDatabase().GetByTagAsync<Helpers.TestClass<string>>(testTag);

        Assert.Equal(testClass, result.First());
    }

    [Fact]
    [Trait("Category", "Tags")]
    public async Task RemoveByTagAsync_ShouldReturnZero_Async()
    {
        const string testTag = "test_tag";

        var result = await Sut.GetDefaultDatabase().RemoveByTagAsync(testTag);

        Assert.Equal(0, result);
    }

    [Fact]
    [Trait("Category", "Tags")]
    public async Task RemoveByTagAsync_ShouldReturnOneDeletedValue_Async()
    {
        const string testKey = "test_key";
        const string testValue = "test_value";
        var testClass = new Helpers.TestClass<string>(testKey, testValue);
        const string testTag = "test_tag";

        await Sut.GetDefaultDatabase().AddAsync(testKey, testClass, tags: [testTag]);

        var result = await Sut.GetDefaultDatabase().RemoveByTagAsync(testTag);

        Assert.Equal(1, result);
    }
}

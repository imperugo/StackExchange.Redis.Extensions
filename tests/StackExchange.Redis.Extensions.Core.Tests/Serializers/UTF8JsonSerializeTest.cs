// Copyright (c) Ugo Lattanzi.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

using StackExchange.Redis.Extensions.Utf8Json;

namespace StackExchange.Redis.Extensions.Core.Tests.Serializers;

public class UTF8JsonSerializeTest() : CacheClientTestBase(new Utf8JsonSerializer());

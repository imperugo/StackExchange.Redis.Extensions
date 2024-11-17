// Copyright (c) Ugo Lattanzi.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

using StackExchange.Redis.Extensions.MsgPack;

namespace StackExchange.Redis.Extensions.Core.Tests.Serializers;

public class MessagePackSerializerTest() : CacheClientTestBase(new MsgPackObjectSerializer());

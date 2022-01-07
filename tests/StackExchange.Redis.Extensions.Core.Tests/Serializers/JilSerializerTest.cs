// Copyright (c) Ugo Lattanzi.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

using Jil;

using StackExchange.Redis.Extensions.Jil;

namespace StackExchange.Redis.Extensions.Core.Tests.Serializers;

public class JilSerializerTest : CacheClientTestBase
{
    public JilSerializerTest()
        : base(new JilSerializer(new(
            false,
            true,
            false,
            DateTimeFormat.ISO8601,
            true,
            UnspecifiedDateTimeKindBehavior.IsLocal)))
    {
    }
}

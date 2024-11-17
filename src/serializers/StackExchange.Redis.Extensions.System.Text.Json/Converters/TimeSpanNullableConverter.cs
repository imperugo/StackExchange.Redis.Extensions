// Copyright (c) Ugo Lattanzi.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

using System;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace StackExchange.Redis.Extensions.System.Text.Json.Converters;

internal sealed class TimeSpanNullableConverter : JsonConverter<TimeSpan?>
{
    public override TimeSpan? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var value = reader.GetString();

        if (value?.Length > 0)
            return TimeSpan.Parse(value, CultureInfo.InvariantCulture);

        return null;
    }

    public override void Write(Utf8JsonWriter writer, TimeSpan? value, JsonSerializerOptions options)
    {
        if (value != null)
            writer.WriteStringValue(value.ToString());
    }
}

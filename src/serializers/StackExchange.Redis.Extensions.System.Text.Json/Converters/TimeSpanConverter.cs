// Copyright (c) Ugo Lattanzi.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

using System;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace StackExchange.Redis.Extensions.System.Text.Json.Converters;

internal sealed class TimeSpanConverter : JsonConverter<TimeSpan>
{
    public override TimeSpan Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return TimeSpan.Parse(reader.GetString()!, CultureInfo.InvariantCulture);
    }

    public override void Write(Utf8JsonWriter writer, TimeSpan value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString());
    }
}

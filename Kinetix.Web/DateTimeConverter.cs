using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Kinetix.Web;

public class DateTimeConverter : JsonConverter<DateTime>
{
    /// <inheritdoc />
    public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return DateTimeOffset.Parse(reader.GetString()!, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal).UtcDateTime;
    }

    /// <inheritdoc />
    public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ssZ"));
    }
}

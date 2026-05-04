using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Kinetix.Web;

/// <summary>
/// Convertisseur JSON pour s'assurer de la bonne sérialisation des TimeSpans.
/// </summary>
public class TimeSpanConverter : JsonConverter<TimeSpan>
{
    /// <inheritdoc />
    public override TimeSpan Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return TimeSpan.ParseExact(reader.GetString()!, "c", CultureInfo.InvariantCulture);
    }

    /// <inheritdoc />
    public override void Write(Utf8JsonWriter writer, TimeSpan value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString("c", CultureInfo.InvariantCulture));
    }
}

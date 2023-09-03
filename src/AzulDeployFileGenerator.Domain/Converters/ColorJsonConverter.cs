namespace AzulDeployFileGenerator.Domain.Converters;

public sealed class ColorJsonConverter : JsonConverter<Color>
{
    public override void WriteJson(JsonWriter writer, Color value, JsonSerializer serializer)
    {
        writer.WriteValue($"#{value.R:X2}{value.G:X2}{value.B:X2}");
    }

    public override Color ReadJson(JsonReader reader, Type objectType, Color existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        string hexColor = (string)reader.Value;
        return ColorTranslator.FromHtml(hexColor);
    }
}

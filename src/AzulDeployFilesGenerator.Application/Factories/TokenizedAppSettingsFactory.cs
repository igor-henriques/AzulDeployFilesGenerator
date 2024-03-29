﻿namespace AzulDeployFilesGenerator.Application.Factories;

/// <summary>
/// Provides functionality to tokenize AppSettings for configuration.
/// </summary>
internal sealed class TokenizedAppSettingsFactory : ITokenizedAppSettingsFactory
{
    /// <summary>
    /// Builds a string representation of tokenized AppSettings.
    /// </summary>
    /// <param name="appSettings">The AppSettings object to tokenize.</param>
    /// <param name="cancellationToken">Cancellation token for async operation.</param>
    /// <returns>String representation of tokenized AppSettings.</returns>
    public string BuildTokenizedAppSettingsAsync(AppSettings appSettings, CancellationToken cancellationToken = default)
    {
        return TokenizeAppSettings(appSettings).ToString();
    }

    /// <summary>
    /// Tokenizes the extra properties in the AppSettings object.
    /// </summary>
    /// <param name="properties">Dictionary of extra properties.</param>
    /// <param name="parentKey">Optional parent key for nested objects.</param>
    /// <returns>A JObject with tokenized extra properties.</returns>
    private static JObject TokenizeFromExtraProperties(Dictionary<string, JToken> properties, string parentKey = "")
    {
        JObject result = new();

        foreach (var item in properties)
        {
            string key = string.IsNullOrEmpty(parentKey) ? item.Key : parentKey + "." + item.Key;

            if (item.Value is JObject childObject)
            {
                // Chamada recursiva para lidar com objetos aninhados
                JObject childResult = TokenizeFromExtraProperties(childObject.ToObject<Dictionary<string, JToken>>(), key);
                result.Add(item.Key, childResult); // Adiciona o objeto filho ao pai
            }
            else
            {
                result.Add(item.Key, "${" + key + "}");
            }
        }

        return result;
    }

    /// <summary>
    /// Tokenizes the AppSettings object.
    /// </summary>
    /// <param name="obj">Object to tokenize.</param>
    /// <param name="path">Path for nested properties.</param>
    /// <returns>A JObject with tokenized AppSettings.</returns>
    private static JObject TokenizeAppSettings(object obj, string path = "")
    {
        if (obj is null)
        {
            return default;
        }

        if (obj.GetType().GetCustomAttribute<IgnoreDockerTokenization>() != null)
        {
            return JToken.FromObject(obj) as JObject;
        }

        JObject result = new();
        foreach (PropertyInfo prop in obj.GetType().GetProperties())
        {
            if (!prop.CanRead || prop.GetIndexParameters().Length > 0)
            {
                continue;
            }

            if (prop.GetCustomAttribute<JsonIgnoreAttribute>() is not null)
            {
                continue;
            }

            if (prop.Name == nameof(AppSettings.ExtraProperties))
            {
                if (prop.GetValue(obj) is Dictionary<string, JToken> extraProperties)
                {
                    var tokenizedExtraProperties = TokenizeFromExtraProperties(extraProperties);
                    result.Merge(tokenizedExtraProperties);
                }

                continue;
            }

            JsonPropertyAttribute jsonAttribute = prop.GetCustomAttribute<JsonPropertyAttribute>();
            string jsonPropName = jsonAttribute?.PropertyName ?? prop.Name;
            object value = prop.GetValue(obj);

            var jsonPropertyAttribute = prop.GetCustomAttribute<JsonPropertyAttribute>();
            if (jsonPropertyAttribute != null
                && jsonPropertyAttribute.NullValueHandling == NullValueHandling.Ignore
                && value is string or null
                && string.IsNullOrWhiteSpace(value?.ToString()))
            {
                continue;
            }

            if (prop.GetCustomAttribute<IgnoreDockerTokenization>() != null)
            {
                result.Add(jsonPropName, JToken.FromObject(value));
                continue;
            }

            if (jsonPropName.Equals("id", StringComparison.OrdinalIgnoreCase)
                || (jsonPropName.Equals("key", StringComparison.OrdinalIgnoreCase)))
            {
                result.Add(jsonPropName, JToken.FromObject(value));
                path += value.ToString() + ".";
            }
            else if (value is string || value is ValueType)
            {
                string tokenString = path + jsonPropName;

                if (obj.GetType().Name is "Parameter"
                    && jsonPropName.Equals("Value", StringComparison.OrdinalIgnoreCase))
                {
                    tokenString = tokenString[..tokenString.LastIndexOf('.')];
                }

                result.Add(jsonPropName, "${" + tokenString + "}");
            }
            else if (value is IList list)
            {
                JArray array = new();

                for (int i = 0; i < list.Count; i++)
                {
                    object listItem = list[i];
                    array.Add(TokenizeAppSettings(listItem, path + jsonPropName + "."));
                }

                result.Add(jsonPropName, array);
            }
            else if (prop.PropertyType.IsClass)
            {
                result.Add(jsonPropName, TokenizeAppSettings(value, path + jsonPropName + "."));
            }
        }

        return result;
    }
}

namespace AzulDeployFileGenerator.Domain.Models.Options;

internal class SettingsAccessor
{
    public static SettingsAccessor Instance = GetInstance();

    public string AzulAcrName { get; init; }
    public string OnlineAcrName { get; init; }

    public SettingsAccessor(string azulAcrName, string onlineAcrName)
    {
        AzulAcrName = azulAcrName;
        OnlineAcrName = onlineAcrName;
    }

    private static SettingsAccessor GetInstance()
    {
        if (Instance == null)
        {
            string settingsJson = File.ReadAllText("./appsettings.json");
            var settings = JObject.Parse(settingsJson);

            string azulAcrName = settings["ApplicationDefaultsOptions"]?["AzulAcrName"]?.ToString() ?? "";
            string onlineAcrName = settings["ApplicationDefaultsOptions"]?["OnlineAcrName"]?.ToString() ?? "";

            Instance = new SettingsAccessor(azulAcrName, onlineAcrName);
        }

        return Instance;
    }
}

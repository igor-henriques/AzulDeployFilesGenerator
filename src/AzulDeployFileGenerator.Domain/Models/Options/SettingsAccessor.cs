namespace AzulDeployFileGenerator.Domain.Models.Options;

internal class SettingsAccessor
{
    public static SettingsAccessor Instance = GetInstance();

    public string AzulAcrName { get; init; }
    public string OnlineAcrName { get; init; }

    /// <summary>
    /// Const for test purpose
    /// </summary>
    private const string AZUL_ACR_NAME = "acrdevopsbr";

    /// <summary>
    /// Const for test purpose
    /// </summary>
    private const string ONLINE_ACR_NAME = "isabko";

    public SettingsAccessor(string azulAcrName, string onlineAcrName)
    {
        AzulAcrName = azulAcrName;
        OnlineAcrName = onlineAcrName;
    }

    private static SettingsAccessor GetInstance()
    {
        if (Instance == null)
        {
            //appsettings wont exist when this method gets called by the tests
            if (!File.Exists("./appsettings.json"))
            {
                Instance = new SettingsAccessor(AZUL_ACR_NAME, ONLINE_ACR_NAME);
                return Instance;
            }

            string settingsJson = File.ReadAllText("./appsettings.json");
            var settings = JObject.Parse(settingsJson);

            string azulAcrName = settings["ApplicationDefaultsOptions"]?["AzulAcrName"]?.ToString() ?? "";
            string onlineAcrName = settings["ApplicationDefaultsOptions"]?["OnlineAcrName"]?.ToString() ?? "";

            Instance = new SettingsAccessor(azulAcrName, onlineAcrName);
        }

        return Instance;
    }
}

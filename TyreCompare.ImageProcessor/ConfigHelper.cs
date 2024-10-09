using Microsoft.Extensions.Configuration;

namespace TyreCompare.ImageProcessor;

public static class ConfigHelper
{
    private static IConfiguration Configuration;

    static ConfigHelper()
    {
        var configurationBuilder = new ConfigurationBuilder().AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
        Configuration = configurationBuilder.Build();
    }

    public static string GetConnectionString()
    {
        string cs = GetConfigurationValue("ConnectionStrings", "HostServerCS", "pa$$word");
        return cs;
    }

    public static string GetImageContainerCS()
    {
        var settingValue = GetConfigurationValue("AppSettings", "ImageContainerCS", "pa$$word");
        var imageContainerCS = Convert.ToString(settingValue);
        return imageContainerCS;
    }

    public static string GetLogFolderPath()
    {
        string destinationPath = string.Empty;
        var logFolderName = Convert.ToString(Configuration["AppSettings:LogFolderName"]);
        destinationPath = Path.Combine(Directory.GetCurrentDirectory(), logFolderName);

        if (!Directory.Exists(destinationPath))
        { Directory.CreateDirectory(destinationPath); }

        return destinationPath;
    }

    public static string GetImageFolderPath()
    {
        bool useTempPath = true;
        string destinationPath = string.Empty;

        useTempPath = Convert.ToBoolean(Configuration["AppSettings:UseImageTempPath"]);

        if (useTempPath)
        { destinationPath = Path.GetTempPath(); }
        else
        {
            var imageFolderName = Convert.ToString(Configuration["AppSettings:ImageFolderName"]);
            destinationPath = Path.Combine(Directory.GetCurrentDirectory(), imageFolderName);
        }

        if (!Directory.Exists(destinationPath))
        { Directory.CreateDirectory(destinationPath); }

        return destinationPath;
    }

    public static string GetConfigurationValue(string group, string key, string password)
    {
        if (string.IsNullOrWhiteSpace(key) || string.IsNullOrWhiteSpace(password) || !password.Equals("pa$$word"))
        { return string.Empty; }

        var value = Convert.ToString(Configuration[$"{group}:{key}"]);
        return value;
    }
}

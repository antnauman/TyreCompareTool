using Microsoft.Extensions.Configuration;

namespace TyreCompare.BCL;

public static class ConfigHelper
{
    private static IConfiguration Configuration;

    public static bool IsSaveImagesInFolder
    {
        get
        {
            var settingValue = ConfigHelper.GetConfigurationValue("AppSettings", "SaveImageInFolder", "pa$$word");
            return Convert.ToBoolean(settingValue);
        }
    }

    public static bool IsSaveImagesInContainer
    {
        get
        {
            var settingValue = ConfigHelper.GetConfigurationValue("AppSettings", "SaveImageInContainer", "pa$$word");
            return Convert.ToBoolean(settingValue);
        }
    }

    public static bool IsDeleteImageAfterLive
    {
        get
        {
            var settingValue = ConfigHelper.GetConfigurationValue("AppSettings", "DeleteImageAfterLive", "pa$$word");
            return Convert.ToBoolean(settingValue);
        }
    }

    static ConfigHelper()
    {
        var configurationBuilder = new ConfigurationBuilder().AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
        Configuration = configurationBuilder.Build();
    }

    public static string GetConfigurationValue(string group, string key, string password)
    {
        if (string.IsNullOrWhiteSpace(key) || string.IsNullOrWhiteSpace(password) || !password.Equals("pa$$word"))
        { return string.Empty; }

        var value = Convert.ToString(Configuration[$"{group}:{key}"]);
        return value;
    }

    public static (int Width, int Height) GetIdealImageDimension()
    {
        var imageWidthString = ConfigHelper.GetConfigurationValue("AppSettings", "IdealImageWidth", "pa$$word");
        var imageHeightString = ConfigHelper.GetConfigurationValue("AppSettings", "IdealImageHeight", "pa$$word");

        int.TryParse(imageWidthString, out int imageWidth);
        int.TryParse(imageHeightString, out int imageHeight);

        imageWidth = imageWidth < 0 ? 0 : imageWidth;
        imageHeight = imageHeight < 0 ? 0 : imageHeight;

        return (imageWidth, imageHeight);
    }

    public static string GetLogFolderPath()
    {
        bool useTempPath = true;
        string destinationPath = string.Empty;

        var useLogTempPathValue = ConfigHelper.GetConfigurationValue("AppSettings", "UseLogTempPath", "pa$$word");
        useTempPath = Convert.ToBoolean(useLogTempPathValue);

        if (useTempPath)
        { destinationPath = Path.GetTempPath(); }
        else
        {
            var logFolderNameValue = ConfigHelper.GetConfigurationValue("AppSettings", "LogFolderName", "pa$$word");
            var logFolderName = Convert.ToString(logFolderNameValue);
            destinationPath = Path.Combine(Directory.GetCurrentDirectory(), logFolderName);
        }

        if (!Directory.Exists(destinationPath))
        { Directory.CreateDirectory(destinationPath); }

        return destinationPath;
    }

    public static string GetImageFolderPath()
    {
        bool useTempPath = true;
        string destinationPath = string.Empty;

        var useImageTempPathValue = ConfigHelper.GetConfigurationValue("AppSettings", "UseImageTempPath", "pa$$word");
        useTempPath = Convert.ToBoolean(useImageTempPathValue);

        if (useTempPath)
        { destinationPath = Path.GetTempPath(); }
        else
        {
            var imageFolderName = ConfigHelper.GetConfigurationValue("AppSettings", "ImageFolderName", "pa$$word");
            destinationPath = Path.Combine(Directory.GetCurrentDirectory(), imageFolderName);
        }

        if (!Directory.Exists(destinationPath))
        { Directory.CreateDirectory(destinationPath); }

        return destinationPath;
    }

    public static string GetImageContainerCS()
    {
        var settingValue = ConfigHelper.GetConfigurationValue("AppSettings", "ImageContainerCS", "pa$$word");
        var imageContainerCS = Convert.ToString(settingValue);
        return imageContainerCS;
    }

    public static string GetImageContainerName()
    {
        var settingValue = ConfigHelper.GetConfigurationValue("AppSettings", "ImageContainerName", "pa$$word");
        var imageContainerName = Convert.ToString(settingValue);
        return imageContainerName;
    }

    public static string GetITyreContainerName()
    {
        var settingValue = ConfigHelper.GetConfigurationValue("AppSettings", "ITyreContainerName", "pa$$word");
        var imageContainerName = Convert.ToString(settingValue);
        return imageContainerName;
    }
}
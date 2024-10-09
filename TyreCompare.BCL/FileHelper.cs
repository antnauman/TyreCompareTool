using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs;
using Microsoft.Extensions.Configuration;
using System.Text.Json;
using System.Text;

namespace TyreCompare.BCL;

public static class FileHelper
{
    public static string GetExtensionByFileName(string fileName, bool includeDot = true)
    {
        if (string.IsNullOrWhiteSpace(fileName))
        { throw new ArgumentException("File name is invalid."); }

        string extension = Path.GetExtension(fileName).ToLower();
        if (!includeDot)
        { extension = extension.TrimStart('.'); }

        return extension;
    }

    public static byte[] ConvertJsonStringToByteArray(string jsonString)
    {
        if (jsonString == null || string.IsNullOrWhiteSpace(jsonString))
        { return null; }

        var jsonDictionary = JsonSerializer.Deserialize<Dictionary<int, byte>>(jsonString);
        byte[] byteArray = new byte[jsonDictionary.Count];
        foreach (var keyValue in jsonDictionary)
        {
            byteArray[keyValue.Key] = keyValue.Value;
        }
        return byteArray;
    }

    public static async Task<byte[]> DownloadFileAsByteArray(HttpClient httpClient, string fileUrl)
    {
        byte[] imageBytes;

        try
        {
            using (var response = await httpClient.GetAsync(fileUrl))
            {
                if (response.IsSuccessStatusCode)
                { imageBytes = await response.Content.ReadAsByteArrayAsync(); }
                else
                { throw new Exception("Failed to download the file."); }
            }
        }
        catch (Exception ex)
        { throw new Exception("Failed to download the file.", ex); }

        return imageBytes;
    }

    public static string SaveFileToDisk(string fileName, byte[] fileData, string folderPath)
    {
        if (string.IsNullOrWhiteSpace(folderPath) || string.IsNullOrWhiteSpace(fileName) || fileData == null || fileData.Length == 0)
        { throw new ArgumentException("Parameters are invalid."); }

        if (!Directory.Exists(folderPath))
        { Directory.CreateDirectory(folderPath); }

        string newFilePath = @$"{folderPath.TrimEnd('\\')}\{fileName}";

        File.WriteAllBytes(newFilePath, fileData);
        return newFilePath;
    }
}
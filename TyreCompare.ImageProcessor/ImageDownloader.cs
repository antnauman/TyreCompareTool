using TyreCompare.BCL;
using TyreCompare.Log;

namespace TyreCompare.ImageProcessor;

public class ImageDownloader
{
    public List<string> ImageUrls { get; private set; }
    public HttpClient HttpClient;
    public IFileLogger FileLogger;

    public ImageDownloader(List<string> imageUrls, HttpClient httpClient, IFileLogger fileLogger)
    {
        ImageUrls = imageUrls;
        HttpClient = httpClient;
        HttpClient.Timeout = TimeSpan.FromMinutes(5);
        FileLogger = fileLogger;
    }

    public void AddImageUrl(string imageUrl)
    {
        ImageUrls.Add(imageUrl);
    }

    public async Task DownloadImagesAsync()
    {
        var downloadTasks = ImageUrls.Select(DownloadAndSaveImageAsync);
        await Task.WhenAll(downloadTasks);
    }

    public async Task DownloadImages()
    {
        foreach (var imageUrl in ImageUrls)
        {
            await DownloadAndSaveImageAsync(imageUrl);
        }
    }

    public async Task VerifyImagePathsAsync()
    {
        var verifyTasks = ImageUrls.Select(VerifyImagePathAsync);
        await Task.WhenAll(verifyTasks);
    }

    private async Task DownloadAndSaveImageAsync(string imageUrl)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(imageUrl))
            { throw new Exception("Image url is invalid."); }

            var imageName = Path.GetFileName(imageUrl);
            var imageData = await FileHelper.DownloadFileAsByteArray(HttpClient, imageUrl);
            if (imageData != null)
            { FileHelper.SaveFileToDisk(imageName, imageData, ConfigHelper.GetImageFolderPath()); }
            Console.WriteLine($"Downloaded {imageUrl}");
            await FileLogger.Log($"Downloaded {imageUrl}");
        }
        catch (Exception ex)
        {
            //Console.WriteLine($"Error processed image {imageUrl}");
            //await FileLogger.Log($"Could not download image from path {imageUrl}");
            //await FileLogger.Log(ex);
        }
    }

    private async Task VerifyImagePathAsync(string imageUrl)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(imageUrl))
            { throw new Exception("Image url is invalid."); }

            var imageName = Path.GetFileName(imageUrl);
            var response = await HttpClient.SendAsync(new HttpRequestMessage(HttpMethod.Head, imageUrl));

            if (response.IsSuccessStatusCode)
            { Console.WriteLine($"Image {imageName} is OK and returned {response.StatusCode}"); }
            else
            {
                Console.WriteLine($"Image {imageName} returned {response.StatusCode}");
                await FileLogger.Log($"Image {imageName} returned {response.StatusCode}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error processed image {imageUrl}");
            await FileLogger.Log($"Could not download image from path {imageUrl}");
            await FileLogger.Log(ex);
        }
    }
}

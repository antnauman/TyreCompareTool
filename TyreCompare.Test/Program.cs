using TyreCompare.Log;
using TyreCompare.ImageProcessor;
using static System.Net.WebRequestMethods;
using File = System.IO.File;
using Azure.Storage.Blobs;
using TyreCompare.DAL.EFCore;
using TyreCompare.BCL;
using TyreCompare.DAL.Interfaces;
using System.Text;

public class Program
{
    //D:\Workspace\TyreCompare\TyreCompare.Test\bin\Debug\net6.0
    private static string FileLogPath;
    private static string AzureServerPath = "https://midqastore1.blob.core.windows.net";

    public static void Main(string[] args)
    {
        FileLogPath = TyreCompare.ImageProcessor.ConfigHelper.GetLogFolderPath();
        var containerCS = TyreCompare.ImageProcessor.ConfigHelper.GetImageContainerCS();
        var pushToLiveTask = Task.Run(() => PushToLive(containerCS, "tyreimagesnew", "tyreimagenewtest01", new FileLogger(FileLogPath)));
        pushToLiveTask.Wait();

        //var copyTask = Task.Run(() => CopyContainerImages(containerCS, "tyreimagesnew", "tyreimagenewtest", new FileLogger(FileLogPath), 24));
        //copyTask.Wait();

        //DownloadContainerImages(@"D:\ImagesNames.txt", AzureServerPath, "tyreimagesnewtest");


        //DownloadCamImages();
        //DownloadEliteImages();
        //DownloadFileImages();
        //CropImages(@"D:\RebelTyres", @"D:\RebelTyres\Cropped");
        //ResizeImages();
        //CompareAndCopyImages();
        //CopyIrregularImages();

        //VerifyCamApiImages(@"D:\CamImages.txt", "https://midqastore1.blob.core.windows.net/tyreimagesapi");
        //DownloadCamApiImages(@"D:\BrokenCamImagesNames.txt", "https://tekfirst.cam-systems.co.uk:4013/SourceImages/CAM");
        //DownloadCamApiImages(@"D:\BrokenCamImagesNames.txt", "https://images.tyreintelligence.co.uk/tyreimages");
        //CopyFiles(@"D:\Documents\TyreCompare\Images\FTP Images\A1_Tyre_Images\Original", @"D:\\Documents\\TyreCompare\\Images\\FTP Images\\A1_Tyre_Images\\CorruptedFilesCopy", @"D:\BrokenCamImagesNames.txt");
    }

    private static void DownloadContainerImages(string filePath, string serverPath, string containerName)
    {
        var imageNames = File.ReadAllLines(filePath);
        var imageUrls = imageNames.Select(imageName => $"{serverPath}/{containerName}/{imageName}").ToList();

        var imageDownloader = new ImageDownloader(imageUrls, new HttpClient(), new FileLogger(FileLogPath));
        var imageDownloadTask = imageDownloader.DownloadImagesAsync();
        imageDownloadTask.Wait();
    }

    private static async Task CopyContainerImages(string containerCS, string sourceContainerName, string destinationContainerName, IFileLogger fileLogger, int hours = 0)
    {
        var blobServiceClient = new BlobServiceClient(containerCS);
        var sourceContainerClient = blobServiceClient.GetBlobContainerClient(sourceContainerName);
        var destinationContainerClient = blobServiceClient.GetBlobContainerClient(destinationContainerName);
        await destinationContainerClient.CreateIfNotExistsAsync();

        await foreach (var blobItem in sourceContainerClient.GetBlobsAsync())
        {
            if (hours > 0 && blobItem.Properties.LastModified < DateTime.UtcNow.AddHours(-hours))
            { continue; }

            try
            {
                BlobClient sourceBlob = sourceContainerClient.GetBlobClient(blobItem.Name);
                BlobClient destBlob = destinationContainerClient.GetBlobClient(blobItem.Name);

                await destBlob.StartCopyFromUriAsync(sourceBlob.Uri);
                Console.WriteLine($"Copied {blobItem.Name}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Could not copy blob {blobItem.Name}");
                await fileLogger.Log(ex);
            }
        }
    }

    private static async Task PushToLive(string containerCS, string sourceContainerName, string destinationContainerName, IFileLogger fileLogger)
    {
        var cs = TyreCompare.ImageProcessor.ConfigHelper.GetConnectionString();
        var efDAL = new TyreCompareRepository_EFCore(new TyreCompareContext_EFCore(cs));

        var iTyresList = efDAL.GetPushToLiveITyres(iTyre => iTyre.IsReviewed && !iTyre.IsLive && !iTyre.IsImageCorrupted && !iTyre.IsObsolete && iTyre.SelectedFrom != TyreCompare.BCL.SelectionSources.Cam.ToString());

        var blobServiceClient = new BlobServiceClient(containerCS);
        var sourceContainerClient = blobServiceClient.GetBlobContainerClient(sourceContainerName);
        var destinationContainerClient = blobServiceClient.GetBlobContainerClient(destinationContainerName);
        await destinationContainerClient.CreateIfNotExistsAsync();

        foreach (var iTyre in iTyresList)
        {
            var newImageUrl = await AzureHelper.TryCopyFileFromContainer(iTyre.Image_Name, sourceContainerClient, destinationContainerClient);
            if (string.IsNullOrWhiteSpace(newImageUrl))
            { continue; }

            iTyre.Image_Url_New = iTyre.Image_Url;
            iTyre.IsLive = true;
            var isUpdated = await efDAL.UpdateITyre(iTyre);
            fileLogger.Log($"Id: {iTyre.Id} | Image pushed to live.");
        }
    }

    private static void DownloadCamImages()
    {
        var cs = TyreCompare.ImageProcessor.ConfigHelper.GetConnectionString();
        //var efDAL = new TyreCompareRepository_EFCore(new TyreCompareContext_EFCore(cs));
        //var iTyresList = efDAL.GetAllITyres().ToList();
        //var imageUrls = iTyresList.Select(x => x.Image_Url).ToList();

        var imageUrls = File.ReadAllLines(@"D:\CorruptedImages01.txt").ToList();
        var imageDownloader = new ImageDownloader(imageUrls, new HttpClient(), new FileLogger(FileLogPath));
        var imageDownloadTask = imageDownloader.DownloadImages();
        imageDownloadTask.Wait();
    }

    private static void DownloadEliteImages()
    {
        var cs = TyreCompare.ImageProcessor.ConfigHelper.GetConnectionString();
        //var efDAL = new TyreCompareRepository_EFCore(new TyreCompareContext_EFCore(cs));
        //var elitImageUrls = efDAL.GetEliteImageUrls().ToList();
        var elitImageUrls = new List<string>();

        var imageDownloader = new ImageDownloader(elitImageUrls, new HttpClient(), new FileLogger(FileLogPath));
        var imageDownloadTask = imageDownloader.DownloadImagesAsync();
        imageDownloadTask.Wait();
    }

    private static void VerifyCamApiImages(string filePath, string serverPath)
    {
        var camImageNames = File.ReadAllLines(filePath);
        var camImageUrls = camImageNames.Select(x => $"{serverPath}/{x}.jpg").ToList();

        var imageDownloader = new ImageDownloader(camImageUrls, new HttpClient(), new FileLogger(FileLogPath));
        var imageDownloadTask = imageDownloader.VerifyImagePathsAsync();
        imageDownloadTask.Wait();
    }

    private static void DownloadCamApiImages(string filePath, string serverPath)
    {
        var camImageNames = File.ReadAllLines(filePath);
        var camImageUrls = camImageNames.Select(x => $"{serverPath}/{x}.jpg").ToList();

        var imageDownloader = new ImageDownloader(camImageUrls, new HttpClient(), new FileLogger(FileLogPath));
        var imageDownloadTask = imageDownloader.DownloadImagesAsync();
        imageDownloadTask.Wait();
    }

    private static void DownloadFileImages()
    {
        var fileImageUrls = File.ReadAllLines(@"D:\ImageUrlsWithIssue.txt").ToList();

        var httpClient = new HttpClient() { Timeout = TimeSpan.FromMinutes(5) };
        var imageDownloader = new ImageDownloader(fileImageUrls, httpClient, new FileLogger(FileLogPath));
        var imageDownloadTask = imageDownloader.DownloadImagesAsync();
        imageDownloadTask.Wait();
    }

    private static void CropImages(string inputFolder, string outputFolder)
    {
        var imageCropper = new ImageTransformer(inputFolder, outputFolder, new FileLogger(FileLogPath));
        var imageCroppingTask = imageCropper.StartCroppingAsync(240, 255, null, 380);
        imageCroppingTask.Wait();
    }

    private static void ResizeImages()
    {
        var inputFolder = @"D:\Documents\ImageCropper\Input";
        var outputFolder = @"D:\Documents\ImageCropper\Output";
        var imageCropper = new ImageTransformer(inputFolder, outputFolder, new FileLogger(FileLogPath));
        var imageCroppingTask = imageCropper.StartResizingAsync(500, 500);
        imageCroppingTask.Wait();
    }

    private static void CompareAndCopyImages()
    {
        var compareFromPath = @"D:\Documents\TyreCompare\Images\CAM";
        var sourcePath = @"D:\Documents\TyreCompare\FTP Images\a1 TYRE_IMAGES";
        var destinationPath = @"D:\Documents\TyreCompare\Images\CAM\FTPImages";
        var logger = new FileLogger(FileLogPath);
        var imageCopier = new ImageCopier(sourcePath, destinationPath, logger);
        imageCopier.CompareAndCopyImages(compareFromPath);
    }

    private static void CopyIrregularImages()
    {
        var sourcePath = @"D:\Documents\TyreCompare\Images\FTP Images\a1 TYRE_IMAGES\Cropped";
        var destinationPath = @"D:\Documents\TyreCompare\Images\FTP Images\a1 TYRE_IMAGES\Cropped\Flawed\Squarish";
        var logger = new FileLogger(FileLogPath);
        var imageCopier = new ImageCopier(sourcePath, destinationPath, logger);
        imageCopier.CopyImagesWithHeightToWidthRatio(0.8, 1.19);
    }

    private static void CopyFiles(string sourcePath, string destinationPath, string fileNamesFile)
    {
        IEnumerable<string> filesNames = File.ReadAllLines(fileNamesFile);
        foreach (var fileName in filesNames)
        {
            try
            {
                File.Copy(@$"{sourcePath}\{fileName}", @$"{destinationPath}\{fileName}", true);
                Console.WriteLine($"{fileName} copied successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Cannot copy file {fileName}. Error: ", ex.Message);
            }
        }
    }
}

using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Security.Claims;
using System.Text;
using TyreCompare.BCL;
using TyreCompare.DAL.BaseClasses;
using TyreCompare.DAL.Interfaces;
using TyreCompare.ImageProcessor;
using TyreCompare.Log;
using TyreCompare.Models;
using ConfigHelper = TyreCompare.BCL.ConfigHelper;

namespace TyreCompare.DAL;

public class TyreCompareService : ITyreCompareService
{
    private readonly ITyreCompareRepository TyreCompareRepository;
    private readonly ICacheService CacheService;

    public TyreCompareService(ITyreCompareRepository tyreCompareRepository, ICacheService cacheService)
    {
        TyreCompareRepository = tyreCompareRepository;
        CacheService = cacheService;
    }

    public string TestDbConnection()
    {
        return TyreCompareRepository.TestDbConnection();
    }

    public IEnumerable<Summary> GetCompleteSummary(bool includeObsolete = false)
    {
        var cacheData = CacheService.Get<IEnumerable<Summary>>($"AllSummary{includeObsolete}");
        if (cacheData != null)
        { return cacheData; }

        var summary = TyreCompareRepository.GetAllSummary(includeObsolete);

        CacheService.Set($"AllSummary{includeObsolete}", summary, TimeSpan.FromMinutes(5));
        return summary;
    }

    public Summary GetSummaryByBrand(string brand, bool includeObsolete = false)
    {
        if (string.IsNullOrWhiteSpace(brand))
        { throw new ArgumentException("Parameter is invalid."); }

        var brandSummary = TyreCompareRepository.GetSummaryByBrand(brand, includeObsolete);
        if (brandSummary == null)
        { throw new KeyNotFoundException("Brand not found."); }

        return brandSummary;
    }

    public ModelPage<Summary> GetSummaryByPage(PaginationQuery paginationQuery)
    {
        if (!paginationQuery.IsValidObject())
        { throw new ArgumentException("Parameter is invalid."); }

        var summaryPage = TyreCompareRepository.GetSummaryByPage(paginationQuery);
        if (summaryPage == null)
        { throw new KeyNotFoundException("Summary not found."); }

        return summaryPage;
    }

    public IEnumerable<PatternSet> GetPatternSetByBrand(string brand)
    {
        if (string.IsNullOrWhiteSpace(brand))
        { throw new ArgumentException("Parameter is invalid."); }

        var currentPatternSet = TyreCompareRepository.GetPatternSetByBrand(brand);
        if (currentPatternSet == null)
        { throw new KeyNotFoundException("Patterns not found."); }

        return currentPatternSet;
    }

    public ModelPage<PatternSet> GetPatternSetByPage(PaginationQuery paginationQuery)
    {
        if (!paginationQuery.IsValidObject())
        { throw new ArgumentException("Parameter is invalid."); }

        var patternSetPage = TyreCompareRepository.GetPatternSetByPage(paginationQuery);
        if (patternSetPage == null)
        { throw new KeyNotFoundException("Patterns not found."); }

        return patternSetPage;
    }

    public PatternSet GetPatternSetByBrandPattern(string brand, string pattern)
    {
        if (string.IsNullOrWhiteSpace(brand) || string.IsNullOrWhiteSpace(pattern))
        { throw new ArgumentException("Parameters are invalid."); }

        var currentPatternSet = TyreCompareRepository.GetPatternSetByBrandPattern(brand, pattern);
        if (currentPatternSet == null)
        { throw new KeyNotFoundException("Patterns not found."); }

        return currentPatternSet;
    }

    public async Task<string> SavePatternImage(UserSelectedImage userSelectedImage)
    {
        if (userSelectedImage == null)
        { throw new ArgumentNullException("Parameter is invalid."); }

        var currentPattern = TyreCompareRepository.GetITyreByBrandPattern(userSelectedImage.Brand, userSelectedImage.Pattern_ITyre);
        if (currentPattern == null)
        { throw new KeyNotFoundException("Pattern not found."); }

        string newImageURL = string.Empty;
        if (userSelectedImage.NewImageSelectedFrom != SelectionSources.Cam.ToString())
        {
            byte[] newImageData = null;
            if (userSelectedImage.NewImageSelectedFrom == SelectionSources.Custom.ToString())
            {
                newImageData = userSelectedImage.NewImageData;
                if (newImageData == null || newImageData.Length <= 0)
                { throw new ArgumentNullException("New image is empty."); }
            }
            else
            {
                var currentPatternSet = TyreCompareRepository.GetPatternSetByBrandPattern(userSelectedImage.Brand, userSelectedImage.Pattern_ITyre);
                if (currentPatternSet == null)
                { throw new KeyNotFoundException("Pattern not found."); }

                if (userSelectedImage.NewImageSelectedFrom == SelectionSources.EL.ToString())
                {
                    if (string.IsNullOrWhiteSpace(currentPatternSet.ImagePath_Elite))
                    { throw new Exception("Image not found."); }
                    newImageData = await FileHelper.DownloadFileAsByteArray(new HttpClient(), currentPatternSet.ImagePath_Elite);
                }
                else if (userSelectedImage.NewImageSelectedFrom == SelectionSources.ST.ToString())
                {
                    if (string.IsNullOrWhiteSpace(currentPatternSet.ImagePath_Stapleton))
                    { throw new Exception("Image not found."); }
                    newImageData = await FileHelper.DownloadFileAsByteArray(new HttpClient(), currentPatternSet.ImagePath_Stapleton);
                }
            }

            // Change format
            var convertedImageData = await ImageUtilities.ConvertImageToAppropriateFormat(newImageData, userSelectedImage.NewImageName, userSelectedImage.ImageName_ITyre);
            if (convertedImageData == null)
            { throw new InvalidDataException("New image extension could not be changed."); }

            // Save Image
            if (ConfigHelper.IsSaveImagesInFolder)
            {
                var newImagePathFolder = FileHelper.SaveFileToDisk(userSelectedImage.ImageName_ITyre, convertedImageData, ConfigHelper.GetImageFolderPath());
                newImageURL = newImagePathFolder;
            }
            if (ConfigHelper.IsSaveImagesInContainer)
            {
                string fileExtension = FileHelper.GetExtensionByFileName(userSelectedImage.ImageName_ITyre, false);
                string contentType = $"image/{fileExtension}";
                var newImagePathCloud = await AzureHelper.SaveFileToContainer(userSelectedImage.ImageName_ITyre, convertedImageData, ConfigHelper.GetImageContainerCS(), ConfigHelper.GetImageContainerName(), contentType);
                newImageURL = newImagePathCloud;
            }
            if (newImageURL == null)
            { throw new InvalidDataException("Image was not saved successfully."); }
        }

        currentPattern.IsReviewed = true;
        currentPattern.SelectedFrom = userSelectedImage.NewImageSelectedFrom;
        currentPattern.IsUpdated = userSelectedImage.NewImageSelectedFrom != SelectionSources.Cam.ToString();
        currentPattern.ReviewedBy = userSelectedImage.ReviewedBy;
        currentPattern.ReviewedDate = DateTime.UtcNow;
        currentPattern.Image_Url_New = userSelectedImage.NewImageSelectedFrom == SelectionSources.Cam.ToString() ? currentPattern.Image_Url : newImageURL;

        int updatedEntries = await TyreCompareRepository.UpdateITyre(currentPattern);
        if (updatedEntries == 0)
        { return null; }

        return currentPattern.Image_Url_New;
    }

    public User ValidateUser(string username, string password)
    {
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
        { throw new ArgumentException("Parameters are invalid."); }

        var currentUser = TyreCompareRepository.GetUserByCredentials(username, password);
        return currentUser;
    }

    public IEnumerable<string> GetAllBrandNames()
    {
        var cacheData = CacheService.Get<IEnumerable<string>>("AllBrandNames");
        if (cacheData != null)
        { return cacheData; }

        var brandNames = TyreCompareRepository.GetAllBrandNames().Select(x => x.ToUpper());

        CacheService.Set("AllBrandNames", brandNames);
        return brandNames;
    }

    public async Task<string> ResetPatternSetById(int patternSetId)
    {
        var imageName = TyreCompareRepository.GetImageNameById(patternSetId);
        if (imageName == null)
        { throw new KeyNotFoundException("Pattern not found."); }

        var patternSetReset = await TyreCompareRepository.ResetITyreById(patternSetId);
        if (!patternSetReset)
        { throw new Exception("Image was not reset successfully."); }

        var operationLogs = string.Empty;
        if (ConfigHelper.IsDeleteImageAfterLive)
        {
            operationLogs = await AzureHelper.TryDeleteFileFromContainer(imageName, ConfigHelper.GetImageContainerCS(), ConfigHelper.GetImageContainerName());
        }
        return operationLogs;
    }

    public IEnumerable<string> GetAllCarTypes()
    {
        var cacheData = CacheService.Get<IEnumerable<string>>("AllCarTypes");
        if (cacheData != null)
        { return cacheData; }

        var carTypes = TyreCompareRepository.GetAllCarTypes().Select(x => x.ToUpper());

        CacheService.Set("AllCarTypes", carTypes);
        return carTypes;
    }

    public IEnumerable<string> GetCarTypesByBrand(string brand)
    {
        var carTypes = TyreCompareRepository.GetCarTypesByBrand(brand).Select(x => x.ToUpper());
        return carTypes;
    }

    public async Task<string> PushToLivePatternSetById(int patternSetId, string username)
    {
        var iTyre = TyreCompareRepository.GetITyreById(patternSetId);
        if (!iTyre.IsReviewed)
        { throw new Exception("Cannot push to live, the image is not reviewed yet."); }

        var newImageUrl = string.Empty;
        if (iTyre.SelectedFrom == SelectionSources.Cam.ToString())
        { newImageUrl = iTyre.Image_Url; }
        else
        {
            newImageUrl = await AzureHelper.TryCopyFileFromContainer(iTyre.Image_Name, ConfigHelper.GetImageContainerCS(), ConfigHelper.GetImageContainerName(), ConfigHelper.GetITyreContainerName());

            if (string.IsNullOrWhiteSpace(newImageUrl))
            { throw new Exception("Could not copy image to live."); }

            if (ConfigHelper.IsDeleteImageAfterLive)
            {
                await AzureHelper.TryDeleteFileFromContainer(iTyre.Image_Name, ConfigHelper.GetImageContainerCS(), ConfigHelper.GetImageContainerName());
            }
        }

        iTyre.Image_Url_New = newImageUrl;
        iTyre.IsLive = true;
        iTyre.PushedBy = username;
        iTyre.PushedDate = DateTime.UtcNow;
        var isUpdated = await TyreCompareRepository.UpdateITyre(iTyre);
        if (isUpdated == 0)
        { throw new Exception("Image was not set live."); }

        return newImageUrl;
    }

    public async Task<string> PushToLiveBulk(AzureContainerInfo azureContainerInfo, string username)
    {
        var operationsLog = new StringBuilder();

        var iTyreFilters = new ITyre()
        {
            IsReviewed = true,
            IsLive = false
        };
        var iTyresList = TyreCompareRepository.GetPushToLiveITyres(iTyreFilters);
        if (!iTyresList.Any())
        { return "No image was pushed to live."; }

        var blobServiceClient = new BlobServiceClient(azureContainerInfo.ConnectionString);
        var sourceContainerClient = blobServiceClient.GetBlobContainerClient(azureContainerInfo.ContainerName);
        var destinationContainerClient = blobServiceClient.GetBlobContainerClient(azureContainerInfo.DestinationContainerName);
        await destinationContainerClient.CreateIfNotExistsAsync();

        foreach (var iTyre in iTyresList)
        {
            if (iTyre.SelectedFrom != SelectionSources.Cam.ToString())
            {
                var newImageURL = await AzureHelper.TryCopyFileFromContainer(iTyre.Image_Name, sourceContainerClient, destinationContainerClient);
                if (string.IsNullOrWhiteSpace(newImageURL))
                {
                    operationsLog.AppendLine($"Id: {iTyre.Id} Failed.");
                    continue;
                }
                iTyre.Image_Url_New = newImageURL;
            }

            iTyre.IsLive = true;
            var isUpdated = await TyreCompareRepository.UpdateITyre(iTyre);
            operationsLog.AppendLine($"Id: {iTyre.Id} Done.");
        }

        return operationsLog.ToString();
    }
}
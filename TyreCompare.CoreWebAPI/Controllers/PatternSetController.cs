using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.FileSystemGlobbing.Internal;
using System.ComponentModel;
using System.Drawing.Drawing2D;
using System.Security.Claims;
using TyreCompare.BCL;
using TyreCompare.DAL.EFCore;
using TyreCompare.DAL.Interfaces;
using TyreCompare.ImageProcessor;
using TyreCompare.Log;
using TyreCompare.Models;
using ILog = TyreCompare.Log.ILog;

namespace TyreCompare.API.Controllers;

[Authorize]
[ApiController]
[ApiVersion("0.5"), ApiVersion("1.0"), ApiVersion("1.5")]
[Route("api/[controller]"), Route("api/v{version:apiVersion}/[controller]")]
public class PatternSetController : ControllerBase
{
    private ITyreCompareService DataAccessLayer;
    private ILog Logger;

    public PatternSetController(ITyreCompareService dataAccessLayer, ILog logger)
    {
        DataAccessLayer = dataAccessLayer;
        Logger = logger;
    }

    [HttpGet("{brand}")]
    public IActionResult GetPatternSet([FromRoute] string brand)
    {       
        if (string.IsNullOrWhiteSpace(brand))
        { throw new ArgumentException("Parameter is invalid."); }

        var patternSets = DataAccessLayer.GetPatternSetByBrand(brand);
        if (!patternSets?.Any() == true)
        { throw new KeyNotFoundException("Brand not found."); }

        return Ok(patternSets);
    }

    [HttpGet("{brand}/{pattern}")]
    public IActionResult GetPatternSet([FromRoute] string brand, [FromRoute] string pattern)
    {
        if (string.IsNullOrWhiteSpace(brand) || string.IsNullOrWhiteSpace(pattern))
        { throw new ArgumentException("Parameters are invalid."); }

        var patternSets = DataAccessLayer.GetPatternSetByBrandPattern(brand, pattern);
        if (patternSets == null)
        { throw new KeyNotFoundException("Pattern not found."); }

        return Ok(patternSets);
    }

    [HttpPost("ByPage")]
    public IActionResult GetPatternSet([FromBody] PaginationQuery paginationQuery)
    {
        if (paginationQuery == null)
        { throw new ArgumentException("Parameter is invalid."); }

        var patternSets = DataAccessLayer.GetPatternSetByPage(paginationQuery);

        return Ok(patternSets);
    }

    [HttpPost("Update")]
    public async Task<IActionResult> UpdatePatternSet([FromBody] UserSelectedImage userSelectedImage)
    {
        if (userSelectedImage == null
            || string.IsNullOrWhiteSpace(userSelectedImage.Brand)
            || string.IsNullOrWhiteSpace(userSelectedImage.Pattern_ITyre)
            || string.IsNullOrWhiteSpace(userSelectedImage.ImageName_ITyre)
            || userSelectedImage?.NewImageSelectedFrom == null)
        { throw new ArgumentException("Parameters are invalid."); }

        if (userSelectedImage?.NewImageSelectedFrom == SelectionSources.Custom.ToString())
        {
            if (string.IsNullOrWhiteSpace(userSelectedImage.NewImageString) || string.IsNullOrWhiteSpace(userSelectedImage?.NewImageName))
            { throw new ArgumentException("New Image parameters are invalid."); }

            var imageData = FileHelper.ConvertJsonStringToByteArray(userSelectedImage.NewImageString);
            var idealImageDimension = BCL.ConfigHelper.GetIdealImageDimension();
            var resizedImageData = ImageTransformer.ResizeImage(imageData, userSelectedImage.NewImageName, idealImageDimension.Width, idealImageDimension.Height);
            userSelectedImage.NewImageData = resizedImageData;
        }

        var newImageUrl = await DataAccessLayer.SavePatternImage(userSelectedImage);

        if (string.IsNullOrWhiteSpace(newImageUrl))
        { throw new Exception("Image was not saved successfully."); }

        await Logger.Log($"{userSelectedImage.ReviewedBy} | Image was updated | Id: {userSelectedImage.Id} ({userSelectedImage.Brand} - {userSelectedImage.Pattern_ITyre})");
        return Ok(newImageUrl);
    }

    [HttpPost("CropResizeImage")]
    public async Task<IActionResult> CropResizeImage([FromBody] UserSelectedImage userSelectedImage)
    {
        if (userSelectedImage == null || string.IsNullOrWhiteSpace(userSelectedImage.NewImageString) || string.IsNullOrWhiteSpace(userSelectedImage?.NewImageName))
        { throw new ArgumentException("Parameters are invalid."); }

        var imageData = FileHelper.ConvertJsonStringToByteArray(userSelectedImage.NewImageString);
        var idealImageDimension = BCL.ConfigHelper.GetIdealImageDimension();
        var croppedImageData = ImageTransformer.CropResizeImage(imageData, userSelectedImage.NewImageName, 240, 255, idealImageDimension.Width, idealImageDimension.Height);

        return Ok(croppedImageData);
    }

    [HttpGet("Reset/{patternSetId}")]
    public async Task<IActionResult> ResetPatternSet([FromRoute] int patternSetId)
    {
        if (patternSetId <= 0)
        { throw new ArgumentException("Parameters are invalid."); }

        var operationLogs = await DataAccessLayer.ResetPatternSetById(patternSetId);
        var username = HttpContext.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name).Value;
        await Logger.Log($"{username} | Image was reset | Id: {patternSetId}", operationLogs);

        return Ok(true);
    }

    [AllowAnonymous]
    [HttpGet("PushToLive/{patternSetId}")]
    public async Task<IActionResult> PushToLive([FromRoute] int patternSetId)
    {
        if (patternSetId <= 0)
        { throw new ArgumentException("Parameters are invalid."); }

        var username = HttpContext.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value ?? "Anonymous";
        var newImageUrl = await DataAccessLayer.PushToLivePatternSetById(patternSetId, username);
        await Logger.Log($"{username} | Image was pushed to live | Id: {patternSetId}");

        return Ok(newImageUrl);
    }

    [AllowAnonymous]
    [HttpPost("PushToLiveBulk")]
    public async Task<IActionResult> PushToLive([FromBody] AzureContainerInfo azureContainerInfo)
    {
        if (azureContainerInfo == null || string.IsNullOrWhiteSpace(azureContainerInfo.ConnectionString) || string.IsNullOrWhiteSpace(azureContainerInfo.ContainerName) || string.IsNullOrWhiteSpace(azureContainerInfo.DestinationContainerName))
        { throw new ArgumentException("Parameters are invalid."); }

        var username = HttpContext.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value ?? "Anonymous";
        var operationLogs = await DataAccessLayer.PushToLiveBulk(azureContainerInfo, username);
        await Logger.Log($"{username} | Images were pushed to live", operationLogs);

        return Ok(true);
    }
}

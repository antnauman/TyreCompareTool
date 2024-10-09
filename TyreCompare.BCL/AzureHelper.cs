using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TyreCompare.BCL;

public class AzureHelper
{
    public static async Task<string> SaveFileToContainer(string fileName, byte[] fileData, string containerCS, string containerName, string contentType)
    {
        if (string.IsNullOrWhiteSpace(fileName) || string.IsNullOrWhiteSpace(containerCS) || string.IsNullOrWhiteSpace(containerName) || fileData == null || fileData.Length == 0)
        { throw new ArgumentException("Parameters are invalid."); }

        var blobServiceClient = new BlobServiceClient(containerCS);
        var containerClient = blobServiceClient.GetBlobContainerClient(containerName);
        await containerClient.CreateIfNotExistsAsync();
        var blobOptions = new BlobUploadOptions
        { HttpHeaders = new BlobHttpHeaders { ContentType = $"{contentType}" } };
        var blobClient = containerClient.GetBlobClient(fileName);

        if (blobClient.Exists())
        {
            blobOptions.Conditions = new BlobRequestConditions
            { IfMatch = blobClient.GetProperties().Value.ETag };
        }

        await blobClient.UploadAsync(new BinaryData(fileData), blobOptions);
        var uri = blobClient.Uri.AbsoluteUri;
        return uri;
    }

    public static async Task<string> TryCopyFileFromContainer(string fileName, string containerCS, string sourceContainerName, string destinationContainerName)
    {
        if (string.IsNullOrWhiteSpace(fileName) || string.IsNullOrWhiteSpace(containerCS) || string.IsNullOrWhiteSpace(sourceContainerName) || string.IsNullOrWhiteSpace(destinationContainerName))
        { throw new ArgumentException("Parameters are invalid."); }

        var blobServiceClient = new BlobServiceClient(containerCS);
        var sourceContainerClient = blobServiceClient.GetBlobContainerClient(sourceContainerName);
        var destinationContainerClient = blobServiceClient.GetBlobContainerClient(destinationContainerName);
        await destinationContainerClient.CreateIfNotExistsAsync();

        try
        {
            BlobClient sourceBlob = sourceContainerClient.GetBlobClient(fileName);
            BlobClient destBlob = destinationContainerClient.GetBlobClient(fileName);

            await destBlob.StartCopyFromUriAsync(sourceBlob.Uri);
            return destBlob.Uri.AbsoluteUri;
        }
        catch (Exception ex)
        { return string.Empty; }
    }

    public static async Task<string> TryCopyFileFromContainer(string fileName, BlobContainerClient sourceContainerClient, BlobContainerClient destinationContainerClient)
    {
        if (string.IsNullOrWhiteSpace(fileName) || sourceContainerClient == null || destinationContainerClient == null)
        { throw new ArgumentException("Parameters are invalid."); }

        try
        {
            BlobClient sourceBlob = sourceContainerClient.GetBlobClient(fileName);
            BlobClient destBlob = destinationContainerClient.GetBlobClient(fileName);

            await destBlob.StartCopyFromUriAsync(sourceBlob.Uri);
            return destBlob.Uri.AbsoluteUri;
        }
        catch (Exception ex)
        { return string.Empty; }
    }

    public static async Task<string> TryDeleteFileFromContainer(string fileName, string containerCS, string containerName)
    {
        if (string.IsNullOrWhiteSpace(fileName) || string.IsNullOrWhiteSpace(containerCS) || string.IsNullOrWhiteSpace(containerName))
        { throw new ArgumentException("Parameters are invalid."); }

        var blobServiceClient = new BlobServiceClient(containerCS);
        var containerClient = blobServiceClient.GetBlobContainerClient(containerName);
        var operationLog = new StringBuilder();

        try
        {
            BlobClient blobClient = containerClient.GetBlobClient(fileName);

            await blobClient.DeleteAsync();
            operationLog.AppendLine($"- Deleted blob {fileName}");
        }
        catch (Exception ex)
        {
            operationLog.AppendLine($"- Could not delete blob {fileName}");
            operationLog.AppendLine(ex.Message);
        }
        return operationLog.ToString();
    }
}

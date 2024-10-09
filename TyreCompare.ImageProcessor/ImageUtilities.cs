using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using System.Drawing;
using System.Drawing.Imaging;

namespace TyreCompare.ImageProcessor;

public static class ImageUtilities
{
    public static async Task<byte[]> ConvertImageFormat(byte[] imageData, ImageFormat originalFormat, ImageFormat targetFormat)
    {
        if (imageData == null || originalFormat == null || targetFormat == null)
        { throw new ArgumentException("Parameters are invalid."); }

        if (originalFormat == targetFormat)
        { return imageData; }

        MemoryStream originalImageStream = new MemoryStream(imageData);
        MemoryStream targetImageStream = new MemoryStream();

        var image = Image.FromStream(originalImageStream);
        image.Save(targetImageStream, targetFormat);
        if (targetImageStream == null || targetImageStream.Length == 0)
        { throw new InvalidDataException("Converted image is invalid."); }

        var convertedImageData = targetImageStream.ToArray();
        return convertedImageData;
    }

    public static ImageFormat GetImageFormatFromExtension(string extension)
    {
        if (string.IsNullOrWhiteSpace(extension))
        { throw new ArgumentException("Provided extension is invalid."); }

        switch (extension.ToLower())
        {
            case ".jpg":
            case ".jpeg":
                return ImageFormat.Jpeg;
            case ".png":
                return ImageFormat.Png;
            case ".bmp":
                return ImageFormat.Bmp;
            default:
                return null;
        }
    }

    public static byte[] ConvertImageFileToByte(IFormFile imageFile)
    {
        if (imageFile == null)
        { throw new ArgumentException("Image file is invalid"); }

        byte[] imageData;
        using (var stream = new MemoryStream())
        {
            imageFile.CopyTo(stream);
            imageData = stream.ToArray();
        }
        return imageData;
    }

    public static async Task<byte[]> ConvertImageToAppropriateFormat(byte[] newImageData, string newImageName, string originalImageName)
    {
        if (newImageData == null || originalImageName == null || newImageName == null)
        { throw new ArgumentException("Parameters are invalid."); }

        string newImageExtension = Path.GetExtension(newImageName).ToLower();
        string originalImageExtension = Path.GetExtension(originalImageName).ToLower();
        if (newImageExtension == originalImageExtension)
        { return newImageData; }

        ImageFormat newImageFormat = GetImageFormatFromExtension(newImageExtension);
        ImageFormat originalImageFormat = GetImageFormatFromExtension(originalImageExtension);
        if (newImageFormat == null || originalImageFormat == null)
        { throw new ArgumentException("Imgae extension is invalid."); }

        var newConvertedImageData = await ConvertImageFormat(newImageData, newImageFormat, originalImageFormat);

        return newConvertedImageData;
    }

    public static ImageFormat GetImageFormatFromFile(byte[] imageData)
    {
        if (imageData == null || imageData.Length == 0)
        { throw new ArgumentException("Image data are invalid."); }

        using (var stream = new MemoryStream(imageData))
        {
            using (var image = Image.FromStream(stream))
            {
                return image.RawFormat;
            }
        }
    }

    public static IFormFile ConvertImageToIFormFile(byte[] imageData, string name, string extension)
    {
        if (imageData == null || imageData.Length == 0 || string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(extension))
        { throw new ArgumentException("Parameters are invalid."); }

        using (var memoryStream = new MemoryStream(imageData))
        {
            var formFile = new FormFile(memoryStream, 0, imageData.Length, name, $"{name}{extension}");
            return formFile;
        }
    }
}

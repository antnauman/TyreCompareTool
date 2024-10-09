using OpenCvSharp;
using TyreCompare.Log;

namespace TyreCompare.ImageProcessor;

public class ImageTransformer
{
    public string InputDirectory { get; private set; }
    public string OutputDirectory { get; private set; }
    public IFileLogger FileLogger { get; private set; }
    public double Threshold { get; private set; }
    public double MaxValue { get; private set; }
    public double? DesiredWidth { get; private set; }
    public double? DesiredHeight { get; private set; }

    public ImageTransformer(IFileLogger fileLogger)
    {
        FileLogger = fileLogger;
    }

    public ImageTransformer(string inputDirectory, string outputDirectory, IFileLogger fileLogger)
    {
        InputDirectory = inputDirectory;
        OutputDirectory = outputDirectory;
        FileLogger = fileLogger;
    }

    /// <summary>
    /// This method will crop white space from the the images in Input Directory.
    /// It will also crop the image if values are provided.
    /// </summary>
    public async Task StartCroppingAsync(double threshold = 240, double maxValue = 255, double? desiredWidth = null, double? desiredHeight = null)
    {
        if (string.IsNullOrWhiteSpace(InputDirectory) || string.IsNullOrWhiteSpace(OutputDirectory))
        { throw new Exception("Input directory or Output directory is invalid."); }

        Threshold = threshold;
        MaxValue = maxValue;
        DesiredWidth = desiredWidth;
        DesiredHeight = desiredHeight;

        var files = GetFilesFromDirectory(InputDirectory);
        await Task.Run(() =>
        {
            Parallel.ForEach(files, file =>
            {
                CropFile(file);
            });
        });
    }

    /// <summary>
    /// This method will crop the images from the the images in Input Directory. Null value means aspect ratio will be maintained.
    /// </summary>
    public async Task StartResizingAsync(double? desiredWidth, double? desiredHeight)
    {
        if (string.IsNullOrWhiteSpace(InputDirectory) || string.IsNullOrWhiteSpace(OutputDirectory))
        { throw new Exception("Input directory or Output directory is invalid."); }

        if (desiredWidth <= 0 || desiredHeight <= 0)
        { throw new Exception("Desired Height or Width are invalid."); }

        DesiredWidth = desiredWidth;
        DesiredHeight = desiredHeight;

        var files = GetFilesFromDirectory(InputDirectory);
        await Task.Run(() =>
        {
            Parallel.ForEach(files, file =>
            {
                ResizeFile(file);
            });
        });
    }

    private void CropFile(string filePath)
    {
        try
        {
            if (!File.Exists(filePath))
            { throw new Exception($"File do not exist {filePath}"); }
            
            var fileName = Path.GetFileName(filePath);
            Mat originalMatrix = null;
            Mat finalMatrix = null;

            originalMatrix = new Mat(filePath, ImreadModes.Color);
            finalMatrix = CropImageMatrix(originalMatrix, Threshold, MaxValue);

            if (DesiredWidth.GetValueOrDefault() > 0 || DesiredHeight.GetValueOrDefault() > 0)
            {
                finalMatrix = ResizeMatrix(finalMatrix, DesiredWidth, DesiredHeight);
            }

            var outputFilePath = Path.Combine(OutputDirectory, Path.GetFileName(filePath));
            finalMatrix.SaveImage(outputFilePath);
            Console.WriteLine($"Cropped image {fileName}");
            finalMatrix.Dispose();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error cropping image {filePath}");
            FileLogger.Log($"Could not crop image {filePath}").Wait();
            FileLogger.Log(ex).Wait();
        }
    }

    private void ResizeFile(string filePath)
    {
        try
        {
            if (!File.Exists(filePath))
            { throw new Exception($"File do not exist {filePath}"); }

            var fileName = Path.GetFileName(filePath);
            var finalMatrix = ResizeImage(filePath);
            var outputFilePath = Path.Combine(OutputDirectory, Path.GetFileName(filePath));
            finalMatrix.SaveImage(outputFilePath);

            Console.WriteLine($"Reszied image {fileName}");
            finalMatrix.Dispose();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error resizing image {filePath}");
            FileLogger.Log($"Could not resizing image {filePath}").Wait();
            FileLogger.Log(ex).Wait();
        }
    }

    public Mat ResizeImage(string imagePath)
    {
        using var sourceMatrix = new Mat(imagePath, ImreadModes.Color);
        var resized = ResizeMatrix(sourceMatrix, DesiredWidth, DesiredHeight);
        return resized;
    }

    public static byte[] CropResizeImage(byte[] imageData, string imageName, double threshold, double maxValue, double? desiredWidth, double? desiredHeight)
    {
        if (imageData == null || string.IsNullOrWhiteSpace(imageName))
        { throw new Exception("Parameters are invalid."); }

        using var originalMatrix = Mat.FromImageData(imageData, ImreadModes.Color);
        var finalMatrix = CropImageMatrix(originalMatrix, threshold, maxValue);

        if (desiredWidth.GetValueOrDefault() > 0 || desiredHeight.GetValueOrDefault() > 0)
        { finalMatrix = ResizeMatrix(finalMatrix, desiredWidth, desiredHeight); }

        finalMatrix.SaveImage(@$"D:\{imageName}");
        var resizedImageData = finalMatrix.ToBytes(Path.GetExtension(imageName));
        return resizedImageData;
    }

    public static byte[] ResizeImage(byte[] imageData, string imageName, double? desiredWidth, double? desiredHeight)
    {
        if (imageData == null || string.IsNullOrWhiteSpace(imageName))
        { throw new Exception("Parameters are invalid."); }

        using var sourceMatrix = Mat.FromImageData(imageData);
        var resizedMatrix = ResizeMatrix(sourceMatrix, desiredWidth, desiredHeight);
        var resizedImageData = resizedMatrix.ToBytes(Path.GetExtension(imageName));
        return resizedImageData;
    }

    private static Mat CropImageMatrix(Mat sourceMatrix, double threshold, double maxValue)
    {
        using var grayMatrix = new Mat();
        using var binaryMatrix = new Mat();

        Cv2.CvtColor(sourceMatrix, grayMatrix, ColorConversionCodes.BGR2GRAY);
        Cv2.Threshold(grayMatrix, binaryMatrix, threshold, maxValue, ThresholdTypes.BinaryInv); // Thresholding to detect non-white regions

        var contours = Cv2.FindContoursAsArray(binaryMatrix, RetrievalModes.External, ContourApproximationModes.ApproxSimple);

        if (contours.Length > 0)
        {
            Rect boundingBox = Cv2.BoundingRect(contours[0]);

            foreach (var contour in contours)
            {
                Rect tempBox = Cv2.BoundingRect(contour);
                boundingBox = Rect.Union(boundingBox, tempBox); // Union of all bounding boxes to ensure the entire tire is captured
            }
            var cropped = new Mat(sourceMatrix, boundingBox);
            return cropped;
        }
        throw new Exception("Contour length is zero.");
    }

    private static Mat ResizeMatrix(Mat imageMatrix, double? desiredWidth, double? desiredHeight)
    {
        double widthRatio = (double)desiredWidth.GetValueOrDefault() / imageMatrix.Width;
        double heightRatio = (double)desiredHeight.GetValueOrDefault() / imageMatrix.Height;

        // Calculate the scaling factor
        double widthScale = (desiredWidth.GetValueOrDefault() > 0) ? widthRatio : heightRatio;
        double heightScale = (desiredHeight.GetValueOrDefault() > 0) ? heightRatio : widthRatio;

        // Calculate the new height
        int newWidth = (int)(imageMatrix.Width * widthScale);
        int newHeight = (int)(imageMatrix.Height * heightScale);

        var resizedMatrix = new Mat();
        Cv2.Resize(imageMatrix, resizedMatrix, new Size(newWidth, newHeight));

        return resizedMatrix;
    }

    private List<string> GetFilesFromDirectory(string directoryPath)
    {
        var validExtensions = new[] { ".jpg", ".jpeg", ".png" };
        var allFiles = Directory.EnumerateFiles(directoryPath)
                             .Where(file => validExtensions.Contains(Path.GetExtension(file).ToLowerInvariant()))
                             .ToList();
        return allFiles;
    }
}
using OpenCvSharp;
using TyreCompare.Log;

namespace TyreCompare.ImageProcessor;

public class ImageCopier
{
    private readonly ILog Logger;
    private string SourcePath;
    private string DestinationPath;

    public ImageCopier(string sourcePath, string destinationPath, ILog logger)
    {
        SourcePath = sourcePath;
        DestinationPath = destinationPath;
        Logger = logger;
    }

    public void CompareAndCopyImages(string comparePath)
    {
        if (string.IsNullOrWhiteSpace(SourcePath) || string.IsNullOrWhiteSpace(DestinationPath) || string.IsNullOrWhiteSpace(comparePath))
        { throw new Exception("Source Path, Destination Path or Compare Path is empty."); }

        try
        {
            var sourceImagePaths = Directory.GetFiles(comparePath);
            var sourceImageNames = sourceImagePaths.Select(x => Path.GetFileName(x));

            var copyFromImagePaths = Directory.GetFiles(SourcePath);
            var copyFromImageNames = sourceImagePaths.Select(x => Path.GetFileName(x));

            var fileNamesToCopy = sourceImageNames.Intersect(copyFromImageNames).ToList();
            var filePathsToCopy = fileNamesToCopy.Select(x => Path.Combine(SourcePath, x)).ToList();

            CopyFiles(filePathsToCopy, DestinationPath);
        }
        catch (Exception ex)
        {
            Logger.Log(ex);
        }
    }

    public void CopyImagesWithHeightToWidthRatio(double lowerRatio, double upperRatio)
    {
        if (string.IsNullOrWhiteSpace(SourcePath) || string.IsNullOrWhiteSpace(DestinationPath))
        { throw new Exception("Source Path, Destination Path is empty."); }

        var filePaths = Directory.GetFiles(SourcePath);
        foreach (var filePath in filePaths)
        {
            using var sourceMatrix = new Mat(filePath, ImreadModes.Color);
            double imageRatio = (double)sourceMatrix.Height/(double)sourceMatrix.Width;
            if (imageRatio > lowerRatio && imageRatio < upperRatio)
            {
                CopyFile(filePath, DestinationPath, Logger);
            }
        }
    }

    private void CopyFiles(IEnumerable<string> sourceFiles, string destinationPath)
    {
        if (sourceFiles?.Any() != true || string.IsNullOrWhiteSpace(destinationPath))
        { throw new Exception("Parameters are invalid."); }

        sourceFiles.ToList().ForEach(sourceImagePath =>
        {
            CopyFile(sourceImagePath, destinationPath, Logger);
        });
    }

    private static void CopyFile(string sourceFile, string destinationPath, ILog logger)
    {
        var sourceFileName = Path.GetFileName(sourceFile);
        try
        {
            //File.Copy(sourceFile, Path.Combine(destinationPath, sourceFileName));
            File.Move(sourceFile, Path.Combine(destinationPath, sourceFileName), true);
            Console.WriteLine($"Copied image {sourceFileName}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Cannot copy file {sourceFileName}");
            logger.Log($"Cannot copy file {sourceFileName}"); logger.Log(ex);
        }
    }
}

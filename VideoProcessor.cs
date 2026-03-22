using System;
using System.Collections.Generic;
using OpenCvSharp;

namespace VideoMosaic;

public class VideoProcessor
{
    private static MosaicTile BinaryClosestLuminance(List<MosaicTile> library, double target)
    {
        int low = 0;
        int high = library.Count - 1;

        if (target <= library[0].Luminance) return library[0];
        if (target >= library[high].Luminance) return library[high];

        while (low <= high)
        {
            int mid = low + (high - low) / 2;
            double midVal = library[mid].Luminance;

            if (midVal < target)
            {
                low = mid + 1;
            }
            else if (midVal > target)
            {
                high = mid - 1;
            }
            else
            {
                return library[mid];
            }
        }

        double diffLow = Math.Abs(library[low].Luminance - target);
        double diffHigh = Math.Abs(library[high].Luminance - target);

        if (diffLow < diffHigh)
        {
            return library[low];
        }
        else
        {
            return library[high];
        }
    }

    public static void Process(string inputPath, string outputPath, List<MosaicTile> tileLibrary, int blockSize)
    {
        Console.WriteLine($"\nPhase 2: Opening Video Canvas -> {inputPath}");

        using var capture = new VideoCapture(inputPath);
        if (!capture.IsOpened()) return;

        int sourceWidth = capture.FrameWidth;
        int sourceHeight = capture.FrameHeight;
        int fps = (int)capture.Fps;


        int sourceChunkSize = 60;
        int multiplier = blockSize / sourceChunkSize;
        int outputWidth = sourceWidth * multiplier;
        int outputHeight = sourceHeight * multiplier;

        using var writer = new VideoWriter(
            outputPath,
            VideoWriter.FourCC('m', 'p', '4', 'v'),
            fps,
            new Size(outputWidth, outputHeight)
            );
        if (!writer.IsOpened()) { return; }

        using var frame = new Mat();
        int frameCount = 0;

        Mat outputFrame = new Mat(outputHeight, outputWidth, MatType.CV_8UC3);

        while (capture.Read(frame))
        {
            using Mat frameGrey = new Mat();
            Cv2.CvtColor(frame, frameGrey, ColorConversionCodes.BGR2GRAY);
            frameCount++;
            Console.WriteLine($"Processing Frame {frameCount}...");

            for (int y = 0; y < sourceHeight; y += sourceChunkSize)
            {
                for (int x = 0; x < sourceWidth; x += sourceChunkSize)
                {
                    Rect chunkRect = new Rect(x, y, sourceChunkSize, sourceChunkSize);
                    using Mat chunkGrey = new Mat(frameGrey, chunkRect);

                    Scalar meanValue = Cv2.Mean(chunkGrey);
                    double targetLuminance = meanValue.Val0;

                    MosaicTile bestMatch = BinaryClosestLuminance(tileLibrary, meanValue.Val0);

                    int destX = x * multiplier;
                    int destY = y * multiplier;
                    Rect destRect = new Rect(destX, destY, blockSize, blockSize);

                    using (Mat roi = new Mat(outputFrame, destRect))
                    {
                        bestMatch.Image.CopyTo(roi);
                    }
                }
            }
            writer.Write(outputFrame);
            if (frameCount > 20) { break; }
        }
    }
}

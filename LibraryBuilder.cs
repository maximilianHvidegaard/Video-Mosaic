using System;
using System.Collections.Generic;
using System.IO;
using OpenCvSharp;

namespace VideoMosaic;

public class LibraryBuilder
{
    public static List<MosaicTile> BuildLibrary(string folderPath, int blockSize)
    {
        List<MosaicTile> tiles = new List<MosaicTile>();

        if (!Directory.Exists(folderPath))
        {
            Console.WriteLine($"CRITICAL: Cannot find {folderPath}");
            return tiles;
        }

        string[] files = Directory.GetFiles(folderPath);

        foreach (string stringName in files)
        {
            Mat original = Cv2.ImRead(stringName);

            if (original.Empty()) { continue; }

            Mat resized = new Mat();
            Cv2.Resize(original, resized, new Size(blockSize, blockSize));

            Mat grey = new Mat();
            Cv2.CvtColor(resized, grey, ColorConversionCodes.BGR2GRAY);

            Scalar meanValue = Cv2.Mean(grey);

            MosaicTile newTile = new MosaicTile()
            {
                Image = resized,
                Luminance = meanValue.Val0
            };

            tiles.Add(newTile);

            original.Dispose();
            grey.Dispose();
        }
        // 6. Sort the list before returning
        tiles.Sort((a, b) => a.Luminance.CompareTo(b.Luminance));

        return tiles;
    }
}

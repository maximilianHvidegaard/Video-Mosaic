using System;
using System.Collections.Generic;

namespace VideoMosaic;

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("Initializing Video Mosaic Engine...");

        // 1. Configuration
        string libraryPath = "ImageLibrary";
        string sourceVideo = "SourceAssets/7690131-hd_1080_1920_30fps.mp4";
        string outputVideo = "Output/final_mosaic.mp4";
        int blockSize = 30;

        List<MosaicTile> tileLibrary = LibraryBuilder.BuildLibrary(libraryPath, blockSize);

        if (tileLibrary.Count == 0)
        {
            Console.WriteLine("CRITICAL: Engine failed to load image library. Exiting.");
            return;
        }

        Console.WriteLine($"SUCCESS: Loaded {tileLibrary.Count} tiles into memory.");
        Console.WriteLine($"Darkest Tile: {tileLibrary[0].Luminance:F2}");
        Console.WriteLine($"Brightest Tile: {tileLibrary[^1].Luminance:F2}");

        VideoProcessor.Process(sourceVideo, outputVideo, tileLibrary, blockSize);
    }
}

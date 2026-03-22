using System;
using OpenCvSharp;

namespace VideoMosaic;

public class MosaicTile
{
    public Mat? Image { get; set; }
    public double Luminance { get; set; }
}



using SkiaSharp;
using System.Runtime.InteropServices;

namespace NoiseTesting.Imaging.Services;

public static class ImageGenerator
{
    public static byte[] Generate(float[,] grid, Func<int, int, float, SKColor> colorFunc)
    {
        using SKBitmap bitmap = new SKBitmap(grid.GetLength(0), grid.GetLength(1));

        Span<float> floats = MemoryMarshal.CreateSpan(ref grid[0, 0], grid.Length);
        Span<SKColor> pixels = MemoryMarshal.Cast<byte, SKColor>(bitmap.GetPixelSpan());

        for (int i = 0; i < pixels.Length; i++)
        {
            // Flip the x- and y axis.
            int x = i % grid.GetLength(0);
            int y = i / grid.GetLength(0);
            int j = y + (x * grid.GetLength(0));

            pixels[j] = colorFunc(0, 0, floats[i]);
        }

        using SKImage image = SKImage.FromBitmap(bitmap);
        using SKData data = image.Encode(SKEncodedImageFormat.Png, 100);
        return data.AsSpan().ToArray();
    }
}

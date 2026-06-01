using SkiaSharp;

namespace NoiseTesting.Imaging.Services;

public class ImageGenerator
{
    public byte[] Generate(float[,] grid, Func<int, int, float, SKColor> colorFunc)
    {
        using SKBitmap bitmap = new SKBitmap(grid.GetLength(0), grid.GetLength(1));

        for (int y = 0; y < grid.GetLength(1); y++)
        {
            for (int x = 0; x < grid.GetLength(0); x++)
            {
                float value = grid[x, y];
                bitmap.SetPixel(x, y, colorFunc(x, y, value));
            }
        }

        using SKImage image = SKImage.FromBitmap(bitmap);
        using SKData data = image.Encode(SKEncodedImageFormat.Png, 100);
        return data.AsSpan().ToArray();
    }
}

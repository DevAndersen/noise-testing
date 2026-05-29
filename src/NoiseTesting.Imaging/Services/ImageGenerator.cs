using SkiaSharp;

namespace NoiseTesting.Imaging.Services;

public class ImageGenerator
{
    public byte[] Generate(float[,] grid)
    {
        using SKBitmap bitmap = new SKBitmap(grid.GetLength(0), grid.GetLength(1));

        for (int y = 0; y < grid.GetLength(1); y++)
        {
            for (int x = 0; x < grid.GetLength(0); x++)
            {
                float value = grid[x, y];
                byte colorValue = (byte)float.Lerp(byte.MinValue, byte.MaxValue, value);

                SKColor color = value switch
                {
                    < 0.30F => new SKColor(0, (byte)float.Max(0, colorValue - 40), (byte)(colorValue + 120)),
                    < 0.40F => new SKColor(0, (byte)float.Max(0, colorValue - 40), (byte)(colorValue + 150)),
                    < 0.43F => new SKColor((byte)(colorValue + 120), (byte)(colorValue + 100), 0),
                    < 0.75F => new SKColor(32, (byte)(250 - colorValue), 32),
                    < 0.90F => new SKColor((byte)((colorValue - 300) * 4), (byte)((colorValue - 300) * 4), (byte)((colorValue - 300) * 4)),
                    _ => new SKColor(colorValue, colorValue, colorValue)
                };

                bitmap.SetPixel(x, y, color);
            }
        }

        using SKImage image = SKImage.FromBitmap(bitmap);
        using SKData data = image.Encode(SKEncodedImageFormat.Png, 100);
        return data.AsSpan().ToArray();
    }
}

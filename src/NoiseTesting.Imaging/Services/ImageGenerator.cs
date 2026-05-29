using SkiaSharp;

namespace NoiseTesting.Imaging.Services;

public class ImageGenerator
{
    public byte[] Generate()
    {
        using SKBitmap bitmap = new SKBitmap(128, 128);

        // Placeholder
        for (int x = 0; x < bitmap.Width; x++)
        {
            bitmap.SetPixel(x, x, new SKColor(255, 0, 0));
            bitmap.SetPixel(bitmap.Width - x - 1, x, new SKColor(255, 0, 0));
        }

        using SKImage image = SKImage.FromBitmap(bitmap);
        using SKData data = image.Encode(SKEncodedImageFormat.Png, 100);

        return data.AsSpan().ToArray();
    }
}

using SkiaSharp;

namespace NoiseTesting.Web.Helpers;

public static class TileHelper
{
    /// <summary>
    /// Calculate the average of corresponding values across <paramref name="grids"/>.
    /// </summary>
    /// <param name="grids"></param>
    /// <returns></returns>
    public static float[,] Combine(params float[][,] grids)
    {
        int width = grids[0].GetLength(0);
        int height = grids[0].GetLength(1);

        float[,] result = new float[width, height];

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                float f = grids.Sum(grid => grid[x, y]) / grids.Length;

                result[x, y] = Sigmoid(f, 3.0F, 0.0F);
            }
        }

        return result;
    }

    public static SKColor GradientColor(float value)
    {
        byte colorValue = (byte)float.Lerp(byte.MinValue, byte.MaxValue, value);
        return new SKColor(colorValue, colorValue, colorValue);
    }

    public static SKColor TerrainColor(float value)
    {
        byte colorValue = (byte)float.Lerp(byte.MinValue, byte.MaxValue, value);

        return value switch
        {
            // Deep ocean
            < 0.30F => new SKColor(0, (byte)float.Max(0, colorValue - 40), (byte)(colorValue + 120)),

            // Shallow ocean
            < 0.40F => new SKColor(0, (byte)float.Max(0, colorValue - 40), (byte)(colorValue + 150)),

            // Beach
            < 0.43F => new SKColor((byte)(colorValue + 120), (byte)(colorValue + 100), 0),

            // Land
            < 0.75F => new SKColor(32, (byte)(250 - colorValue), 32),

            // Mountain
            < 0.90F => new SKColor((byte)((colorValue - 300) * 4), (byte)((colorValue - 300) * 4), (byte)((colorValue - 300) * 4)),

            // Mountain peak
            _ => new SKColor(colorValue, colorValue, colorValue)
        };
    }

    public static SKColor TerrainTemperatureColor(float value, float temperature, float river)
    {
        byte colorValue = (byte)float.Lerp(byte.MinValue, byte.MaxValue, value);

        return value switch
        {
            // Iceberg
            < 0.15F when temperature < 0.1F => new SKColor(191, 202, 255),

            // Deep ocean
            < 0.30F => new SKColor(0, (byte)float.Max(0, colorValue - 40), (byte)(colorValue + 120)),

            // Cold shallow ocean
            < 0.40F when temperature < 0.3 => new SKColor(145, 186, 207),

            // Shallow ocean
            < 0.40F => new SKColor(0, (byte)float.Max(0, colorValue - 40), (byte)(colorValue + 150)),

            // River
            < 0.75F when river is > 0.495F and < 0.51F => new SKColor(119, 208, 235),

            // Cold land
            < 0.75F when temperature < 0.3 => new SKColor((byte)(70 - colorValue), (byte)(70 - colorValue), (byte)(70 - colorValue)),

            // Beach
            < 0.43F => new SKColor((byte)(colorValue + 120), (byte)(colorValue + 100), 0),

            // Land
            < 0.75F => new SKColor(32, (byte)(250 - colorValue), 32),

            // Mountain
            < 0.90F => new SKColor((byte)((colorValue - 300) * 4), (byte)((colorValue - 300) * 4), (byte)((colorValue - 300) * 4)),

            // Mountain peak
            _ => new SKColor(colorValue, colorValue, colorValue)
        };
    }

    /// <summary>
    /// Sigmoid function with adjustable multiplier (how steep the curve is) and bias (shifts the middle point of the curve).
    /// </summary>
    /// <param name="x"></param>
    /// <param name="mult"></param>
    /// <param name="bias"></param>
    /// <returns></returns>
    public static float Sigmoid(float x, float mult, float bias)
    {
        return 1 / (1 + float.Pow(float.E, -bias + -x * (mult * 2) + mult));
    }

    /// <summary>
    /// Sigmoid function with adjustable multiplier (how steep the curve is), bias (shifts the middle point of the curve), minimum, and maximum.
    /// </summary>
    /// <param name="x"></param>
    /// <param name="mult"></param>
    /// <param name="bias"></param>
    /// <param name="min"></param>
    /// <param name="max"></param>
    /// <returns></returns>
    public static float SigmoidMinMax(float x, float mult, float bias, float min, float max)
    {
        float sigmoid = Sigmoid(x, mult, bias);
        return sigmoid * max + (min - sigmoid * min);
    }
}

using NoiseTesting.Imaging.Services;
using NoiseTesting.Noise.Services;
using SkiaSharp;

namespace NoiseTesting.Web.Components.Pages;

public partial class Home
{
    private static IEnumerable<ImageLayer> GenerateNoiseImages(int size, int posX, int posY)
    {
        PerlinNoiseGenerator terrainNoiseGenerator = new PerlinNoiseGenerator(1);
        PerlinNoiseGenerator landNoiseGenerator = new PerlinNoiseGenerator(3);
        PerlinNoiseGenerator mountainNoiseGenerator = new PerlinNoiseGenerator(4);
        PerlinNoiseGenerator temperatureNoiseGenerator = new PerlinNoiseGenerator(5);
        PerlinNoiseGenerator riverNoiseGenerator = new PerlinNoiseGenerator(7);

        float[,] terrainNoise = terrainNoiseGenerator.GenerateNoise(size, 64, 5, posX, posY, f => Sigmoid(f, 2.5F, 0.0F));
        yield return new ImageLayer("Terrain", ImageGenerator.Generate(terrainNoise, (_, _, value) => GradientColor(value)));

        float[,] landNoise = landNoiseGenerator.GenerateNoise(size, 256, 6, posX, posY, f => SigmoidMinMax(f, mult: 20, bias: -1.0F, min: 0.0F, max: 1.0F));
        yield return new ImageLayer("Land", ImageGenerator.Generate(landNoise, (_, _, value) => GradientColor(value)));

        float[,] mountainNoise = mountainNoiseGenerator.GenerateNoise(size, 126, 4, posX, posY, f => SigmoidMinMax(f, mult: 15.0F, bias: -2.0F, min: 0.0F, max: 1.0F));
        yield return new ImageLayer("Mountain", ImageGenerator.Generate(mountainNoise, (_, _, value) => GradientColor(value)));

        float[,] temperatureNoise = temperatureNoiseGenerator.GenerateNoise(size, 128, 3, posX, posY, f => Sigmoid(f, 10.5F, 0.0F));
        yield return new ImageLayer("Temperature", ImageGenerator.Generate(temperatureNoise, (_, _, value) => GradientColor(value)));

        float[,] riverNoise = riverNoiseGenerator.GenerateNoise(size, 64, 5, posX, posY, f => Sigmoid(f, 2.5F, 0.0F));
        yield return new ImageLayer("River", ImageGenerator.Generate(riverNoise, (_, _, value) => GradientColor(value is > 0.495F and < 0.51F ? 1F : 0F)));

        float[,] color = Combine(terrainNoise, landNoise, mountainNoise);
        yield return new ImageLayer("Color", ImageGenerator.Generate(color, (_, _, value) => TerrainColor(value)));

        yield return new ImageLayer("Color with features", ImageGenerator.Generate(color, (x, y, value) => TerrainTemperatureColor(value, temperatureNoise[x, y], riverNoise[x, y])));
    }

    private static byte[] GenerateTerrainImages(int size, int posX, int posY)
    {
        PerlinNoiseGenerator terrainNoiseGenerator = new PerlinNoiseGenerator(1);
        PerlinNoiseGenerator landNoiseGenerator = new PerlinNoiseGenerator(3);
        PerlinNoiseGenerator mountainNoiseGenerator = new PerlinNoiseGenerator(4);
        PerlinNoiseGenerator temperatureNoiseGenerator = new PerlinNoiseGenerator(5);
        PerlinNoiseGenerator riverNoiseGenerator = new PerlinNoiseGenerator(7);

        float[,] terrainNoise = terrainNoiseGenerator.GenerateNoise(size, 64, 5, posX, posY, f => Sigmoid(f, 2.5F, 0.0F));

        float[,] landNoise = landNoiseGenerator.GenerateNoise(size, 256, 6, posX, posY, f => SigmoidMinMax(f, mult: 20, bias: -1.0F, min: 0.0F, max: 1.0F));

        float[,] mountainNoise = mountainNoiseGenerator.GenerateNoise(size, 126, 4, posX, posY, f => SigmoidMinMax(f, mult: 15.0F, bias: -2.0F, min: 0.0F, max: 1.0F));

        float[,] temperatureNoise = temperatureNoiseGenerator.GenerateNoise(size, 128, 3, posX, posY, f => Sigmoid(f, 10.5F, 0.0F));

        float[,] color = Combine(terrainNoise, landNoise, mountainNoise);

        float[,] riverNoise = riverNoiseGenerator.GenerateNoise(size, 64, 5, posX, posY, f => Sigmoid(f, 2.5F, 0.0F));

        return ImageGenerator.Generate(color, (x, y, value) => TerrainTemperatureColor(value, temperatureNoise[x, y], riverNoise[x, y]));
    }

    private static SKColor GradientColor(float value)
    {
        byte colorValue = (byte)float.Lerp(byte.MinValue, byte.MaxValue, value);
        return new SKColor(colorValue, colorValue, colorValue);
    }

    private static SKColor TerrainColor(float value)
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

    private static SKColor TerrainTemperatureColor(float value, float temperature, float river)
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
    /// Calculate the average of corresponding values across <paramref name="grids"/>.
    /// </summary>
    /// <param name="grids"></param>
    /// <returns></returns>
    private static float[,] Combine(params float[][,] grids)
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

    /// <summary>
    /// Sigmoid function with adjustable multiplier (how steep the curve is) and bias (shifts the middle point of the curve).
    /// </summary>
    /// <param name="x"></param>
    /// <param name="mult"></param>
    /// <param name="bias"></param>
    /// <returns></returns>
    private static float Sigmoid(float x, float mult, float bias)
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
    private static float SigmoidMinMax(float x, float mult, float bias, float min, float max)
    {
        float sigmoid = Sigmoid(x, mult, bias);
        return sigmoid * max + (min - sigmoid * min);
    }

    private record ImageLayer(string Name, byte[] ImageBytes);
}

using NoiseTesting.Imaging.Services;
using NoiseTesting.Noise.Services;
using static NoiseTesting.Web.Helpers.TileHelper;

namespace NoiseTesting.Web.Components.Pages;

public partial class Layers
{
    private static readonly PerlinNoiseGenerator _terrainNoiseGenerator = new PerlinNoiseGenerator(1);
    private static readonly PerlinNoiseGenerator _landNoiseGenerator = new PerlinNoiseGenerator(3);
    private static readonly PerlinNoiseGenerator _mountainNoiseGenerator = new PerlinNoiseGenerator(4);
    private static readonly PerlinNoiseGenerator _temperatureNoiseGenerator = new PerlinNoiseGenerator(5);
    private static readonly PerlinNoiseGenerator _riverNoiseGenerator = new PerlinNoiseGenerator(7);

    private static IEnumerable<ImageLayer> GenerateNoiseImages(int size, int posX, int posY)
    {
        float[,] terrainNoise = _terrainNoiseGenerator.GenerateNoise(size, 64, 5, posX, posY, f => Sigmoid(f, 2.5F, 0.0F));
        yield return new ImageLayer("Terrain", ImageGenerator.Generate(terrainNoise, (_, _, value) => GradientColor(value)));

        float[,] landNoise = _landNoiseGenerator.GenerateNoise(size, 256, 6, posX, posY, f => SigmoidMinMax(f, mult: 20, bias: -1.0F, min: 0.0F, max: 1.0F));
        yield return new ImageLayer("Land", ImageGenerator.Generate(landNoise, (_, _, value) => GradientColor(value)));

        float[,] mountainNoise = _mountainNoiseGenerator.GenerateNoise(size, 126, 4, posX, posY, f => SigmoidMinMax(f, mult: 15.0F, bias: -2.0F, min: 0.0F, max: 1.0F));
        yield return new ImageLayer("Mountain", ImageGenerator.Generate(mountainNoise, (_, _, value) => GradientColor(value)));

        float[,] temperatureNoise = _temperatureNoiseGenerator.GenerateNoise(size, 128, 3, posX, posY, f => Sigmoid(f, 10.5F, 0.0F));
        yield return new ImageLayer("Temperature", ImageGenerator.Generate(temperatureNoise, (_, _, value) => GradientColor(value)));

        float[,] riverNoise = _riverNoiseGenerator.GenerateNoise(size, 64, 5, posX, posY, f => Sigmoid(f, 2.5F, 0.0F));
        yield return new ImageLayer("River", ImageGenerator.Generate(riverNoise, (_, _, value) => GradientColor(value is > 0.495F and < 0.51F ? 1F : 0F)));

        float[,] color = Combine(terrainNoise, landNoise, mountainNoise);
        yield return new ImageLayer("Color", ImageGenerator.Generate(color, (_, _, value) => TerrainColor(value)));

        yield return new ImageLayer("Color with features", ImageGenerator.Generate(color, (x, y, value) => TerrainTemperatureColor(value, temperatureNoise[x, y], riverNoise[x, y])));
    }

    private record ImageLayer(string Name, byte[] ImageBytes);
}

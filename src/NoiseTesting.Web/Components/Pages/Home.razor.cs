using NoiseTesting.Imaging.Services;
using NoiseTesting.Noise.Services;
using static NoiseTesting.Web.Helpers.TileHelper;

namespace NoiseTesting.Web.Components.Pages;

public partial class Home
{
    private static readonly PerlinNoiseGenerator _terrainNoiseGenerator = new PerlinNoiseGenerator(1);
    private static readonly PerlinNoiseGenerator _landNoiseGenerator = new PerlinNoiseGenerator(3);
    private static readonly PerlinNoiseGenerator _mountainNoiseGenerator = new PerlinNoiseGenerator(4);
    private static readonly PerlinNoiseGenerator _temperatureNoiseGenerator = new PerlinNoiseGenerator(5);
    private static readonly PerlinNoiseGenerator _riverNoiseGenerator = new PerlinNoiseGenerator(7);

    private int PosX { get; set; }

    private int PosY { get; set; }

    private int Width { get; set; }

    private int Height { get; set; }

    private int Seed { get; set; }

    private static readonly Dictionary<ImageDetails, byte[]> _images = [];

    protected override void OnInitialized()
    {
        Width = 2;
        Height = 2;
    }

    private byte[] GetImageBytes(int posX, int posY)
    {
        ImageDetails details = new ImageDetails(posX, posY, Seed);

        if (!_images.TryGetValue(details, out byte[]? bytes))
        {
            bytes = _images[details] = GenerateTerrainImages(256, posX * 256, posY * 256);
        }

        return bytes;
    }

    private static byte[] GenerateTerrainImages(int size, int posX, int posY)
    {
        float[,] terrainNoise = _terrainNoiseGenerator.GenerateNoise(size, 64, 5, posX, posY, f => Sigmoid(f, 2.5F, 0.0F));

        float[,] landNoise = _landNoiseGenerator.GenerateNoise(size, 256, 6, posX, posY, f => SigmoidMinMax(f, mult: 20, bias: -1.0F, min: 0.0F, max: 1.0F));

        float[,] mountainNoise = _mountainNoiseGenerator.GenerateNoise(size, 126, 4, posX, posY, f => SigmoidMinMax(f, mult: 15.0F, bias: -2.0F, min: 0.0F, max: 1.0F));

        float[,] temperatureNoise = _temperatureNoiseGenerator.GenerateNoise(size, 128, 3, posX, posY, f => Sigmoid(f, 10.5F, 0.0F));

        float[,] color = Combine(terrainNoise, landNoise, mountainNoise);

        float[,] riverNoise = _riverNoiseGenerator.GenerateNoise(size, 64, 5, posX, posY, f => Sigmoid(f, 2.5F, 0.0F));

        return ImageGenerator.Generate(color, (x, y, value) => TerrainTemperatureColor(value, temperatureNoise[x, y], riverNoise[x, y]));
    }

    private void MoveUp()
    {
        PosY--;
    }

    private void MoveDown()
    {
        PosY++;
    }

    private void MoveLeft()
    {
        PosX--;
    }

    private void MoveRight()
    {
        PosX++;
    }

    private readonly record struct ImageDetails(int PosX, int PosY, int Seed);
}

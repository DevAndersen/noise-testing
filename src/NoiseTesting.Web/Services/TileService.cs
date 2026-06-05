using NoiseTesting.Imaging.Services;
using NoiseTesting.Noise.Services;
using System.Collections.Concurrent;
using static NoiseTesting.Web.Helpers.TileHelper;

namespace NoiseTesting.Web.Services;

public class TileService
{
    private static readonly ConcurrentDictionary<int, NoiseGeneratorSet> _generatorSets = [];
    private static readonly ConcurrentDictionary<ImageDetails, byte[]> _images = [];

    public byte[] GetImageBytes(int posX, int posY, int seed)
    {
        ImageDetails details = new ImageDetails(posX, posY, seed);

        if (!_images.TryGetValue(details, out byte[]? bytes))
        {
            bytes = _images[details] = GenerateTerrainImages(256, posX * 256, posY * 256, seed);
        }

        return bytes;
    }

    private static byte[] GenerateTerrainImages(int size, int posX, int posY, int seed)
    {
        NoiseGeneratorSet noise = GetNoiseGeneratorSet(seed);

        float[,] terrainNoise = noise.TerrainNoiseGenerator.GenerateNoise(size, 64, 5, posX, posY, f => Sigmoid(f, 2.5F, 0.0F));

        float[,] landNoise = noise.LandNoiseGenerator.GenerateNoise(size, 256, 6, posX, posY, f => SigmoidMinMax(f, mult: 20, bias: -1.0F, min: 0.0F, max: 1.0F));

        float[,] mountainNoise = noise.MountainNoiseGenerator.GenerateNoise(size, 126, 4, posX, posY, f => SigmoidMinMax(f, mult: 15.0F, bias: -2.0F, min: 0.0F, max: 1.0F));

        float[,] temperatureNoise = noise.TemperatureNoiseGenerator.GenerateNoise(size, 128, 3, posX, posY, f => Sigmoid(f, 10.5F, 0.0F));

        float[,] color = Combine(terrainNoise, landNoise, mountainNoise);

        float[,] riverNoise = noise.RiverNoiseGenerator.GenerateNoise(size, 64, 5, posX, posY, f => Sigmoid(f, 2.5F, 0.0F));

        //return ImageGenerator.Generate(color, (x, y, value) => TerrainTemperatureColor(value, temperatureNoise[x, y], riverNoise[x, y]));
        return ImageGenerator.Generate(color, (_, _, value) => TerrainColor(value));
    }

    private static NoiseGeneratorSet GetNoiseGeneratorSet(int seed)
    {
        if (!_generatorSets.TryGetValue(seed, out NoiseGeneratorSet? set))
        {
            set = _generatorSets[seed] = new NoiseGeneratorSet(seed);
        }

        return set;
    }

    private readonly record struct ImageDetails(int PosX, int PosY, int Seed);

    private class NoiseGeneratorSet
    {
        public int Seed { get; }

        public PerlinNoiseGenerator TerrainNoiseGenerator { get; }

        public PerlinNoiseGenerator LandNoiseGenerator { get; }

        public PerlinNoiseGenerator MountainNoiseGenerator { get; }

        public PerlinNoiseGenerator TemperatureNoiseGenerator { get; }

        public PerlinNoiseGenerator RiverNoiseGenerator { get; }

        public NoiseGeneratorSet(int seed)
        {
            Seed = seed;

            Random rand = new Random(seed);
            TerrainNoiseGenerator = new PerlinNoiseGenerator(rand.Next());
            LandNoiseGenerator = new PerlinNoiseGenerator(rand.Next());
            MountainNoiseGenerator = new PerlinNoiseGenerator(rand.Next());
            TemperatureNoiseGenerator = new PerlinNoiseGenerator(rand.Next());
            RiverNoiseGenerator = new PerlinNoiseGenerator(rand.Next());
        }
    }
}

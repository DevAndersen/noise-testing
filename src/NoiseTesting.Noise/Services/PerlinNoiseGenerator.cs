using System.Numerics;
using System.Runtime.InteropServices;
using System.Security.Cryptography;

namespace NoiseTesting.Noise.Services;

public class PerlinNoiseGenerator
{
    /// <summary>
    /// The size of each vector tile.
    /// Square root of 16, (the number of bytes of an MD5 digest).
    /// </summary>
    private static readonly int _tileSize = (int)float.Sqrt(MD5.HashSizeInBytes);

    /// <summary>
    /// The largest negative grid value.
    /// Distance from one corner to the opposite corner.
    /// </summary>
    private static readonly float _minGridValue = -float.Sqrt(2);

    /// <summary>
    /// The largest positive grid value.
    /// Distance from one corner to the opposite corner.
    /// </summary>
    private static readonly float _maxGridValue = float.Sqrt(2);

    /// <summary>
    /// Store for calculated noise vectors.
    /// </summary>
    private readonly Dictionary<Point, Vector2> _vectorStore = [];

    private readonly int _seed;

    public PerlinNoiseGenerator(int seed)
    {
        _seed = seed;
    }

    /// <summary>
    /// Generate multi-octave noise.
    /// </summary>
    /// <param name="size"></param>
    /// <param name="density"></param>
    /// <param name="octaves"></param>
    /// <param name="posX"></param>
    /// <param name="posY"></param>
    /// <returns></returns>
    public float[,] GenerateNoise(int size, int density, int octaves, int posX, int posY, Func<float, float>? transformFunc = null)
    {
        float[,] floatGrid = new float[size, size];
        Span<float> grid = MemoryMarshal.CreateSpan(ref floatGrid[0, 0], floatGrid.Length);


        float[] octaveLayers = new float[size * size * octaves];

        // Generate the noise octave layers.
        for (int i = 0; i < octaves; i++)
        {
            int steps = density / (int)float.Pow(2, i);

            int octaveSliceLength = size * size;

            Span<float> octave = octaveLayers.AsSpan().Slice(octaveSliceLength * i, octaveSliceLength);
            GenerateNoiseLevel(octave, steps, posX, posY, size);

            // Accumulate the layers, with diminishing impact.
            for (int j = 0; j < grid.Length; j++)
            {
                grid[j] += octave[j] * (1 / float.Pow(2, i));
            }
        }

        // Normalizes the grid to [0, 1], then apply the transform function if it has been defined.
        for (int i = 0; i < grid.Length; i++)
        {
            float value = grid[i];
            grid[i] = (value - _minGridValue) / (_maxGridValue - _minGridValue);
            if (transformFunc != null)
            {
                grid[i] = transformFunc(grid[i]);
            }
        }

        return floatGrid;
    }

    /// <summary>
    /// Generate a single layer of noise.
    /// </summary>
    /// <param name="octaveGridData"></param>
    /// <param name="steps"></param>
    /// <param name="posX"></param>
    /// <param name="posY"></param>
    /// <param name="gridSize"></param>
    private void GenerateNoiseLevel(Span<float> octaveGridData, int steps, int posX, int posY, int gridSize)
    {
        for (int y = 0; y < gridSize; y++)
        {
            int py = y + posY;
            int vy = py / steps;
            float ty = (float)(py % steps) / steps;

            for (int x = 0; x < gridSize; x++)
            {
                int px = x + posX;
                int vx = px / steps;
                float tx = (float)(px % steps) / steps;

                Vector2 v00 = GetVector(vx, vy);
                Vector2 v01 = GetVector(vx, vy + 1);
                Vector2 v10 = GetVector(vx + 1, vy);
                Vector2 v11 = GetVector(vx + 1, vy + 1);

                int index = y + (x * gridSize);

                octaveGridData[index] = Interpolate(v00, v01, v10, v11, tx, ty);
            }
        }
    }

    /// <summary>
    /// Returns the vector for position <c>[x, y]</c>, generating it not yet defined.
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <returns></returns>
    private Vector2 GetVector(int x, int y)
    {
        // Generate the vector tile if it has not yet been generated.
        if (!_vectorStore.TryGetValue(new Point(x, y), out Vector2 vector))
        {
            int tileX = x / _tileSize;
            int tileY = y / _tileSize;

            Vector2[,] vectors = new Vector2[_tileSize, _tileSize];

            Span<Vector2> vectorSpan = MemoryMarshal.CreateSpan(ref vectors[0, 0], vectors.Length);
            Span<byte> workBuffer = stackalloc byte[MD5.HashSizeInBytes];
            PopulateNoiseVectors(vectorSpan, workBuffer, [tileX, tileY, _seed]);

            for (int relativeY = 0; relativeY < _tileSize; relativeY++)
            {
                for (int relativeX = 0; relativeX < _tileSize; relativeX++)
                {
                    int vectorX = (tileX * _tileSize) + relativeX;
                    int vectorY = (tileY * _tileSize) + relativeY;
                    _vectorStore[new Point(vectorX, vectorY)] = vectors[relativeX, relativeY];
                }
            }

            vector = vectors[x % _tileSize, y % _tileSize];
        }

        return vector;
    }

    /// <summary>
    /// Determine the value of the specified coordinate relative to four corner vectors.
    /// </summary>
    /// <param name="v00">The top-left vector.</param>
    /// <param name="v01">The top-right vector.</param>
    /// <param name="v10">The bottom-left vector.</param>
    /// <param name="v11">The bottom-right vector.</param>
    /// <param name="relativeX">The X-coordinate relative to the vector square.</param>
    /// <param name="relativeY">The Y-coordinate relative to the vector square.</param>
    /// <returns></returns>
    private static float Interpolate(Vector2 v00, Vector2 v01, Vector2 v10, Vector2 v11, float relativeX, float relativeY)
    {
        // Calculate dot products between each corner vector and a vector point from the relative coordinate to the corner vectors.
        float d00 = Vector2.Dot(v00, new Vector2(relativeX, relativeY));
        float d01 = Vector2.Dot(v01, new Vector2(relativeX, relativeY - 1));
        float d10 = Vector2.Dot(v10, new Vector2(relativeX - 1, relativeY));
        float d11 = Vector2.Dot(v11, new Vector2(relativeX - 1, relativeY - 1));

        // Interpolate between the four dot products.
        float topInterpolation = Interpolate(d00, d10, relativeX);
        float bottomInterpolation = Interpolate(d01, d11, relativeX);
        return Interpolate(topInterpolation, bottomInterpolation, relativeY);
    }

    /// <summary>
    /// Improved smoothstep interpolation.
    /// </summary>
    /// <param name="f1"></param>
    /// <param name="f2"></param>
    /// <param name="relative"></param>
    /// <returns></returns>
    private static float Interpolate(float f1, float f2, float relative)
    {
        float s = (relative * (relative * 6 - 15) + 10) * relative * relative * relative;
        return (f2 - f1) * s + f1;
    }

    /// <summary>
    /// Populates <paramref name="vectors"/> with pseudo-random unit vectors, based on the elements of <paramref name="input"/>.
    /// </summary>
    /// <remarks>
    /// Both components of each vector is derived from a single byte value.
    /// The populated vectors therefore have 256 possible permutations.
    /// </remarks>
    /// <param name="vectors">The vectors to be populated. Must contain between 1 and 16 elements.</param>
    /// <param name="workBuffer">The buffer used to store temporary data. Must have a length of at least 16 bytes.</param>
    /// <param name="input">The input data, used as the seed for the vector population. Must contain at least one value.</param>
    /// <returns></returns>
    private static void PopulateNoiseVectors(Span<Vector2> vectors, Span<byte> workBuffer, ReadOnlySpan<int> input)
    {
        // Generate pseudo-random binary data from the input arguments.
        MD5.HashData(MemoryMarshal.Cast<int, byte>(input), workBuffer);

        // Read numbers from the digest buffer, and use them to create unit vectors.
        for (int i = 0; i < vectors.Length; i++)
        {
            // Calculate the angle from the read number, ensuring even distribution, and avoiding perfectly straight angles.
            float angle = workBuffer[i] * (float.Tau / (byte.MaxValue + 1)) + float.E;

            // Populate the indexed vector data.
            vectors[i] = new Vector2(
                float.Sin(angle),
                float.Cos(angle));
        }
    }

    private readonly record struct Point(int X, int Y);
}

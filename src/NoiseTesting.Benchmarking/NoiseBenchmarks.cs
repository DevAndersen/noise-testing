using BenchmarkDotNet.Attributes;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace NoiseTesting.Benchmarking;

[MemoryDiagnoser]
public class NoiseBenchmarks
{
    private static readonly float _minGridValue = -float.Sqrt(2);
    private static readonly float _maxGridValue = float.Sqrt(2);

    private const int _size = 256;
    private const int _density = 64;
    private const int _octaves = 5;

    public void Debug()
    {
        float[,] v1 = new NoiseBenchmarks().GenerateNoise_2D();
        float[] v2 = new NoiseBenchmarks().GenerateNoise_1D();
        float[] v3 = new NoiseBenchmarks().GenerateNoise_1D_Flattened();
        float[] v4 = new NoiseBenchmarks().GenerateNoise_1D_Flattened2();

        Span<float> span1 = MemoryMarshal.CreateSpan(ref v1[0, 0], v1.Length);
        Span<float> span2 = v2.AsSpan();
        Span<float> span3 = v3.AsSpan();
        Span<float> span4 = v4.AsSpan();

        bool b1 = span1.SequenceEqual(span2);
        bool b2 = span1.SequenceEqual(span3);
        bool b3 = span1.SequenceEqual(span4);
    }

    [Benchmark(Baseline = true)]
    public float[,] GenerateNoise_2D()
    {
        float[,] grid = new float[_size, _size];
        List<float[,]> octaveLayers = [];

        // Generate the noise octave layers.
        for (int i = 0; i < _octaves; i++)
        {
            int steps = _density / (int)float.Pow(2, i);
            float[,] octave = DoWork2D(_size);
            octaveLayers.Add(octave);
        }

        // Accumulate the layers, with diminishing impact.
        for (int y = 0; y < _size; y++)
        {
            for (int x = 0; x < _size; x++)
            {
                grid[x, y] = octaveLayers.Select((o, i) => o[x, y] * (1 / float.Pow(2, i))).Sum();
            }
        }

        // Normalizes the grid to [0, 1], then apply the transform function if it has been defined.
        for (int y = 0; y < _size; y++)
        {
            for (int x = 0; x < _size; x++)
            {
                float value = grid[x, y];
                grid[x, y] = (value - _minGridValue) / (_maxGridValue - _minGridValue);
            }
        }

        return grid;
    }

    [Benchmark]
    public float[] GenerateNoise_1D()
    {
        float[] grid = new float[_size * _size];
        List<float[]> octaveLayers = [];

        // Generate the noise octave layers.
        for (int i = 0; i < _octaves; i++)
        {
            int steps = _density / (int)float.Pow(2, i);
            float[] octave = DoWork1D(_size);
            octaveLayers.Add(octave);
        }

        // Accumulate the layers, with diminishing impact.
        for (int y = 0; y < _size; y++)
        {
            for (int x = 0; x < _size; x++)
            {
                int i = (y * _size) + x;
                grid[i] = octaveLayers.Select((o, j) => o[i] * (1 / float.Pow(2, j))).Sum();
            }
        }

        // Normalizes the grid to [0, 1], then apply the transform function if it has been defined.
        for (int y = 0; y < _size; y++)
        {
            for (int x = 0; x < _size; x++)
            {
                int i = (y * _size) + x;
                float value = grid[i];
                grid[i] = (value - _minGridValue) / (_maxGridValue - _minGridValue);
            }
        }

        return grid;
    }

    [Benchmark]
    public float[] GenerateNoise_1D_Flattened()
    {
        float[] grid = new float[_size * _size];
        List<float[]> octaveLayers = [];

        // Generate the noise octave layers.
        for (int i = 0; i < _octaves; i++)
        {
            int steps = _density / (int)float.Pow(2, i);
            float[] octave = DoWork1D(_size);
            octaveLayers.Add(octave);
        }

        // Accumulate the layers, with diminishing impact.
        for (int i = 0; i < grid.Length; i++)
        {
            grid[i] = octaveLayers.Select((o, j) => o[i] * (1 / float.Pow(2, j))).Sum();
        }

        // Normalizes the grid to [0, 1], then apply the transform function if it has been defined.
        for (int i = 0; i < grid.Length; i++)
        {
            float value = grid[i];
            grid[i] = (value - _minGridValue) / (_maxGridValue - _minGridValue);
        }

        return grid;
    }

    [Benchmark]
    public float[] GenerateNoise_1D_Flattened2()
    {
        float[] grid = new float[_size * _size];
        float[] octaveLayers = new float[_size * _size * _octaves];

        // Generate the noise octave layers.
        for (int i = 0; i < _octaves; i++)
        {
            int steps = _density / (int)float.Pow(2, i);

            int octaveSliceLength = _size * _size;

            Span<float> octave = octaveLayers.AsSpan().Slice(octaveSliceLength * i, octaveSliceLength);
            DoWork1DInput(octave);

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
        }

        return grid;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static float[] DoWork1D(int size)
    {
        float[] data = new float[size * size];
#if DEBUG
        for (int i = 0; i < data.Length; i++)
        {
            data[i] = i;
        }
#else
        data[0] = 2F;
#endif
        return data;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void DoWork1DInput(Span<float> data)
    {
#if DEBUG
        for (int i = 0; i < data.Length; i++)
        {
            data[i] = i;
        }
#else
        data[0] = 2F;
#endif
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static float[,] DoWork2D(int size)
    {
        float[,] data = new float[size, size];
#if DEBUG
        Span<float> span = MemoryMarshal.CreateSpan(ref data[0, 0], data.Length);
        for (int i = 0; i < span.Length; i++)
        {
            span[i] = i;
        }
#else
        data[0, 0] = 2F;
#endif
        return data;
    }
}

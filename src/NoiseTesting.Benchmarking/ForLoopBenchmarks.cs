using BenchmarkDotNet.Attributes;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

/// <summary>
/// Compares approaches for iterating over a 2D array and performing a simple action.
/// </summary>
[MemoryDiagnoser]
public class ForLoopBenchmarks
{
    private const int _gridSize = 256;
    private readonly float[,] _grid;

    public ForLoopBenchmarks()
    {
        _grid = new float[_gridSize, _gridSize];
    }

    /// <summary>
    /// Two nested for-loops.
    /// </summary>
    [Benchmark(Baseline = true)]
    public void NestedForLoops()
    {
        for (int y = 0; y < _grid.GetLength(1); y++)
        {
            for (int x = 0; x < _grid.GetLength(0); x++)
            {
                _grid[x, y] = DoWork();
            }
        }
    }

    /// <summary>
    /// Single for-loop.
    /// </summary>
    [Benchmark]
    public void SingleForLoop()
    {
        for (int i = 0; i < _grid.Length; i++)
        {
            int x = i % _grid.GetLength(0);
            int y = i / _grid.GetLength(0);
            _grid[x, y] = DoWork();
        }
    }

    /// <summary>
    /// Single parallel for-loop.
    /// </summary>
    [Benchmark]
    public void SingleParallelForLoop()
    {
        Parallel.For(0, _grid.Length, i =>
        {
            int x = i % _grid.GetLength(0);
            int y = i / _grid.GetLength(0);
            _grid[x, y] = DoWork();
        });
    }

    /// <summary>
    /// Single span-based for-loop.
    /// </summary>
    [Benchmark]
    public void SingleSpanForLoop()
    {
        Span<float> span = MemoryMarshal.CreateSpan(ref _grid[0, 0], _grid.Length);

        for (int x = 0; x < span.Length; x++)
        {
            span[x] = DoWork();
        }
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static float DoWork()
    {
        return 2F;
    }
}

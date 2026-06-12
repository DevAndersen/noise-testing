using System.Numerics;
using System.Runtime.InteropServices;
using System.Security.Cryptography;

namespace NoiseTesting.Noise.Services;

public class PerlinNoiseGenerator3
{
    /// <summary>
    /// The size of each vector tile.
    /// Square root of 16, (the number of bytes of an MD5 digest), divided by 2 (bytes used to generate each).
    /// </summary>
    private static readonly int _tileSize = (int)float.Sqrt(MD5.HashSizeInBytes) / 2;

    /// <summary>
    /// Populates <paramref name="vectors"/> with pseudo-random 3D unit vectors, based on the elements of <paramref name="input"/>.
    /// </summary>
    /// <remarks>
    /// All three components of each vector are derived from two byte values.
    /// The populated vectors therefore have 65.536 possible permutations.
    /// </remarks>
    /// <param name="vectors">The vectors to be populated. Must contain between 1 and 8 elements.</param>
    /// <param name="workBuffer">The buffer used to store temporary data. Must have a length of at least 16 bytes.</param>
    /// <param name="input">The input data, used as the seed for the vector population. Must contain at least one value.</param>
    /// <returns></returns>
    public static void PopulateNoiseVectors(Span<Vector3> vectors, Span<byte> workBuffer, ReadOnlySpan<int> input)
    {
        // Generate pseudo-random binary data from the input arguments.
        MD5.HashData(MemoryMarshal.Cast<int, byte>(input), workBuffer);

        // Read numbers from the digest buffer, and use them to create unit vectors.
        for (int i = 0; i < vectors.Length; i += 2)
        {
            // Calculate the angles from the read numbers, ensuring even distribution, and avoiding perfectly straight angles.
            float angleA = workBuffer[i] * (float.Tau / (byte.MaxValue + 1)) + float.E;
            float angleB = workBuffer[i + 1] * (float.Tau / (byte.MaxValue + 1)) + float.E;

            // Populate the indexed vector data.
            float b1 = float.Sin(angleB);
            vectors[i] = new Vector3(
                float.Sin(angleA) * b1,
                float.Cos(angleA) * b1,
                float.Cos(angleB));
        }
    }

    private readonly record struct Point3(int X, int Y, int Z);
}

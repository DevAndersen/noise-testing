using BenchmarkDotNet.Running;
using NoiseTesting.Benchmarking;
using System.Runtime.InteropServices;

#if DEBUG

new NoiseBenchmarks().Debug();

#else

//BenchmarkRunner.Run<ForLoopBenchmarks>();
BenchmarkRunner.Run<NoiseBenchmarks>();

#endif

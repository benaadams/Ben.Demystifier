using System;
using System.Diagnostics;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Attributes.Jobs;

namespace Ben.Demystifier.Benchmarks
{
    [ClrJob, CoreJob]
    [Config(typeof(Config))]
    public class ExceptionTests
    {
        [Benchmark(Baseline = true, Description = ".ToString()")]
        public string Baseline() => new Exception().ToString();

        [Benchmark(Description = "Demystify().ToString()")]
        public string Demystify() => new Exception().Demystify().ToString();
    }
}

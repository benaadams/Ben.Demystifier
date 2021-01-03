using System;
using System.Collections.Generic;
using System.Diagnostics;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;

namespace Ben.Demystifier.Benchmarks
{
    [SimpleJob(RuntimeMoniker.Net48)]
    [SimpleJob(RuntimeMoniker.NetCoreApp21)]
    [SimpleJob(RuntimeMoniker.NetCoreApp31)]
    [SimpleJob(RuntimeMoniker.NetCoreApp50)]
    [Config(typeof(Config))]
    public class ExceptionTests
    {
        [Benchmark(Baseline = true, Description = ".ToString()")]
        public string Baseline() => new Exception().ToString();

        [Benchmark(Description = "Demystify().ToString()")]
        public string Demystify() => new Exception().Demystify().ToString();

        [Benchmark(Description = "(left, right).ToString()")]
        public string ToStringForTupleBased() => GetException(() => ReturnsTuple()).ToString();

        [Benchmark(Description = "(left, right).Demystify().ToString()")]
        public string ToDemystifyForTupleBased() => GetException(() => ReturnsTuple()).Demystify().ToString();

        private static Exception GetException(Action action)
        {
            try
            {
                action();
                throw new InvalidOperationException("Should not be reachable.");
            }
            catch (Exception e)
            {
                return e;
            }
        }

        private static List<(int left, int right)> ReturnsTuple() => throw new Exception();
    }
}

using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Running;
using System;
using System.Linq;
using System.Reflection;

namespace Ben.Demystifier.Benchmarks
{
    public static class Program
    {
        private const string BenchmarkSuffix = "Tests";

        public static void Main(string[] args)
        {
            var benchmarks = Assembly.GetEntryAssembly()
                .DefinedTypes.Where(t => t.Name.EndsWith(BenchmarkSuffix))
                .ToDictionary(t => t.Name.Substring(0, t.Name.Length - BenchmarkSuffix.Length), t => t, StringComparer.OrdinalIgnoreCase);

            if (args.Length > 0 && args[0].Equals("all", StringComparison.OrdinalIgnoreCase))
            {
                Console.WriteLine("Running full benchmarks suite");
                benchmarks.Select(pair => pair.Value).ToList().ForEach(action => BenchmarkRunner.Run(action));
                return;
            }

            if (args.Length == 0 || !benchmarks.ContainsKey(args[0]))
            {
                Console.WriteLine("Please, select benchmark, list of available:");
                benchmarks
                    .Select(pair => pair.Key)
                    .ToList()
                    .ForEach(Console.WriteLine);
                Console.WriteLine("All");
                return;
            }

            BenchmarkRunner.Run(benchmarks[args[0]]);

            Console.Read();
        }
    }

    internal class Config : ManualConfig
    {
        public Config() => AddDiagnoser(MemoryDiagnoser.Default);
    }
}

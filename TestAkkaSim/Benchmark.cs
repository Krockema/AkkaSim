using System.Linq;
using BenchmarkDotNet.Running;
using Xunit;
using Xunit.Abstractions;

namespace TestAkkaSim
{
    public class Benchmark
    {
        private ITestOutputHelper _out;
        public Benchmark(ITestOutputHelper output)
        {
            _out = output;
        }
        [Fact]
        public void BenchTimeComparer()
        {
            var summary = BenchmarkRunner.Run<TimeTest>();
            var te = summary.LogFilePath.Length;

        }
    }
}
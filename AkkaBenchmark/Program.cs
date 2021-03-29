using System;
using BenchmarkDotNet.Running;

namespace AkkaBenchmark
{
    class Program
    {
        static void Main(string[] args)
        {
            var summary = BenchmarkRunner.Run<TimeTest>();
        }
    }
}

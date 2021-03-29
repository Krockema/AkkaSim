using System;
using AkkaSim.Definitions;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Engines;
using Microsoft.Diagnostics.Runtime.Interop;

namespace AkkaBenchmark
{
    [SimpleJob(RunStrategy.ColdStart, targetCount: 10000)]
    [MemoryDiagnoser]
    [ThreadingDiagnoser]
    [RPlotExporter]
    public class TimeTest
    {
        private Random rnd = new(212);
        private TimeWrapper current;
        private TimeWrapper next;
        private TimeWrapper euality;
        private long currentLong;
        private long nextLong;
        private long equalLong;

        [IterationSetup]
        public void Setup()
        {


            currentLong = rnd.Next();
            nextLong = rnd.Next();
            equalLong = rnd.Next();
            current = new TimeWrapper(DateTime.FromFileTime(currentLong));
            next = new TimeWrapper(DateTime.FromFileTime(nextLong));
            euality = new TimeWrapper(DateTime.FromFileTime(equalLong));
        }

        [Benchmark]
        public bool FalseDateEquals() => current.Value.TrimMilliseconds().Equals(next.Value.TrimMilliseconds());

        [Benchmark]
        public bool FalseDateCompareWIth() => current.Value.CompareWith(next.Value);
        [Benchmark]
        public bool FalseDateAreEqual() => current.Value.AreEqual(next.Value, TimeSpan.FromSeconds(1));
        [Benchmark]
        public bool FalseLong() => currentLong.Equals(nextLong);

        [Benchmark]
        public bool TrueDateEquals() => euality.Value.TrimMilliseconds().Equals(euality.Value.TrimMilliseconds());

        [Benchmark]
        public bool TrueDateCompareWIth() => euality.Value.CompareWith(euality.Value);
        [Benchmark]
        public bool TrueDateAreEqual() => euality.Value.AreEqual(euality.Value, TimeSpan.FromSeconds(1));
        [Benchmark]
        public bool TrueLong() => equalLong.Equals(equalLong);
    }

    public class TimeWrapper
    {
        private DateTime _time;
        public TimeWrapper(DateTime time)
        {
            _time = time;
        }
        public DateTime Value => _time;
    }
}
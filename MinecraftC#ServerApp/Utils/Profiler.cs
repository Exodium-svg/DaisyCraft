using System.Collections.Concurrent;
using System.Diagnostics;

namespace Utils
{
    public static class Profiler
    {
        private static readonly ConcurrentDictionary<string, Stats> _stats = new();

        public static IDisposable Measure(string name) => new ProfileScope(name);

        public static IReadOnlyDictionary<string, Stats> GetStats() => _stats;

        private static void Record(string name, double ms)
        {
            var stat = _stats.GetOrAdd(name, _ => new Stats());
            lock (stat)
            {
                stat.Count++;
                stat.Total += ms;
            }
        }

        public class Stats
        {
            public long Count { get; set; }
            public double Total { get; set; }
            public double Average => Count == 0 ? 0 : Total / Count;
        }

        private sealed class ProfileScope : IDisposable
        {
            private readonly string _name;
            private readonly Stopwatch _sw = Stopwatch.StartNew();

            public ProfileScope(string name) => _name = name;

            public void Dispose()
            {
                _sw.Stop();
                Record(_name, _sw.Elapsed.TotalMilliseconds);
            }
        }
    }
}

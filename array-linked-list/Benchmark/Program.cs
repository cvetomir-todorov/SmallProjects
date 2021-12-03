using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Benchmark
{
    public static class Program
    {
        public static void Main()
        {
            const int count = ArrayLinkedList<string>.MaxCapacity;
            ArrayLinkedList<string> llArray = new(capacity: count);
            LinkedList<string> llClassic = new();

            // warm-up
            const int warmUpCount = 1024;
            TestAddRemoveRandom(llArray, warmUpCount, warmUpCount);
            TestAddRemoveRandom(llClassic, warmUpCount, warmUpCount);

            // actual
            TimeSpan llArrayElapsed = TestAddRemoveRandom(llArray, count, count);
            TimeSpan llClassicElapsed = TestAddRemoveRandom(llClassic, count, count);

            Console.WriteLine("Array   linked-list: {0:0.##} ms", llArrayElapsed.TotalMilliseconds);
            Console.WriteLine("Classic linked-list: {0:0.##} ms", llClassicElapsed.TotalMilliseconds);
        }

        private static TimeSpan TestAddRemoveRandom(ICollection<string> target, int addCount, int removeCount)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();

            for (int i = 0; i < addCount; ++i)
            {
                target.Add(i.ToString());
            }

            for (int i = 0; i < removeCount; ++i)
            {
                target.Remove(i.ToString());
            }

            GC.Collect(generation: GC.MaxGeneration, GCCollectionMode.Forced, blocking: true);

            stopwatch.Stop();
            return stopwatch.Elapsed;
        }
    }
}
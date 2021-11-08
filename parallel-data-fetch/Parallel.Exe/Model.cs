using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Parallel.Exe
{
    public sealed class Person
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public override string ToString() => $"ID:{Id} {Name}";
    }

    public interface IPersonRepo
    {
        Task<int> GetPageCount(int pageSize, CancellationToken ct);
        Task<IEnumerable<Person>> GetPersonPage(int page, int pageSize, CancellationToken ct);
    }

    public class FakePersonRepo : IPersonRepo
    {
        private readonly Random _random = new();

        public int ReturnedPageCount { get; set; }
        public IReadOnlyCollection<Person> ReturnedPeople { get; set; }

        public Task<int> GetPageCount(int pageSize, CancellationToken ct)
        {
            return Task.FromResult(ReturnedPageCount);
        }

        public async Task<IEnumerable<Person>> GetPersonPage(int page, int pageSize, CancellationToken ct)
        {
            int waitMillis = _random.Next(500, 5000);
            Console.WriteLine("Fetching page {0} and waiting for {1} ms...", page, waitMillis);
            await Task.Delay(TimeSpan.FromMilliseconds(waitMillis), ct);

            if (waitMillis % 3 == 0)
            {
                throw new Exception("Fake error.");
            }

            return ReturnedPeople;
        }
    }
}
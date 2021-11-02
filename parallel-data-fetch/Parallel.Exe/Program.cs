using System;
using System.Threading;
using System.Threading.Tasks;

namespace Parallel.Exe
{
    public static class Program
    {
        public static async Task Main(string[] args)
        {
            FakePersonRepo repo = new()
            {
                ReturnedPageCount = 10,
                ReturnedPeople = new[]
                {
                    new Person {Id = 0, Name = "Alice"},
                    new Person {Id = 1, Name = "Bob"},
                    new Person {Id = 2, Name = "Helen"},
                    new Person {Id = 3, Name = "Susan"},
                    new Person {Id = 4, Name = "Peter"}
                }
            };

            // page size is set to be = count of returned people from the fake repo
            IPersonFetcher fetcher = new ParallelPagedPersonFetcher(repo, pageSize: 5);
            int counter = 0;
            using CancellationTokenSource cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
            await foreach (Person person in fetcher.FetchPeople(cts.Token))
            {
                Console.WriteLine(person);
                counter++;
            }
            Console.WriteLine("Fetched {0} people.", counter);
        }
    }
}
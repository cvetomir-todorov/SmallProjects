using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace Parallel.Exe
{
    public interface IPersonFetcher
    {
        IAsyncEnumerable<Person> FetchPeople(CancellationToken ct);
    }

    public sealed class ParallelPagedPersonFetcher : IPersonFetcher
    {
        private readonly IPersonRepo _repo;
        private readonly int _pageSize;

        public ParallelPagedPersonFetcher(IPersonRepo repo, int pageSize)
        {
            _repo = repo;
            _pageSize = pageSize;
        }

        public async IAsyncEnumerable<Person> FetchPeople([EnumeratorCancellation] CancellationToken ct)
        {
            int pageCount = await _repo.GetPersonPageCount(_pageSize, ct);
            if (pageCount <= 0)
            {
                yield break;
            }

            // the channel is unbounded and will have:
            // - multiple producers - each thread fetching a given page
            // - a single consumer - this method, which streams the people back to the client
            Channel<IEnumerable<Person>> channel = Channel.CreateUnbounded<IEnumerable<Person>>(new UnboundedChannelOptions
            {
                SingleWriter = false,
                SingleReader = true
            });

            Task parallelTask = null;
            try
            {
                parallelTask = Task.Factory.StartNew(() =>
                {
                    FetchPagesInParallel(pageCount, channel, ct);
                }, TaskCreationOptions.LongRunning);

                await foreach (IEnumerable<Person> personPage in channel.Reader.ReadAllAsync(ct))
                {
                    foreach (Person person in personPage)
                    {
                        yield return person;
                    }
                }
            }
            finally
            {
                parallelTask?.Wait(ct);
                parallelTask?.Dispose();
            }
        }

        private void FetchPagesInParallel(int pageCount, Channel<IEnumerable<Person>> channel, CancellationToken ct)
        {
            Task[] tasks = new Task[pageCount];
            for (int page = 0; page < pageCount; ++page)
            {
                int localPage = page;
                tasks[page] = _repo.GetPersonPage(page, _pageSize, ct)
                    .ContinueWith(task =>
                    {
                        if (task.IsCompletedSuccessfully)
                        {
                            Console.WriteLine("Page {0} fetched successfully.", localPage);
                            channel.Writer.TryWrite(task.Result);
                        }
                        else
                        {
                            Console.WriteLine("Fetching page {0} failed: {1}.", localPage, task.Exception);
                        }
                    }, ct);
            }
            Task.WaitAll(tasks, ct);

            // complete the channel so that the reader stops after reading all data
            channel.Writer.Complete();
        }
    }
}
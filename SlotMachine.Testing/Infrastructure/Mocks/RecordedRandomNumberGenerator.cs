using System;
using System.Collections.Generic;
using SlotMachine.App.Game.Spinning;

namespace SlotMachine.Testing.Infrastructure.Mocks
{
    public sealed class RecordedRandomNumberGenerator : IRandomNumberGenerator
    {
        private readonly Queue<int> _records;

        public RecordedRandomNumberGenerator()
        {
            _records = new Queue<int>();
        }

        public void Add(int randomNumber)
        {
            _records.Enqueue(randomNumber);
        }

        public void AddMany(IEnumerable<int> randomNumbers)
        {
            foreach (int randomNumber in randomNumbers)
            {
                _records.Enqueue(randomNumber);
            }
        }

        public int Generate(int max)
        {
            if (!_records.TryDequeue(out int record))
            {
                throw new InvalidOperationException("Out of recorded random numbers.");
            }
            else
            {
                return record;
            }
        }
    }
}

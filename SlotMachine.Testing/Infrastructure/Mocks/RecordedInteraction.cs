using System;
using System.Collections.Generic;
using SlotMachine.App.Game.UI;

namespace SlotMachine.Testing.Infrastructure.Mocks
{
    public sealed class RecordedInteraction : IInteraction
    {
        private readonly Queue<Result<char>> _charRecords;
        private readonly Queue<Result<decimal>> _decimalRecords;

        public RecordedInteraction()
        {
            _charRecords = new Queue<Result<char>>();
            _decimalRecords = new Queue<Result<decimal>>();
        }

        public RecordedInteraction AddCharResult(char c)
        {
            _charRecords.Enqueue(new Result<char>(isSuccess: true, c));
            return this;
        }

        public RecordedInteraction AddFailedCharResult()
        {
            _charRecords.Enqueue(new Result<char>());
            return this;
        }

        public RecordedInteraction AddDecimalResult(decimal d)
        {
            _decimalRecords.Enqueue(new Result<decimal>(isSuccess: true, d));
            return this;
        }

        public RecordedInteraction AddFailedDecimalResult()
        {
            _decimalRecords.Enqueue(new Result<decimal>());
            return this;
        }

        public Result<char> TryGetChar()
        {
            if (!_charRecords.TryDequeue(out var result))
            {
                throw new InvalidOperationException("Out of recorded char results.");
            }
            else
            {
                return result;
            }
        }

        public Result<decimal> TryGetDecimal()
        {
            if (!_decimalRecords.TryDequeue(out var result))
            {
                throw new InvalidOperationException("Out of recorded decimal results.");
            }
            else
            {
                return result;
            }
        }
    }
}

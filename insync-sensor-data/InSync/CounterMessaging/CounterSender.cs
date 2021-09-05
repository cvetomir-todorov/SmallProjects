using System;
using System.IO.Pipes;
using System.Threading;
using System.Threading.Tasks;

namespace InSync.CounterMessaging
{
    public interface ICounterSender : IDisposable
    {
        bool IsConnected { get; }

        Task<bool> GetStatus(CancellationToken ct);

        Task SetStatus(bool status, CancellationToken ct);
    }

    public class CounterSender : CounterBase, ICounterSender
    {
        public CounterSender(PipeStream pipeStream) : base(pipeStream)
        {}

        public bool IsConnected => PipeStream.IsConnected;

        public async Task<bool> GetStatus(CancellationToken ct)
        {
            Buffer[0] = CounterCommands.GetStatus;
            await PipeStream.WriteAsync(Buffer, offset: 0, count: 1, ct).ConfigureAwait(false);
            await PipeStream.FlushAsync(ct).ConfigureAwait(false);

            await PipeStream.ReadAsync(Buffer, offset: 0, count: 2, ct).ConfigureAwait(false);
            if (Buffer[0] != CounterCommands.GetStatus)
            {
                throw new InvalidOperationException($"Expected command {CounterCommands.GetStatus} but received {Buffer[0]}.");
            }

            bool status = Buffer[1] > 0;
            return status;
        }

        public async Task SetStatus(bool status, CancellationToken ct)
        {
            Buffer[0] = CounterCommands.SetStatus;
            Buffer[1] = status ? (byte)1 : (byte)0;

            await PipeStream.WriteAsync(Buffer, offset: 0, count: 2, ct).ConfigureAwait(false);
            await PipeStream.FlushAsync(ct).ConfigureAwait(false);
        }
    }
}

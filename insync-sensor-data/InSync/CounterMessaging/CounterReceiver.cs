using System;
using System.IO.Pipes;
using System.Threading;
using System.Threading.Tasks;

namespace InSync.CounterMessaging
{
    public interface ICounterReceiver : IDisposable
    {
        Task Listen(
            Func<bool> getStatusCallback,
            Action<bool> setStatusCallback,
            CancellationToken ct);
    }

    public class CounterReceiver : CounterBase, ICounterReceiver
    {
        public CounterReceiver(PipeStream pipeStream) : base(pipeStream)
        {}

        public async Task Listen(
            Func<bool> getStatusCallback,
            Action<bool> setStatusCallback,
            CancellationToken ct)
        {
            while (!ct.IsCancellationRequested && PipeStream.IsConnected)
            {
                (bool success, byte command) = await TryReadCommand(ct).ConfigureAwait(false);
                if (!success)
                    break;

                if (command == CounterCommands.GetStatus)
                {
                    if (false == await TryHandleGetStatus(getStatusCallback, ct).ConfigureAwait(false))
                        break;
                }
                else if (command == CounterCommands.SetStatus)
                {
                    if (false == await TryHandleSetStatus(setStatusCallback, ct).ConfigureAwait(false))
                        break;
                }
                else
                {
                    if (ct.IsCancellationRequested)
                        break;

                    throw new InvalidOperationException($"Unknown command {command}.");
                }
            }
        }

        private async Task<(bool, byte)> TryReadCommand(CancellationToken ct)
        {
            int bytesRead = await PipeStream.ReadAsync(Buffer, offset: 0, count: 1, ct).ConfigureAwait(false);
            if (bytesRead <= 0) // disconnected
            {
                return (false, 0);
            }

            byte command = Buffer[0];
            return (true, command);
        }

        private async Task<bool> TryHandleGetStatus(Func<bool> getStatusCallback, CancellationToken ct)
        {
            if (ct.IsCancellationRequested)
                return false;

            bool status = getStatusCallback();
            Buffer[1] = status ? (byte) 1 : (byte) 0;
            await PipeStream.WriteAsync(Buffer, offset: 0, count: 2, ct).ConfigureAwait(false);
            return true;
        }

        private async Task<bool> TryHandleSetStatus(Action<bool> setStatusCallback, CancellationToken ct)
        {
            int bytesRead = await PipeStream.ReadAsync(Buffer, offset: 1, count: 1, ct).ConfigureAwait(false);
            if (bytesRead <= 0) // disconnected
            {
                return false;
            }

            bool status = Buffer[1] > 0;

            if (ct.IsCancellationRequested)
                return false;

            setStatusCallback(status);
            return true;
        }
    }
}

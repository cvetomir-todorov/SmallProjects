using System;
using System.IO.Pipes;

namespace InSync.CounterMessaging
{
    public abstract class CounterBase : IDisposable
    {
        protected CounterBase(PipeStream pipeStream)
        {
            PipeStream = pipeStream;
            Buffer = new byte[2];
        }

        protected PipeStream PipeStream { get; private set; }
        protected byte[] Buffer { get; private set; }

        public void Dispose()
        {
            CleanUp(isDisposing: true);
            GC.SuppressFinalize(this);
        }

        protected virtual void CleanUp(bool isDisposing)
        {
            if (isDisposing)
            {
                PipeStream.Dispose();
            }
        }

        protected static class CounterCommands
        {
            public const byte GetStatus = 1;
            public const byte SetStatus = 2;
        }
    }
}

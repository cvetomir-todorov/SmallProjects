using System;

namespace InSync.Api.Counter
{
    public sealed class CounterOptions
    {
        public string PipeName { get; set; }

        public TimeSpan PipeReconnectInterval { get; set; }

        public TimeSpan PipeConnectTimeout { get; set; }
    }
}

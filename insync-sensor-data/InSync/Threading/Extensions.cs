using System;
using System.Threading;

namespace InSync.Threading
{
    public static class Extensions
    {
        /// <summary>
        /// Safely cancels the <see cref="CancellationTokenSource"/> even if it has been previously disposed.
        /// </summary>
        public static void SafelyCancel(this CancellationTokenSource cts)
        {
            try
            {
                cts.Cancel();
            }
            catch (ObjectDisposedException)
            {
                // already disposed, simply ignore
            }
        }
    }
}

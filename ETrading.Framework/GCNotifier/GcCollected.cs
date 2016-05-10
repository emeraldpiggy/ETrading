using System;
using System.Threading;

namespace ETrading.Framework.GCNotifier
{
    internal class GcCollected
    {
        private static int _count;

        private GcCollected()
        {
            Interlocked.Increment(ref _count);
        }

        public static int Count { get { return _count; } }

        ~GcCollected()
        {
            if (Environment.HasShutdownStarted)
                return;

            if (AppDomain.CurrentDomain.IsFinalizingForUnload())
                return;

            Interlocked.Decrement(ref _count);
            GCNotifier.GCCollected(_count);
        }

        internal static GcCollected Create()
        {
            var cnt = Interlocked.CompareExchange(ref _count, 0, 0);
            if (cnt == 0)
            {
                return new GcCollected();
            }
            return null;
        }
    }
}
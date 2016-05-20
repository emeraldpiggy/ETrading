using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ETrading.Framework.GCNotifier
{
    public delegate void GarbageCollectionHandler();


    public static class GCNotifier
    {
#pragma warning disable 649
        private static readonly GCNotifierThread _notifierThread;
#pragma warning restore 649

        /// <summary>
        /// OS Thread ID when the thread was created.
        /// </summary>
        public static int? ThreadID
        {
            get
            {
                if (_notifierThread != null)
                {
                    return _notifierThread.ThreadID;
                }
                return 0;
            }
        }

        /// <summary>
        /// Notify notifier thread GC has occured
        /// </summary>
        /// <param name="count"></param>
        internal static void GCCollected(int count)
        {
            if (_notifierThread != null)
            {
                _notifierThread.GCCollected();
            }
        }

        static GCNotifier()
        {
            bool enable = true;
            if (enable)
            {
                _notifierThread = new GCNotifierThread(TimeSpan.FromSeconds(5)) { GCThrottle = TimeSpan.FromSeconds(10) };
            }
        }

        public static void Register(IGCNotifierRegistration registration)
        {
            if (_notifierThread != null)
            {
                _notifierThread.AddHandler(registration);
            }
        }

        public static void UnRegister(IGCNotifierRegistration registration)
        {
            if (_notifierThread != null)
            {
                _notifierThread.RemoveHandler(registration);
            }
        }


        public static void Collect(bool block = false)
        {
            if (_notifierThread != null)
            {
                _notifierThread.Collect(block);
            }
        }
    }

}

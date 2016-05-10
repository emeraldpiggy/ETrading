using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ETrading.Framework.GCNotifier
{
    internal class GCNotifierThread
    {
        private readonly bool _disableGCForTesting;

        #region Private Declarations
        [DllImport("kernel32.dll")]
        private static extern uint GetCurrentThreadId();



        private readonly ConcurrentQueue<GCNotifierMessage> _handlerQueue = new ConcurrentQueue<GCNotifierMessage>();

        private readonly Queue<IGCNotifierRegistration> _removeList = new Queue<IGCNotifierRegistration>();
        private readonly GCNotifierBuffer _delegates = new GCNotifierBuffer(1000000, 500000);
        private readonly Thread _thread;
        private readonly ManualResetEvent _handlerQueueWait = new ManualResetEvent(false);
        private bool _loop;
        private TimeSpan? _collectionWaitTime;
        private int? _threadID;
        private bool _gcCollected;
        private readonly Stopwatch _lastCollection = new Stopwatch();
        private bool _hasChanges;
        private TimeSpan _gcThrottle = TimeSpan.FromSeconds(1);
        #endregion

        #region Properties
        /// <summary>
        /// Count of currently registered delegates
        /// </summary>
        internal int Count
        {
            get
            {
                return _delegates.Count;
            }
        }

        /// <summary>
        /// The capacity of the Handler Buffer
        /// </summary>
        internal int Capacity
        {
            get
            {
                return _delegates.Capacity;
            }
        }

        /// <summary>
        /// The OS thread ID
        /// </summary>
        public int? ThreadID
        {
            get
            {
                return _threadID;
            }
        }

        /// <summary>
        /// Ignore GC requires that are received at a rate faster than the throttle value
        /// </summary>
        public TimeSpan GCThrottle
        {
            get { return _gcThrottle; }
            set { _gcThrottle = value; }
        }

        #endregion

        #region Constructors
        /// <summary>
        /// 
        /// </summary>
        /// <param name="collectionTime">Set the collection timeout value.  If null then GC notification is used</param>
        public GCNotifierThread(TimeSpan? collectionTime = null)
            : this(false, collectionTime)
        {


        }

        internal GCNotifierThread(bool disableGCForTesting, TimeSpan? collectionTime = null)
        {
            _disableGCForTesting = disableGCForTesting;
            _collectionWaitTime = collectionTime;
            _thread = new Thread(ThreadProc);
            _thread.Priority = ThreadPriority.BelowNormal;
            _thread.IsBackground = true;
            _thread.Name = "GCNotifierThread";
            _thread.Start();
            _lastCollection.Start();
            _loop = true;
            if (!disableGCForTesting)
            {
                Internal.GCCollected.Create();
            }
        }

        #endregion

        #region Private Methods
        private void ThreadProc()
        {
            bool waitTriggered = false;
            _threadID = (int)GetCurrentThreadId();

            var stopWatch = new Stopwatch();
            if (_collectionWaitTime.HasValue)
            {
                stopWatch.Start();
            }

            while (_loop)
            {
                _handlerQueueWait.Reset();

                if (waitTriggered)
                {
                    ProcessHandlerQueue();
                }

                if (_collectionWaitTime.HasValue && stopWatch.ElapsedMilliseconds > _collectionWaitTime.Value.TotalMilliseconds)
                {
                    stopWatch.Reset();

                    _handlerQueue.Enqueue(new GCNotifierMessage(NotifierType.Timed, null));

                    stopWatch.Start();
                    waitTriggered = true;
                    continue;
                }

                waitTriggered = _handlerQueueWait.WaitOne(200);
            }
        }

        internal void Stop()
        {
            _loop = false;
        }

        private void ProcessHandlerQueue()
        {
            //block reentrant calls
            bool collect = false;
            bool timed = false;
            bool requiresGCMessage = false;
            List<Action> pumps = null;

            GCNotifierMessage message;

            var messages = new List<GCNotifierMessage>(20);
            while (_handlerQueue.TryDequeue(out message))
            {
                messages.Add(message);
            }
            var len = messages.Count;
            if (len > 0)
            {
                for (var i = 0; i < len; i++)
                {
                    var msg = messages[i];
                    switch (msg.Type)
                    {
                        case NotifierType.Add:
                            {
                                msg.Action();
                                break;
                            }
                        case NotifierType.Remove:
                            {
                                _hasChanges = true;
                                msg.Action();
                                break;
                            }
                        case NotifierType.Pump:
                            {
                                if (pumps == null)
                                {
                                    pumps = new List<Action>();
                                }
                                pumps.Add(msg.Action);
                                break;
                            }
                        case NotifierType.Collect:
                            {
                                collect = true;
                                break;
                            }
                        case NotifierType.GC:
                            {
                                _gcCollected = true;
                                break;
                            }

                        case NotifierType.Timed:
                            {
                                timed = true;
                                break;
                            }
                    }
                }
            }


            if (_gcCollected && _lastCollection.ElapsedMilliseconds < _gcThrottle.TotalMilliseconds && pumps == null)
            {
                requiresGCMessage = true;
            }
            else
            {
                if (_gcCollected || collect)
                {
                    CheckReferences(collect, _gcCollected);
                    _hasChanges = false;
                    if (_gcCollected)
                    {
                        _gcCollected = false;
                    }
                }
                else if ((timed && _hasChanges))
                {
                    _hasChanges = false;
                    CheckReferences(false);
                }
            }

            if (pumps != null)
            {
                pumps.ForEach(f => f());
            }

            if (requiresGCMessage)
            {
                _handlerQueue.Enqueue(new GCNotifierMessage(NotifierType.GC, null));
            }
        }

        private void ProcessAdd(IGCNotifierRegistration handler)
        {
            _delegates.Add(handler);
        }

        private void CheckReferences(bool collect, bool gc = false)
        {
            var removeList = _removeList.ToArray();
            _removeList.Clear();

            _delegates.Process(collect || gc, removeList);

            if (collect || gc)
            {
                _lastCollection.Restart();
            }

            if (!_disableGCForTesting && gc)
            {
                Internal.GCCollected.Create();
            }
        }

        //Add GC Collected message
        internal void GCCollected()
        {
            AddActionHandler(new GCNotifierMessage(NotifierType.GC, () => { }));
        }
        #endregion


        #region Public Methods

        /// <summary>
        /// Add a GarbageCollectionHandler to the queue
        /// </summary>
        /// <param name="handler"></param>
        public void AddHandler(IGCNotifierRegistration handler)
        {
            AddActionHandler(new GCNotifierMessage(NotifierType.Add, () => ProcessAdd(handler)));
        }


        internal void AddActionHandler(GCNotifierMessage message)
        {
            _handlerQueue.Enqueue(message);
            _handlerQueueWait.Set();
        }

        /// <summary>
        /// Remove a registerd GarbageCollectionHandler
        /// </summary>
        /// <param name="handler"></param>
        public void RemoveHandler(IGCNotifierRegistration handler)
        {
            AddActionHandler(new GCNotifierMessage(NotifierType.Remove, () => _removeList.Enqueue(handler)));
        }

        /// <summary>
        /// Process pending messages and block the calling thread until completed
        /// </summary>
        public void Pump()
        {
            using (var collectionWait = new ManualResetEvent(false))
            {
                // ReSharper disable AccessToDisposedClosure
                Task.Factory.StartNew(() => AddActionHandler(new GCNotifierMessage(NotifierType.Pump, () => collectionWait.Set())))
                    .ContinueWith(t => { }).Wait();

                collectionWait.WaitOne(TimeSpan.FromMinutes(1));
                // ReSharper restore AccessToDisposedClosure
            }
        }

        /// <summary>
        /// Force collection and block the calling thread until complete
        /// </summary>
        public void Collect()
        {
            Collect(true);
        }

        /// <summary>
        /// Send a collection notification to process the registered delegates.
        /// </summary>
        /// <param name="block">Block current thread until collection is complete</param>
        public void Collect(bool block)
        {
            if (block)
            {
                AddActionHandler(new GCNotifierMessage(NotifierType.Collect, () => { }));
                Pump();
            }
            else
            {
                AddActionHandler(new GCNotifierMessage(NotifierType.Timed, () => { }));
            }
        }
        #endregion



    }
}
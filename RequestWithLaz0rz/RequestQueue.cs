using System;
using System.Collections.Generic;
using C5;

namespace RequestWithLaz0rz
{
    public class RequestQueue
    {
        private static readonly Lazy<RequestQueue> Lazy = new Lazy<RequestQueue>(() => new RequestQueue());
        private readonly IntervalHeap<IRequest> _queue = new IntervalHeap<IRequest>(QueueCapacity, new PriorityComparer());
        private int _threadCount;

        private const int QueueCapacity = 256;
        
        /// <summary>
        /// Maximum number of threads running
        /// parallel.
        /// </summary>
        private const int MaxThreads = 4;

        /// <summary>
        /// Gets the singleton instance of
        /// this queue.
        /// </summary>
        public static RequestQueue Instance
        {
            get { return Lazy.Value; }
        }

        /// <summary>
        /// Hidden constructor
        /// </summary>
        private RequestQueue() { }

        #region event handler

        public event Action Started;

        protected virtual void OnStarted()
        {
            var handler = Started;
            if (handler != null) handler();
        }

        public event Action Completed;

        protected virtual void OnCompleted()
        {
            var handler = Completed;
            if (handler != null) handler();
        }

        #endregion

        /// <summary>
        /// Adds a new request to the queue. In addition
        /// it starts to dequeue the queue when min one 
        /// request is added.
        /// </summary>
        /// <param name="handle">The request handle of the added request</param>
        /// <param name="request">The request to add</param>
        protected internal void Enqueue(ref IPriorityQueueHandle<IRequest> handle, IRequest request)
        {
            lock (_queue)
            {
                //invoke started event
                if (_queue.IsEmpty) OnStarted();

                _queue.Add(ref handle, request);
                TryDequeueNext();
            }
        }

        /// <summary>
        /// Removes the request with the highest 
        /// priority from queue and executes it.
        /// </summary>
        /// <returns>Returns whether the queue is empty</returns>
        private bool TryDequeue()
        {
            lock (_queue)
            {
                //queue is completely dequeued
                if (_queue.IsEmpty) return true;

                //max number of running thrads reached
                if (_threadCount.Equals(MaxThreads)) return false;

                ++_threadCount; //TODO create thread-safe method to increment

                //get request with the highest priority
                var request = _queue.DeleteMax();

                request.Run(() =>
                {
                    --_threadCount; //TODO create thread-safe method to decrement
                    TryDequeueNext();
                });

                return false;
            }
        }

        /// <summary>
        /// Tries to dequeue the next request. Whenever 
        /// the queue is empty it invokes the onCompleted
        /// event.
        /// </summary>
        private void TryDequeueNext()
        {
            lock (_queue)
            {
                //invoke completed event
                if (TryDequeue()) OnCompleted();
            }
        }
    }

    /// <summary>
    /// Comparer which is used to select the request
    /// with the highest priority in queue.
    /// </summary>
    internal struct PriorityComparer : IComparer<IRequest>
    {
        public int Compare(IRequest x, IRequest y)
        {
            return x.CompareTo(y);
        }
    }
}

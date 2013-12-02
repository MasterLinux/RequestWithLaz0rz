using System;
using System.Collections.Generic;
using C5;

namespace RequestWithLaz0rz
{
    public class RequestQueue
    {
        private static readonly Lazy<RequestQueue> Lazy = new Lazy<RequestQueue>(() => new RequestQueue());
        private readonly IntervalHeap<IRequest> _queue = new IntervalHeap<IRequest>(new PriorityComparer());

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
                _queue.Add(ref handle, request); 
                Dequeue();
            }
        }

        /// <summary>
        /// Removes the next request with the highest 
        /// priority from queue and executes this.
        /// </summary>
        private void Dequeue()
        {
            lock (_queue)
            {
                if (_queue.IsEmpty) return;
                var request = _queue.DeleteMin();

                request.Run(); //TODO wait for events
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

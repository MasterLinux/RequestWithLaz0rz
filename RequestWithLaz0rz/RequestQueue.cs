using System;
using System.Collections.Generic;
using System.Threading;
using C5;

namespace RequestWithLaz0rz
{
    /// <summary>
    /// Request queue which handles the execution of all started requests. It allows
    /// to cancel a specific or all requests currently running. In addition it provides
    /// events to listen for lifecycle events, like the completed event.
    /// </summary>
    /// <example>This queue uses the singleton pattern.</example>
    /// <code>
    /// //get the queue
    /// var queue = RequestQueue.Instance;
    /// 
    /// //register lifecycle events
    /// queue.Started += () => { /* show loading indicator */ }
    /// queue.Completed += () => { /* hide loading indicator */ }
    /// 
    /// //create a request and execute to add it to queue,
    /// //because the request enqueues itself when executing
    /// var request = new ExampleRequest();
    /// request.Execute();
    /// 
    /// //cancel a specific request
    /// queue.Cancel(request);
    /// 
    /// //cancel all requests
    /// queue.Cancel();
    /// 
    /// </code>
    public class RequestQueue
    {
        private static readonly Lazy<RequestQueue> Lazy = new Lazy<RequestQueue>(() => new RequestQueue());
        private readonly IntervalHeap<IRequest> _queue = new IntervalHeap<IRequest>(QueueCapacity, new PriorityComparer());
        private int _threadCount;

        /// <summary>
        /// Maximum number of requests in queue.
        /// </summary>
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

        /// <summary>
        /// Event which is invoked whenever min
        /// one request is started.
        /// </summary>
        public event Action Started; //TODO possible that an event is added more than once, find solution

        /// <summary>
        /// Invokes the started event
        /// </summary>
        private void OnStarted()
        {
            var handler = Started;
            if (handler != null) handler();
        }

        /// <summary>
        /// Event which is invoked whenever
        /// all request are excuted.
        /// </summary>
        public event Action Completed;

        /// <summary>
        /// Invokes the completed event
        /// </summary>
        private void OnCompleted()
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
                DequeueNext();
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

                Interlocked.Increment(ref _threadCount);

                //get request with the highest priority
                var request = _queue.DeleteMax();

                request.RunAsync(() =>
                {
                    //on request completed
                    Interlocked.Decrement(ref _threadCount);
                    DequeueNext();
                });

                return false;
            }
        }

        /// <summary>
        /// Tries to dequeue the next request. Whenever 
        /// the queue is empty it invokes the completed
        /// event.
        /// </summary>
        private void DequeueNext()
        {
            lock (_queue)
            {
                //invoke completed event
                if (TryDequeue()) OnCompleted();
            }
        }

        /// <summary>
        /// Cancels a specific request and removes
        /// it from queue or cancels all requests
        /// whenever no specific request is passed.
        /// </summary>
        /// <param name="request">The request to cancel or null to cancel all</param>
        public void Cancel(IRequest request = null)
        {
            throw new NotImplementedException();
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

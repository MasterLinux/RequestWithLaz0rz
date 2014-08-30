using System;
using System.Collections.Concurrent;
using System.Threading;

namespace RequestWithLaz0rz.Data
{
    /// <summary>
    /// Request queue which handles the execution of all started requests. It allows
    /// to cancel a specific or all requests currently running. In addition it provides
    /// events to listen for lifecycle events, like the completed event.
    /// </summary>
    /// <example>This queue uses the singleton pattern.</example>
    /// <code>
    /// //get the queue
    /// var queue = new GetRequestQueue();
    /// 
    /// //register lifecycle events
    /// queue.Started += () => { /* show loading indicator */ }
    /// queue.Completed += () => { /* hide loading indicator */ }
    /// 
    /// //create a request and execute to add it to queue,
    /// //because the request enqueues itself when executing
    /// var request = new ExampleRequest();
    /// request.GetResponseAsync();
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
        private static readonly ConcurrentDictionary<string, RequestQueue> Instances = new ConcurrentDictionary<string, RequestQueue>();
        private readonly PriorityQueue<IPriorityRequest> _queue = new PriorityQueue<IPriorityRequest>();
        private int _threadCount;

        /// <summary>
        /// Maximum number of requests in queue.
        /// </summary>
        private const int QueueCapacity = 256;

        /// <summary>
        /// Maximum number of threads running parallel.
        /// </summary>
        public const int MaxThreads = 4;

        /// <summary>
        /// The key of the default request queue instance
        /// </summary>
        public const string DefaultQueueKey = "$_DefaultRequestQueue_$";

        /// <summary>
        /// Initializes the request queue
        /// </summary>
        /// <param name="id">Unique identifier which identifies the queue</param>
        private RequestQueue(string id)
        {
            Id = id;
        }

        /// <summary>
        /// Gets a specific request queue by its name
        /// </summary>
        /// <param name="key">The key of the queue to get</param>
        /// <returns></returns>
        public static RequestQueue GetRequestQueue(string key = DefaultQueueKey)
        {
            return Instances.GetOrAdd(key, id => new RequestQueue(id));
        }

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
        /// Unique ID of this queue
        /// </summary>
        public string Id { get; private set; }

        /// <summary>
        /// Checks whether the queue is empty
        /// </summary>
        public bool IsEmpty
        {
            get { return _queue.IsEmpty && _threadCount == 0; }
        }

        /// <summary>
        /// Checks whether the queue is not empty
        /// </summary>
        public bool IsNotEmpty
        {
            get { return !IsEmpty; }
        }

        /// <summary>
        /// Enqueues a new request into the queue
        /// </summary>
        /// <param name="request">The request to enqueue</param>
        public void Enqueue(IPriorityRequest request)
        {
            lock (_queue)
            {
                //invoke started event
                if (_queue.IsEmpty) OnStarted();

                _queue.Insert(request);
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

                request.RunAsync().ContinueWith(task =>
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
        public void Cancel(IPriorityRequest request = null)
        {
            throw new NotImplementedException();
        }
    }
}
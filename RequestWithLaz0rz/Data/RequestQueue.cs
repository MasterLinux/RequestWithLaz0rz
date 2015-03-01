using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace RequestWithLaz0rz.Data
{
    public delegate void QueueStartedHandler(RequestQueue sender);
    public delegate void QueueCompletedHandler(RequestQueue sender);

    public delegate void RequestQueueTaskHandler(RequestQueueTask sender);

    public class RequestQueueTask
    {
        private readonly RequestQueue _queue;
        private readonly IRequest<dynamic>[] _requests;
        private RequestQueueTaskHandler _onCompletedHandler;
        private bool _isCompleted;

        public RequestQueueTask(RequestQueue queue, params IRequest<dynamic>[] requests)
        {
            _queue = queue;
            _requests = requests;
        }

        public void Then(RequestQueueTaskHandler handler)
        {
            _onCompletedHandler = handler;

            if (_isCompleted)
            {
                NotifyCompletion();
            }
        }

        public void NotifyCompletion()
        {
            _isCompleted = true;

            if (_onCompletedHandler != null)
            {
                _onCompletedHandler(this);
            }
        }
    }

    /// <summary>
    /// Thread-safe priority queue for requests
    /// </summary>
    /// <example>Shows the usage of this queue</example>
    /// <code>
    /// //create a new queue. 3 threads are granted to run concurrently
    /// var queue = RequestQueue(3);
    /// 
    /// //register lifecycle events
    /// queue.Started += () => { /* show loading indicator */ }
    /// queue.Completed += () => { /* hide loading indicator */ }
    /// 
    /// //create a request 
    /// var request = new ExampleRequest();
    /// var anotherRequest = new ExampleRequest();
    /// 
    /// //enqueue and start request
    /// queue.Enqueue(request);
    /// queue.Enqueue(anotherRequest).Then((sender, args) => {
    ///     //optional onCompleted delegate
    ///     //which is invoked whenever 
    ///     //"anotherRequest" is completed
    /// });
    /// 
    /// //or enqueue multiple requests
    /// queue.Enqueue(request, anotherRequest).Then((sender, args) => {
    ///     //optional onCompleted delegate
    ///     //which is invoked whenever all 
    ///     //enqueued requests are completed
    /// });
    /// 
    /// //cancel a specific request
    /// await queue.AbortAsync(request);
    /// 
    /// //cancel all requests
    /// await queue.AbortAllAsync();
    /// 
    /// </code>
    public class RequestQueue
    {
        private readonly PriorityQueue<IRequest<dynamic>> _queue = new PriorityQueue<IRequest<dynamic>>();
        private readonly List<IRequest<dynamic>> _concurrentRequests = new List<IRequest<dynamic>>();
        private readonly ReaderWriterLockSlim _lock = new ReaderWriterLockSlim();        
        private readonly SemaphoreSlim _maxThreadsSemaphoreSlim;
        private readonly object _lockObject = new object();

        /// <summary>
        /// Maximum number of requests that can be granted concurrently
        /// </summary>
        public const int DefaultMaxThreadCount = 4;

        public event QueueStartedHandler Started;

        protected virtual void OnStarted()
        {
            var handler = Started;
            if (handler != null) handler(this);
        }

        public event QueueCompletedHandler Completed;

        protected virtual void OnCompleted()
        {
            var handler = Completed;
            if (handler != null) handler(this);
        }

        /// <summary>
        /// Initializes the request queue
        /// </summary>
        /// <param name="maxThreadCount">The maximum number of requests that can be granted concurrently</param>
        public RequestQueue(int maxThreadCount = DefaultMaxThreadCount)
        {
            _maxThreadsSemaphoreSlim = new SemaphoreSlim(maxThreadCount, maxThreadCount);
            MaxThreadCount = maxThreadCount;
        }

        /// <summary>
        /// Gets the number of requests actually 
        /// containted in this queue
        /// </summary>
        public int Count
        {
            get
            {
                _lock.EnterReadLock();
                var count = _queue.Count + _concurrentRequests.Count;
                _lock.ExitReadLock();

                return count;
            }
        }

        /// <summary>
        /// Checks whether the queue is empty
        /// </summary>
        public bool IsEmpty
        {
            get { return Count == 0; }
        }

        /// <summary>
        /// Checks whether the queue is not empty
        /// </summary>
        public bool IsNotEmpty
        {
            get { return Count > 0; }
        }

        /// <summary>
        /// Gets the maximum number of requests 
        /// that can be granted concurrently
        /// </summary>
        public int MaxThreadCount { get; private set; }

        /// <summary>
        /// Gets the current number of concurrently 
        /// executing requests
        /// </summary>
        public int CurrentThreadCount 
        {
            get
            {
                return MaxThreadCount - _maxThreadsSemaphoreSlim.CurrentCount;
            }
        }

        /// <summary>
        /// Enqueues a request and starts it 
        /// </summary>
        /// <param name="requests">All requests to enqueue</param>
        public RequestQueueTask Enqueue(params IRequest<dynamic>[] requests)
        {
            var queueTask = new RequestQueueTask(this, requests);

            _lock.EnterWriteLock();
            _queue.InsertAll(requests);
            _lock.ExitWriteLock();

            Task.Run(() =>
            {
                for(var i=0; i<requests.Count(); i++) {
                    DequeueAsync().Wait();
                }

                queueTask.NotifyCompletion();
            });

            return queueTask;
        }

        /// <summary>
        /// Dequeues the next request in queue and executes it
        /// </summary>
        /// <returns>The execution task</returns>
        private async Task DequeueAsync()
        {
            await _maxThreadsSemaphoreSlim.WaitAsync();

            if (CurrentThreadCount == 1)
            {
                OnStarted();
            }

            _lock.EnterWriteLock();
            var request = _queue.DeleteMax();
            _concurrentRequests.Add(request);
            _lock.ExitWriteLock();

            await request.GetResponseAsync().ContinueWith(delegate
            {
                lock (_lockObject)
                {
                    _lock.EnterWriteLock();
                    _concurrentRequests.Remove(request);
                    _lock.ExitWriteLock();

                    _maxThreadsSemaphoreSlim.Release();

                    if (IsEmpty)
                    {
                        OnCompleted();
                    }
                }
            });

            
        }

        /// <summary>
        /// Deletes all running and waiting 
        /// requests from queue
        /// </summary>
        /// <returns>List of deleted requests</returns>
        private List<IRequest<dynamic>> DeleteAll()
        {
            _lock.EnterWriteLock();

            //delete running requests
            var requests = new List<IRequest<dynamic>>(_concurrentRequests);
            _concurrentRequests.Clear();

            //delete waiting requests
            requests.AddRange(_queue.DeleteAll());

            _lock.ExitWriteLock();

            return requests;
        } 

        /// <summary>
        /// Aborts all requests and removes these from queue
        /// </summary>
        /// <returns>Abortion task</returns>
        public async Task AbortAllAsync()
        {
            await Task.Run(() =>
            {
                var requests = DeleteAll();
                var count = requests.Count();

                if(count > 0) {
                    var completionSemaphore = new SemaphoreSlim(0, count);

                    foreach(var request in requests)
                    {
                        request.AbortAsync().GetAwaiter().OnCompleted(() => completionSemaphore.Release());
                    }

                    completionSemaphore.Wait();
                }
            });
        }

        /// <summary>
        /// Aborts a specific request and removes it from queue
        /// </summary>
        /// <param name="request">The request to abort</param>
        /// <returns>Abortion task</returns>
        public async Task AbortAsync(IRequest<dynamic> request)
        {
            await Task.Run(() =>
            {
                _lock.EnterWriteLock();
                _queue.Delete(request);
                _lock.ExitWriteLock();

                request.AbortAsync().Wait();
            });
        }       
    }
}
   
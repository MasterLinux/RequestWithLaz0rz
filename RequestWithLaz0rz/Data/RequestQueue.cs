using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace RequestWithLaz0rz.Data
{
    /// <summary>
    /// Interface for queueable requests
    /// </summary>
    public interface IRequest : IPriorityQueueItem, IComparable<IRequest> //TODO rename to ITask?
    {      
        /// <summary>
        /// Gets the execution priority. A request with a higher
        /// priority will be executed before a request
        /// with a lower one.
        /// </summary>
        RequestPriority Priority { get; }

        /// <summary>
        /// Executes the request
        /// </summary>
        Task ExecuteAsync();

        /// <summary>
        /// Stops the execution of this request.
        /// </summary>
        /// <returns>This request</returns>
        Task AbortAsync();

        /// <summary>
        /// Flag which indicates whether the 
        /// request is currently executing.
        /// </summary>
        bool IsExecuting { get; }

        /// <summary>
        /// Gets whether the request is aborted
        /// </summary>
        bool IsAborted { get; }
    }

    public delegate void StartedHandler(RequestQueue sender);
    public delegate void CompletedHandler(RequestQueue sender);

    /// <summary>
    /// Thread-safe priority queue for requests
    /// </summary>
    /// <example></example>
    /// <code>
    /// //create a new queue which allows 3 threads running concurrently
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
    /// await queue.EnqueueAsync(request);
    /// await queue.EnqueueAsync(anotherRequest);
    /// 
    /// //cancel a specific request
    /// await queue.AbortAsync(request);
    /// 
    /// //cancel all requests
    /// await queue.AbortAllAsync();
    /// 
    /// </code>
    public class RequestQueue //TODO rename to TaskQueue?
    {
        private readonly PriorityQueue<IRequest> _queue = new PriorityQueue<IRequest>();
        private readonly ReaderWriterLockSlim _lock = new ReaderWriterLockSlim();
        private readonly SemaphoreSlim _maxThreadsSemaphore;
        private readonly object _lockObject = new object();
        private readonly List<IRequest> _currentRequests = new List<IRequest>();

        /// <summary>
        /// Maximum number of requests that can be granted concurrently
        /// </summary>
        public const int DefaultMaxThreadCount = 4;

        public event StartedHandler Started;

        protected virtual void OnStarted()
        {
            var handler = Started;
            if (handler != null) handler(this);
        }

        public event CompletedHandler Completed;

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
            _maxThreadsSemaphore = new SemaphoreSlim(maxThreadCount, maxThreadCount);
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
                var count = _queue.Count + _currentRequests.Count;
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
                return MaxThreadCount - _maxThreadsSemaphore.CurrentCount;
            }
        }

        /// <summary>
        /// Enqueues a request and starts it 
        /// </summary>
        /// <param name="request">The request to enqueue</param>
        public async Task EnqueueAsync(IRequest request)
        {
            _lock.EnterWriteLock();
            _queue.Insert(request);
            _lock.ExitWriteLock();

            await DequeueAsync();
        }

        /// <summary>
        /// Dequeues the next request in queue and executes it
        /// </summary>
        /// <returns>The execution task</returns>
        private async Task DequeueAsync()
        {
            await _maxThreadsSemaphore.WaitAsync();

            if (CurrentThreadCount == 1)
            {
                OnStarted();
            }

            _lock.EnterWriteLock();
            var request = _queue.DeleteMax();
            _currentRequests.Add(request);
            _lock.ExitWriteLock();

            request.ExecuteAsync().GetAwaiter().OnCompleted(() =>
            {
                lock (_lockObject)
                {                    
                    _lock.EnterWriteLock();
                    _currentRequests.Remove(request);                  
                    _lock.ExitWriteLock();                   
                                     
                    if (IsEmpty)
                    {
                        OnCompleted();
                    }

                    _maxThreadsSemaphore.Release();
                }               
            });           
        }

        /// <summary>
        /// Deletes all running and waiting 
        /// requests from queue
        /// </summary>
        /// <returns>List of deleted requests</returns>
        private List<IRequest> DeleteAll()
        {
            _lock.EnterWriteLock();

            //delete running requests
            var requests = new List<IRequest>(_currentRequests);
            _currentRequests.Clear();

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
        public async Task AbortAsync(IRequest request)
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
   
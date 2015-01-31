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
    /// Priority queue for requests
    /// </summary>
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
            await Task.Factory.StartNew(() =>
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
            await Task.Factory.StartNew(() =>
            {
                _lock.EnterWriteLock();
                _queue.Delete(request);
                _lock.ExitWriteLock();

                request.AbortAsync().Wait();
            });
        }       
    }


    /// <summary>
    /// Request queue which handles the execution of all started requests. It allows
    /// to cancel a specific or all requests which are currently running. In addition it provides
    /// events to listen for lifecycle events, like the completed event.
    /// </summary>
    /// <example>This queue uses the multiton pattern.</example>
    /// <code>
    /// //get the default queue
    /// var queue = RequestQueue.GetRequestQueue();
    /// 
    /// //get a specific queue
    /// var specificQueue = RequestQueue.GetRequestQueue("aKey");
    /// 
    /// //register lifecycle events
    /// queue.Started += () => { /* show loading indicator */ }
    /// queue.Completed += () => { /* hide loading indicator */ }
    /// 
    /// //create a request and execute to add it to queue,
    /// //because the request enqueues itself when executing
    /// var request = new ExampleRequest();
    /// var task = request.GetResponseAsync();
    /// 
    /// //cancel a specific request
    /// await queue.AbortAsync(request);
    /// 
    /// //cancel all requests
    /// await queue.AbortAllAsync();
    /// 
    /// </code>
    /**
    public class RequestQueue2
    {
        private static readonly ConcurrentDictionary<string, RequestQueue> Instances = new ConcurrentDictionary<string, RequestQueue>();
        private readonly PriorityQueue<IPriorityRequest> _queue = new PriorityQueue<IPriorityRequest>();
        private readonly List<IPriorityRequest> _executionQueue = new List<IPriorityRequest>(); 
        private int _threadCount;

        /// <summary>
        /// Maximum number of threads running parallel.
        /// </summary>
        public const int MaxThreads = 4; //TODO implement getter and setter

        /// <summary>
        /// The key of the default request queue instance
        /// </summary>
        public const string DefaultQueueKey = "$_DefaultRequestQueue_$";      

        /// <summary>
        /// Initializes the request queue
        /// </summary>
        /// <param name="id">Unique identifier which identifies the queue</param>
        private RequestQueue2(string id)
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
        /// Gets the number of requests actually 
        /// containted in this queue
        /// </summary>
        public int Count
        {
            get { return _queue.Count + _executionQueue.Count; }
        }

        /// <summary>
        /// Unique ID of this queue
        /// </summary>
        public string Id { get; private set; }

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
                _executionQueue.Add(request);

                request.RunAsync().ContinueWith(task =>
                {
                    //on request completed
                    _executionQueue.Remove(request);
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
        /// Cancels and removes a specific request from queue
        /// </summary>
        /// <param name="request">The request to cancel</param>
        public async Task AbortAsync(IPriorityRequest request)
        {
            await Task.Factory.StartNew(() =>
            {
                _queue.Delete(request);
                request.AbortAsync().Wait();
            });
        }

        /// <summary>
        /// Cancels and removes each request in queue
        /// </summary>
        public async Task AbortAllAsync()
        {
            await Task.Factory.StartNew(() =>
            {
                //delete all waiting requests
                var requests = _queue.DeleteAll();
                foreach (var priorityRequest in requests.Where(request => request != null))
                {
                    priorityRequest.AbortAsync().Wait();
                }

                //delete all running requests
                _executionQueue.CopyTo(requests);
                foreach (var priorityRequest in requests.Where(request => request != null))
                {
                    priorityRequest.AbortAsync().Wait(); 
                }
            });
        }
    } */
}
   
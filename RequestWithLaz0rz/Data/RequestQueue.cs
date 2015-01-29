using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace RequestWithLaz0rz.Data
{
    public delegate void CompletedHandler<TResponse>(IRequest<TResponse> sender, RequestCompletedEventArgs<TResponse> args);

    public delegate void ErrorOccurredHandler<TResponse>(IRequest<TResponse> sender, RequestErrorOccurredEventArgs args);

    public class RequestErrorOccurredEventArgs : EventArgs
    {
        //TODO implement
    }

    public class RequestCompletedEventArgs<TResponse> : EventArgs
    {
        public TResponse Response { private set; get; }

        public RequestCompletedEventArgs(TResponse response)
        {
            Response = response;
        }
    }

    public interface IResponseModel
    {
        //TODO implement inherits from ISerializable?
    }

    public interface IRequest<TResponse> : IPriorityQueueItem, IComparable<IRequest<IResponseModel>>
    {
        /// <summary>
        /// Event which is invoked whenever the request is
        /// successfully executed. 
        /// </summary>
        event CompletedHandler<TResponse> Completed;

        /// <summary>
        /// Event which is invoked whenever an error occurred
        /// when executing.
        /// </summary>
        event ErrorOccurredHandler<TResponse> ErrorOccurred;

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

    public class RequestQueue
    {
        private PriorityQueue<IRequest<IResponseModel>> _queue = new PriorityQueue<IRequest<IResponseModel>>();
        private readonly Object _threadLock = new Object();
        private int _currentThreads;

        public RequestQueue(int maxThreads)
        {
            MaxThreads = maxThreads;
        }

        /// <summary>
        /// Gets the number of requests actually 
        /// containted in this queue
        /// </summary>
        public int Count
        {
            get { return _queue.Count; }
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
        /// Gets the max number of threads 
        /// allowed executing concurrent
        /// </summary>
        public int MaxThreads { get; private set; }

        /// <summary>
        /// Enqueues a request and starts it 
        /// </summary>
        /// <param name="request">The request to enqueue</param>
        public void Enqueue(IRequest<IResponseModel> request)
        {
            _queue.Insert(request);

            if (IncrementCount())
            {
                ExecuteNext();
            }
        }

        public async Task AbortAllAsync()
        {
            
        }

        public async Task AbortAsync(IRequest<IResponseModel> request)
        {

        }

        /// <summary>
        /// Executes the next request in queue
        /// </summary>
        private void ExecuteNext()
        {
            var task = DequeueAsync();

            task.GetAwaiter().OnCompleted(() =>
            {
                if(DecrementCount() > 0) 
                {
                    ExecuteNext();
                }
            });
        }

        /// <summary>
        /// Dequeues the next request in queue and executes it
        /// </summary>
        /// <returns>The execution task</returns>
        private async Task DequeueAsync()
        {
            var request = _queue.DeleteMax();
            await request.ExecuteAsync();
        }

        /// <summary>
        /// Increments the counter of concurrent threads 
        /// but the max number ob concurrent threads is reached
        /// </summary>
        /// <returns>true if the counter is incremented, false otherwise</returns>
        private bool IncrementCount()
        {
            var isIncremented = false;

            lock (_threadLock)
            {
                if (_currentThreads < MaxThreads)
                {
                    ++_currentThreads;
                    isIncremented = true;
                } 
            }

            return isIncremented;
        }

        /// <summary>
        /// Decrements the counter of concurrent threads 
        /// </summary>
        /// <returns>The number of concurrent threads</returns>
        private int DecrementCount()
        {
            lock (_threadLock)
            {
                if (_currentThreads > 0)
                {
                    --_currentThreads;
                }
            }

            return _currentThreads;
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
   
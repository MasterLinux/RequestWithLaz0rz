using System;
using System.Threading;
using System.Threading.Tasks;
using RequestWithLaz0rz.Data;
using RequestWithLaz0rz.Type;

namespace RequestWithLaz0rzTest.Mock
{
    public class RequestMock : IRequest<Object>
    {
        private readonly SemaphoreSlim _semaphoreSlim = new SemaphoreSlim(1);

        public event Action Started;

        public event Action Completed;

        public RequestMock(RequestPriority priority)
        {
            Priority = priority;
        }

        public int CompareTo(IRequest<Object> other)
        {
            return Priority.CompareTo(other.Priority);
        }

        public RequestPriority Priority { get; private set; }

        public async Task<Response<Object>> GetResponseAsync()
        {
            if (Started != null) Started();

            _semaphoreSlim.Wait();
            IsExecuting = true;

            await Task.Factory.StartNew(() =>
            {
                while (!IsAborted)
                {
                    //does nothing
                }

                IsExecuting = false;
                _semaphoreSlim.Release();

                if (Completed != null) Completed();
            });

            return null;
        }

        public async Task AbortAsync()
        {
            await Task.Factory.StartNew(() =>
            {
                IsAborted = true;

                //wait for request completion
                _semaphoreSlim.Wait();
            });
        }

        public bool IsExecuting { get; private set; }

        public bool IsAborted { get; private set; }

        public int QueueHandle { get; set; }
    }
}

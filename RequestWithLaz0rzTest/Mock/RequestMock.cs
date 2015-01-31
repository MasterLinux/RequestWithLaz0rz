using System.Threading;
using System.Threading.Tasks;
using RequestWithLaz0rz;
using RequestWithLaz0rz.Data;

namespace RequestWithLaz0rzTest.Mock
{
    public class RequestMock : IRequest
    {
        private readonly SemaphoreSlim _semaphoreSlim = new SemaphoreSlim(1);
        public RequestMock(RequestPriority priority)
        {
            Priority = priority;
        }

        public int CompareTo(IRequest other)
        {
            return Priority.CompareTo(other.Priority);
        }

        public RequestPriority Priority { get; private set; }

        public async Task ExecuteAsync()
        {
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
            });
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

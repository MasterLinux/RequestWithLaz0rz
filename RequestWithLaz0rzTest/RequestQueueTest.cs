using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using RequestWithLaz0rz;
using RequestWithLaz0rz.Data;
using RequestWithLaz0rz.Extension;

namespace RequestWithLaz0rzTest
{
    [TestClass]
    public class RequestQueueTest
    {

        [TestMethod]
        public void TestGetInstance()
        {
            const string expectedDefaultQueueId = RequestQueue.DefaultQueueKey;
            const string expectedTestQueueId = "test";
            
            var defaultQueue = RequestQueue.GetRequestQueue();
            var defaultQueue2 = RequestQueue.GetRequestQueue();
            var testQueue = RequestQueue.GetRequestQueue(expectedTestQueueId);
            var testQueue2 = RequestQueue.GetRequestQueue(expectedTestQueueId);

            Assert.AreSame(defaultQueue, defaultQueue2);
            Assert.AreSame(testQueue, testQueue2);
            Assert.AreNotSame(defaultQueue, testQueue);
            Assert.AreEqual(expectedDefaultQueueId, defaultQueue.Id);
            Assert.AreEqual(expectedTestQueueId, testQueue.Id);
        }

        [TestMethod]
        public void TestQueueIsEmpty()
        {
            var queue = RequestQueue.GetRequestQueue("emptyTest");
    
            Assert.IsTrue(queue.IsEmpty);
            Assert.IsFalse(queue.IsNotEmpty);
        }

        [TestMethod]
        public void TestQueueIsNotEmpty()
        {
            var queue = RequestQueue.GetRequestQueue("notEmptyTest");
            var expensiveRequest = new ExpensiveRequest(RequestPriority.High);
            queue.Enqueue(expensiveRequest);

            Assert.IsFalse(queue.IsEmpty);
            Assert.IsTrue(queue.IsNotEmpty);

            expensiveRequest.AbortAsync().Wait();
        }

        [TestMethod]
        public void TestEnqueueRequest()
        {
            var queue = RequestQueue.GetRequestQueue("enqueueTest");

            var expensiveRequest = new ExpensiveRequest(RequestPriority.High);
            var expensiveRequest2 = new ExpensiveRequest(RequestPriority.High);
            var expensiveRequest3 = new ExpensiveRequest(RequestPriority.High);
            var expensiveRequest4 = new ExpensiveRequest(RequestPriority.High);
            var expensiveRequest5 = new ExpensiveRequest(RequestPriority.High);

            queue.Enqueue(expensiveRequest);
            queue.Enqueue(expensiveRequest2);
            queue.Enqueue(expensiveRequest3);
            queue.Enqueue(expensiveRequest4);
            queue.Enqueue(expensiveRequest5);

            Assert.IsTrue(queue.IsNotEmpty);
            Assert.IsTrue(expensiveRequest.IsBusy);
            Assert.IsTrue(expensiveRequest2.IsBusy);
            Assert.IsTrue(expensiveRequest3.IsBusy);
            Assert.IsTrue(expensiveRequest4.IsBusy);

            //just four request will be executed at the same time
            Assert.IsFalse(expensiveRequest5.IsBusy);

            expensiveRequest.AbortAsync().Wait();

            Assert.IsFalse(expensiveRequest.IsBusy);
            Assert.IsTrue(expensiveRequest5.IsBusy);

            expensiveRequest2.AbortAsync().Wait();
            expensiveRequest3.AbortAsync().Wait();
            expensiveRequest4.AbortAsync().Wait();
            expensiveRequest5.AbortAsync().Wait();
        }

        [TestMethod]
        public void TestAbortRequest()
        {
            
        }

        /// <summary>
        /// Request mock required to test the request queue
        /// </summary>
        public class ExpensiveRequest : IPriorityRequest
        {
            private readonly RequestPriority _priority;
            private SemaphoreSlim _completedSignal;

            public ExpensiveRequest(RequestPriority priority)
            {
                _priority = priority;
                IsCompleted = false;
            }

            public async Task RunAsync()
            {
                IsBusy = true;

                var task = Task.Factory.StartNew(() =>
                {
                    while (!IsCompleted)
                    {
                        //does nothing
                    }

                    IsBusy = false;
                    _completedSignal.Release();
                });

                await task;
            }

            public async Task AbortAsync()
            {
                _completedSignal = new SemaphoreSlim(0, 1);

                IsAborted = true;
                IsCompleted = true;

                await _completedSignal.WaitAsync();
            }

            public bool IsBusy { get; private set; }

            public bool IsAborted { get; private set; }

            private bool IsCompleted { get; set; }

            public int CompareTo(IPriorityRequest other)
            {
                return Priority.Compare(other.Priority);
            }

            public RequestPriority Priority
            {
                get { return _priority; }
            }

            public int QueueHandle { get; set; }
        }        
    }
}

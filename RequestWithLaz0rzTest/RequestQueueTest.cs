using System;
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
        private RequestQueue _queue;

        [TestInitialize]
        public void InitializeTest()
        {
            //_queue = new RequestQueue();
        }

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
            var queue = RequestQueue.GetRequestQueue("emptyTest");
            var expensiveRequest = new ExpensiveRequest(RequestPriority.High);
            queue.Enqueue(expensiveRequest);

            Assert.IsFalse(queue.IsEmpty);
            Assert.IsTrue(queue.IsNotEmpty);

            queue.Cancel(expensiveRequest);
        }

        public class ExpensiveRequest : IPriorityRequest
        {
            private readonly RequestPriority _priority;

            public ExpensiveRequest(RequestPriority priority)
            {
                _priority = priority;
                IsCompleted = false;
            }

            public async Task RunAsync()
            {
                var task = new Task(() =>
                {
                    while (!IsCompleted)
                    {
                        //does nothing
                    }
                });

                await task;
            }

            public void Abort()
            {
                IsCompleted = true;
            }

            private bool IsCompleted { get; set; }

            public int CompareTo(IPriorityRequest other)
            {
                return Priority.Compare(other.Priority);
            }

            public RequestPriority Priority
            {
                get { return _priority; }
            }
        }

        [TestMethod]
        public void TestEnqueueRequest()
        {
            var queue = RequestQueue.GetRequestQueue("enqueueTest");
    
            //queue.
        }
    }
}

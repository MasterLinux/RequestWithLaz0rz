using System;
using System.Threading;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using RequestWithLaz0rz;
using RequestWithLaz0rz.Data;
using RequestWithLaz0rzTest.Mock;

namespace RequestWithLaz0rzTest
{
    [TestClass]
    public class RequestQueueTest
    {

        [TestMethod]
        public void TestShouldAddAndExecuteRequest()
        {
            var queueUnderTest = new RequestQueue();
            var request = new RequestMock(RequestPriority.High);
            const int expectedCountAfterEnqueueing = 1;

            queueUnderTest.EnqueueAsync(request).Wait();

            Assert.IsTrue(request.IsExecuting, "Request is not executing");
            Assert.IsTrue(queueUnderTest.IsNotEmpty, "Queue is empty but should contain {0} request(s)", expectedCountAfterEnqueueing);
            Assert.IsFalse(queueUnderTest.IsEmpty, "Queue is empty but should contain {0} request(s)", expectedCountAfterEnqueueing);
            Assert.AreEqual(expectedCountAfterEnqueueing, queueUnderTest.Count, "Queue should contain {0} request(s), but contains {1} requests", expectedCountAfterEnqueueing, queueUnderTest.Count);

            //clean up
            request.AbortAsync().Wait();
        }

        [TestMethod]
        public void TestShouldAbortRequest()
        {
            var queueUnderTest = new RequestQueue();
            var request = new RequestMock(RequestPriority.High);
            const int expectedCountAfterEnqueueing = 1;
            const int expectedCountAfterAbort = 0;

            queueUnderTest.EnqueueAsync(request).Wait();

            Assert.IsFalse(request.IsAborted, "Request is aborted");
            Assert.AreEqual(expectedCountAfterEnqueueing, queueUnderTest.Count, "Queue should contain {0} request(s), but contains {1} requests", expectedCountAfterAbort, queueUnderTest.Count);

            queueUnderTest.AbortAsync(request).Wait();

            Assert.IsTrue(request.IsAborted, "Request is not aborted");
            Assert.IsFalse(request.IsExecuting, "Request is executing but should be aborted");
            Assert.IsFalse(queueUnderTest.IsNotEmpty, "Queue contains {0} request(s) but should be empty", queueUnderTest.Count);
            Assert.IsTrue(queueUnderTest.IsEmpty, "Queue contains {0} request(s) but should be empty", queueUnderTest.Count);
            Assert.AreEqual(expectedCountAfterAbort, queueUnderTest.Count, "Queue should contain {0} request(s), but contains {1} requests", expectedCountAfterAbort, queueUnderTest.Count);
        }

        [TestMethod]
        public void TestShouldAbortAllRequests()
        {
            var queueUnderTest = new RequestQueue();
            var request = new RequestMock(RequestPriority.High);
            var anotherRequest = new RequestMock(RequestPriority.Low);
            const int expectedCountAfterEnqueueing = 2;
            const int expectedCountAfterAbort = 0;

            queueUnderTest.EnqueueAsync(request).Wait();
            queueUnderTest.EnqueueAsync(anotherRequest).Wait();

            Assert.IsFalse(request.IsAborted, "Request is aborted");
            Assert.IsFalse(anotherRequest.IsAborted, "Another request is aborted");
            Assert.AreEqual(expectedCountAfterEnqueueing, queueUnderTest.Count, "Queue should contain {0} request(s), but contains {1} requests", expectedCountAfterAbort, queueUnderTest.Count);

            queueUnderTest.AbortAllAsync().Wait();

            Assert.IsTrue(request.IsAborted, "Request is not aborted");
            Assert.IsTrue(anotherRequest.IsAborted, "Another request is not aborted");
            Assert.AreEqual(expectedCountAfterAbort, queueUnderTest.Count, "Queue should contain {0} request(s), but contains {1} requests", expectedCountAfterAbort, queueUnderTest.Count);
        }

        [TestMethod]
        public void TestShouldInvokeStartedAndCompletedEvent()
        {
            var queueUnderTest = new RequestQueue();
            var request = new RequestMock(RequestPriority.High);
            var anotherRequest = new RequestMock(RequestPriority.Low);           
            int startedEventCallCount = 0, completedEventCallCount = 0;
            var requestsCompletedSemaphoreSlim = new SemaphoreSlim(0, 1);
            const int expectedStartedEventCallCount = 1, expectedCompletedEventCallCount = 1;

            StartedHandler startedHandlerMock = sender => Interlocked.Increment(ref startedEventCallCount);
            CompletedHandler completionHandlerMock = sender =>
            {
                requestsCompletedSemaphoreSlim.Release();
                Interlocked.Increment(ref completedEventCallCount);
            };       

            //set event handler
            queueUnderTest.Started += startedHandlerMock;
            queueUnderTest.Completed += completionHandlerMock;            

            //first started request should invoke started handler 
            queueUnderTest.EnqueueAsync(request).Wait();
            Assert.AreEqual(expectedStartedEventCallCount, startedEventCallCount, "Started event handler should be called once");
            
            queueUnderTest.EnqueueAsync(anotherRequest).Wait();         
            queueUnderTest.AbortAsync(request).Wait();

            //completion handler must not be called if there are currently running requests
            Assert.AreEqual(0, completedEventCallCount, "Completed event handler should not be called");
            queueUnderTest.AbortAsync(anotherRequest).Wait();

            //wait for completion handler calls
            requestsCompletedSemaphoreSlim.Wait();

            Assert.AreEqual(expectedStartedEventCallCount, startedEventCallCount, "Started event handler should be called once");
            Assert.AreEqual(expectedCompletedEventCallCount, completedEventCallCount, "Completed event handler should be called once");
        }

        [TestMethod]
        public void TestShouldLimitMaxNumberOfThreadsRunningConcurrently()
        {
            const int expectedMaxThreadCount = 2;
            const int expectedCount = 3;
            var requestCompletedSemaphoreSlim = new SemaphoreSlim(0, 1);
            var queueUnderTest = new RequestQueue(expectedMaxThreadCount);
            var request = new RequestMock(RequestPriority.High);
            var anotherRequest = new RequestMock(RequestPriority.Low);
            var yetAnotherRequest = new RequestMock(RequestPriority.Medium);

            queueUnderTest.EnqueueAsync(request).GetAwaiter().OnCompleted(() => requestCompletedSemaphoreSlim.Release());
            queueUnderTest.EnqueueAsync(anotherRequest).GetAwaiter().OnCompleted(() => { });
            queueUnderTest.EnqueueAsync(yetAnotherRequest).GetAwaiter().OnCompleted(() => { });

            Assert.AreEqual(expectedCount, queueUnderTest.Count, "Queue should contain {0} request(s) but contains {1} request(s)", expectedCount, queueUnderTest.Count);
            Assert.AreEqual(expectedMaxThreadCount, queueUnderTest.MaxThreadCount);
            Assert.AreEqual(expectedMaxThreadCount, queueUnderTest.CurrentThreadCount);
            Assert.IsTrue(request.IsExecuting);
            Assert.IsTrue(anotherRequest.IsExecuting);
            Assert.IsFalse(yetAnotherRequest.IsExecuting);

            queueUnderTest.AbortAsync(request).Wait();

            //wait for completion handler call
            requestCompletedSemaphoreSlim.Wait();

            Assert.IsTrue(yetAnotherRequest.IsExecuting);

            queueUnderTest.AbortAsync(anotherRequest).Wait();
            queueUnderTest.AbortAsync(yetAnotherRequest).Wait();
        }
    }
}

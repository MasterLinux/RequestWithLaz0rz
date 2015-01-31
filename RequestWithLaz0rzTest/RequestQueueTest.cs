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
            var requestsCompletedSemaphoreSlim = new SemaphoreSlim(1);
            StartedHandler startedMock = sender =>
            {
                Interlocked.Increment(ref startedEventCallCount);
                requestsCompletedSemaphoreSlim.Release();
            };
            CompletedHandler completedMock = sender => Interlocked.Increment(ref completedEventCallCount);
            const int expectedStartedEventCallCount = 1, expectedCompletedEventCallCount = 1;

            queueUnderTest.Started += startedMock;
            queueUnderTest.Completed += completedMock;

            queueUnderTest.EnqueueAsync(request).Wait();
            queueUnderTest.EnqueueAsync(anotherRequest).Wait();  
        
            queueUnderTest.AbortAsync(request).Wait();
            queueUnderTest.AbortAsync(anotherRequest).Wait();

            //request completion handler run asynchonously
            requestsCompletedSemaphoreSlim.Wait();

            Assert.AreEqual(expectedStartedEventCallCount, startedEventCallCount, "");
            Assert.AreEqual(expectedCompletedEventCallCount, completedEventCallCount);
        }

    }
}

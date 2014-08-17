using System;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using RequestWithLaz0rz.Data;

namespace RequestWithLaz0rzTest
{
    [TestClass]
    public class PriorityQueueTest
    {
        public const int InitalCapacity = 5;
        private PriorityQueue<ItemMock> _queue;

        [TestInitialize]
        public void InitializeTest()
        {
            _queue = new PriorityQueue<ItemMock>(InitalCapacity);
        }

        [TestMethod]
        public void TestPreconditions()
        {
            Assert.IsNotNull(_queue, "PriorityQueue is null");
        }

        [TestMethod]
        public void TestInsertItem()
        {
            var expectedSize = 0;
            var actualSize = _queue.Size;

            //check whether queue is empty
            Assert.AreEqual(expectedSize, actualSize, "PriortyQueue is not empty");
            Assert.IsTrue(_queue.IsEmpty);

            _queue.Insert(new ItemMock(1, "a"));
            expectedSize = 1;
            actualSize = _queue.Size;

            Assert.AreEqual(expectedSize, actualSize, "No item added to priority queue");
            Assert.IsFalse(_queue.IsEmpty);
        }

        public class ItemMock : IComparable<ItemMock>
        {
            public ItemMock(int priority, string value)
            {
                Priority = priority;
                Value = value;
            }

            public int Priority { get; private set; }

            public string Value { get; private set; }

            public int CompareTo(ItemMock item)
            {
                throw new NotImplementedException();
            }
        }
    }
}

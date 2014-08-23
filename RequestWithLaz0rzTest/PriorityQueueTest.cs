using System;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using RequestWithLaz0rz.Data;

namespace RequestWithLaz0rzTest
{
    [TestClass]
    public class PriorityQueueTest
    {
        public const int InitalCapacity = 2;
        private PriorityQueue<ItemMock> _queue;

        [TestInitialize]
        public void InitializeTest()
        {
            _queue = new PriorityQueue<ItemMock>();
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
            var actualSize = _queue.Count;

            //check whether queue is empty
            Assert.AreEqual(expectedSize, actualSize, "PriortyQueue is not empty");
            Assert.IsFalse(_queue.IsNotEmpty);
            Assert.IsTrue(_queue.IsEmpty);

            _queue.Insert(new ItemMock(1, "a"));
            _queue.Insert(new ItemMock(2, "b"));
            _queue.Insert(new ItemMock(3, "c"));
            expectedSize = 3;
            actualSize = _queue.Count;

            //check whether all items are added
            Assert.AreEqual(expectedSize, actualSize, "No item added to priority queue");
            Assert.IsTrue(_queue.IsNotEmpty);
            Assert.IsFalse(_queue.IsEmpty);        
        }

        [TestMethod]
        public void TestGetItemWithHighestPriority()
        {
            const int expectedPriority = 8;
            const string expectedValue = "b";
            var expectedItem = new ItemMock(expectedPriority, expectedValue);

            _queue.Insert(new ItemMock(1, "a"));
            _queue.Insert(expectedItem);
            _queue.Insert(new ItemMock(2, "c"));
            _queue.Insert(new ItemMock(3, "c"));
            _queue.Insert(new ItemMock(2, "c"));
            _queue.Insert(new ItemMock(4, "c"));
            _queue.Insert(new ItemMock(0, "c"));

            var actualItem = _queue.Max;

            Assert.IsNotNull(actualItem, "Required item is null");
            Assert.AreEqual(expectedItem, actualItem, "Actual item <prio: {0}, val: {1}> is not equal to the expected one <prio: {2}, val: {3}>", actualItem.Priority, actualItem.Value, expectedItem.Priority, expectedItem.Value);
            Assert.AreEqual(expectedPriority, actualItem.Priority, "Actual priority is not equal to expected one");
            Assert.AreEqual(expectedValue, actualItem.Value, "Actual value is not equal to expected one");
        }

        [TestMethod]
        public void TestGetAndRemoveItemWithHighestPriority()
        {
            var expectedItem1 = new ItemMock(1, "a");
            var expectedItem2 = new ItemMock(2, "b");
            var expectedItem3 = new ItemMock(3, "c");
            var expectedItem4 = new ItemMock(4, "d");
            var expectedItem5 = new ItemMock(5, "e");
            var expectedItem6 = new ItemMock(6, "f");
            var expectedItem7 = new ItemMock(7, "g");

            _queue.Insert(expectedItem6);
            _queue.Insert(expectedItem2);
            _queue.Insert(expectedItem4);
            _queue.Insert(expectedItem7);
            _queue.Insert(expectedItem5);
            _queue.Insert(expectedItem3);
            _queue.Insert(expectedItem1);

            var actualItem = _queue.DeleteMax();
            Assert.AreEqual(expectedItem7, actualItem);

            actualItem = _queue.DeleteMax();
            Assert.AreEqual(expectedItem6, actualItem);

            actualItem = _queue.DeleteMax();
            Assert.AreEqual(expectedItem5, actualItem);

            actualItem = _queue.DeleteMax();
            Assert.AreEqual(expectedItem4, actualItem);

            actualItem = _queue.DeleteMax();
            Assert.AreEqual(expectedItem3, actualItem);

            actualItem = _queue.DeleteMax();
            Assert.AreEqual(expectedItem2, actualItem);

            actualItem = _queue.DeleteMax();
            Assert.AreEqual(expectedItem1, actualItem);

            Assert.IsTrue(_queue.IsEmpty);
        }

        public class ItemMock : IComparable<ItemMock>
        {
            public ItemMock()
            {
                
            }
            public ItemMock(int priority, string value)
            {
                Priority = priority;
                Value = value;
            }

            public int Priority { get; private set; }

            public string Value { get; private set; }

            public int CompareTo(ItemMock another)
            {
                return Priority - another.Priority;
            }
        }
    }
}

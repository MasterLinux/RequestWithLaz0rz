using System;
using RequestWithLaz0rz.Data;

namespace RequestWithLaz0rzTest.Mock
{
    /// <summary>
    /// A simple priority queue item mock used to test the PriorityQueue
    /// </summary>
    public class PriorityQueueItemMock : IComparable<PriorityQueueItemMock>, IPriorityQueueItem
    {
        /// <summary>
        /// Initializes the mock with a priority and a value
        /// </summary>
        /// <param name="priority">The priority of the item</param>
        /// <param name="value">The value of the item</param>
        public PriorityQueueItemMock(int priority, string value)
        {
            Priority = priority;
            Value = value;
        }

        public int Priority { get; private set; }

        public string Value { get; private set; }

        public int CompareTo(PriorityQueueItemMock another)
        {
            return Priority - another.Priority;
        }

        public int QueueHandle { get; set; }
    }
}

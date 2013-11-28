using C5;

namespace RequestWithLaz0rz
{
    public interface IRequest //TODO as abstract class?
    {
        /// <summary>
        /// Queue handle which is used to get a specific request
        /// from queue. This getter must not be used directly. 
        /// </summary>
        IPriorityQueueHandle<Priority<IRequest>> QueueHandle { get; set; }

        /// <summary>
        ///  Starts the request. This method must not be used directly.
        ///  </summary>
        void Run();
    }
}

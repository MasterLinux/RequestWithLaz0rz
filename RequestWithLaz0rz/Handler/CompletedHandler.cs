using System;

namespace RequestWithLaz0rz.Handler
{
    public delegate void CompletedHandler<TResponse>(Request<TResponse> sender, CompletedEventArgs<TResponse> args);

    public class CompletedEventArgs<TResponse> : EventArgs
    {
        public TResponse Response { set; get; }

        public bool IsCached { get; set; }

        public int StatusCode { get; set; }
    }
}

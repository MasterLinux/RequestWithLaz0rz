using System;

namespace RequestWithLaz0rz.Handler
{
    public delegate void CompletedHandler<TResponse>(Request<TResponse> sender, CompletedEventArgs<TResponse> args);

    public class CompletedEventArgs<TResponse> : EventArgs
    {
        public CompletedEventArgs(int statusCode, TResponse response, bool isCached)
        {
            Response = response;
        }

        public TResponse Response
        {
            private set; get;
        }
    }
}

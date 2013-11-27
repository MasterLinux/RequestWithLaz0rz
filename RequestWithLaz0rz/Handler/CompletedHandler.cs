using System;
using System.Net;

namespace RequestWithLaz0rz.Handler
{
    public delegate void CompletedHandler<TResponse>(
        Request<TResponse> sender, 
        CompletedEventArgs<TResponse> args
    );

    public class CompletedEventArgs<TResponse> : EventArgs
    {
        public TResponse Response { set; get; }

        public bool IsCached { get; set; }

        public bool IsErrorOccured { get; set; }

        public string ErrorMessage { get; set; }

        public HttpStatusCode StatusCode { get; set; }
    }
}

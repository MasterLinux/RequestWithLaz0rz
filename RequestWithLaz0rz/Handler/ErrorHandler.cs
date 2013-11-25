using System;

namespace RequestWithLaz0rz.Handler
{
    public delegate void ErrorHandler<TResponse>(Request<TResponse> sender, ErrorEventArgs args);

    public class ErrorEventArgs : EventArgs
    {
        public ErrorEventArgs(int statusCode, string message)
        {
            StatusCode = statusCode;
            Message = message;
        }

        /// <summary>
        /// Gets the HTTP status code 
        /// </summary>
        public int StatusCode { get; private set; }

        /// <summary>
        /// Gets the message of the occured error
        /// </summary>
        public string Message { get; private set; }
    }
}

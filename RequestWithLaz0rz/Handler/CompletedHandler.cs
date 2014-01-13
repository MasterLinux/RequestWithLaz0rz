using System;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;

namespace RequestWithLaz0rz.Handler
{
    public delegate void CompletedHandler<TResponse>(
        Request<TResponse> sender, 
        CompletedEventArgs<TResponse> args
    );

    public class CompletedEventArgs<TResponse> : EventArgs
    {
        private readonly HttpResponseHeaders _headers;

        /// <summary>
        /// Initializes the completed event arguments
        /// </summary>
        /// <param name="headers">Headers of the response</param>
        public CompletedEventArgs(HttpResponseHeaders headers = null)
        {
            _headers = headers;
        }

        /// <summary>
        /// The received response
        /// </summary>
        public TResponse Response { set; get; }

        /// <summary>
        /// Flag which indicates whether the response is 
        /// a previously cached response
        /// </summary>
        public bool IsCached { get; set; }

        /// <summary>
        /// Flag which indicates whether an error occured
        /// </summary>
        public bool IsErrorOccured { get; set; }

        /// <summary>
        /// The error message if an error occured
        /// </summary>
        public string ErrorMessage { get; set; }

        /// <summary>
        /// The status code of the response
        /// </summary>
        public HttpStatusCode StatusCode { get; set; }

        /// <summary>
        /// Tries to get a specific header
        /// </summary>
        /// <param name="key">Key of the header to get</param>
        /// <param name="value">The value of the specific header or null if header not exists</param>
        /// <returns>Returns true if header exists, false otherwise</returns>
        public bool TryGetHeader(string key, out string value)
        {
            if (_headers == null)
            {
                value = null;
                return false;
            }

            value =_headers.GetValues(key).FirstOrDefault();
            return !string.IsNullOrEmpty(value);
        }
    }
}

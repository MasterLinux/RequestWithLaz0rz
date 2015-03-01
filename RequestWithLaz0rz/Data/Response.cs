using System.Linq;
using System.Net;
using System.Net.Http.Headers;

namespace RequestWithLaz0rz.Data
{
    public class Response<TResponse>
    {
        private readonly HttpResponseHeaders _headers;

        /// <summary>
        /// Initializes the completed event arguments
        /// </summary>
        /// <param name="headers">Headers of the response</param>
        public Response(HttpResponseHeaders headers = null)
        {
            _headers = headers;
        }

        /// <summary>
        /// The received response
        /// </summary>
        public TResponse Content { set; get; }

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

        /// <summary>
        /// Gets a specific header or null if not exists
        /// </summary>
        /// <param name="key">The key of the required header</param>
        /// <returns>The header value or null</returns>
        public string GetHeader(string key)
        {
            string value;
            if (TryGetHeader(key, out value))
            {
                return value;
            }

            return null;
        }
    }
}

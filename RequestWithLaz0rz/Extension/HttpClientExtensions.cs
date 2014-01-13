using System.Collections.Generic;
using System.Linq;
using System.Net.Http;

namespace RequestWithLaz0rz.Extension
{
    public static class HttpClientExtensions
    {
        /// <summary>
        /// Sets the user agent header
        /// </summary>
        /// <param name="client">This client</param>
        /// <param name="userAgent">The user agent to set</param>
        /// <returns></returns>
        public static HttpClient SetUserAgent(this HttpClient client, string userAgent)
        {
            if (string.IsNullOrEmpty(userAgent)) return client;
            client.DefaultRequestHeaders.UserAgent.ParseAdd(userAgent);
            return client;
        }

        /// <summary>
        /// Sets the accept header 
        /// </summary>
        /// <param name="client">This client</param>
        /// <param name="accept">The accept header to set</param>
        /// <returns></returns>
        public static HttpClient SetAcceptHeader(this HttpClient client, string accept)
        {
            if (string.IsNullOrEmpty(accept)) return client;
            client.DefaultRequestHeaders.Accept.ParseAdd(accept);
            return client;
        }

        /// <summary>
        /// Adds all headers
        /// </summary>
        /// <param name="client">This client</param>
        /// <param name="headers">All headers to add</param>
        /// <returns></returns>
        public static HttpClient AddHeaders(this HttpClient client, Dictionary<string, string> headers)
        {
            if (!headers.Any()) return client;

            foreach (var valuePair in headers)
            {
                client.DefaultRequestHeaders.Add(valuePair.Key, valuePair.Value);
            }

            return client;
        }
    }
}

using System.Collections.Generic;
using System.Linq;
using System.Net;
using RequestWithLaz0rz.Type;

namespace RequestWithLaz0rz.Extension
{
    static class HttpWebRequestExtension
    {
        /// <summary>
        /// Adds a dictionary of header to the request
        /// </summary>
        /// <param name="request">The request to update</param>
        /// <param name="header">The header to add</param>
        /// <returns>The updated request</returns>
        public static HttpWebRequest AddHeader(this HttpWebRequest request, Dictionary<string, string> header)
        {
            if(!header.Any()) return request;

            foreach (var keyValue in header)
            {
                request.Headers[keyValue.Key] = keyValue.Value;
            }

            return request;
        }

        /// <summary>
        /// Sets the HTTP method for this request.
        /// </summary>
        /// <param name="request">The request to update</param>
        /// <param name="method">The method to set</param>
        /// <returns>The updated request</returns>
        public static HttpWebRequest SetMethod(this HttpWebRequest request, HttpMethod method)
        {
            switch (method)
            {
                case HttpMethod.DELETE:
                    request.Method = "DELETE";
                    break;

                case HttpMethod.GET:
                    request.Method = "GET";
                    break;

                case HttpMethod.HEAD:
                    request.Method = "HEAD";
                    break;

                case HttpMethod.OPTIONS:
                    request.Method = "OPTIONS";
                    break;

                case HttpMethod.PATCH:
                    request.Method = "PATCH";
                    break;

                case HttpMethod.POST:
                    request.Method = "POST";
                    break;

                case HttpMethod.PUT:
                    request.Method = "PUT";
                    break;

                case HttpMethod.UPDATE:
                    request.Method = "UPDATE";
                    break;

                default:
                    request.Method = "GET";
                    break;
            }

            return request;
        }
    }
}

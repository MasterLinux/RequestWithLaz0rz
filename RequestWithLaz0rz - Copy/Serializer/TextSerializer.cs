using System;
using System.Net;

namespace RequestWithLaz0rz.Serializer
{
    class TextSerializer<TResponse> : ISerializer<TResponse>
    {
        /// <summary>
        /// Tries to get the response as string.
        /// </summary>
        /// <param name="response">The WebResponse to parse</param>
        /// <param name="obj">The required response as string or null on error</param>
        /// <returns>Whether the reading of the response stream was successfull</returns>
        public bool TryParse(WebResponse response, out TResponse obj)
        {
            throw new NotImplementedException();
        }
    }
}

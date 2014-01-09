using System;
using System.Net;

namespace RequestWithLaz0rz.Serializer
{
    class XmlSerializer<TResponse> : ISerializer<TResponse>
    {
        /// <summary>
        /// Tries to convert a XML string into its object representation.
        /// </summary>
        /// <param name="response">The WebResponse to parse</param>
        /// <param name="obj">The required object or the default value on error</param>
        /// <returns>Whether the parsing was successfull</returns>
        public bool TryParse(WebResponse response, out TResponse obj)
        {
            throw new NotImplementedException();
        }
    }
}

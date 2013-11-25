using System.IO;
using System.Net;
using Newtonsoft.Json;

namespace RequestWithLaz0rz.Serializer
{
    class JsonSerializer<TResponse> : ISerializer<TResponse>
    {
        /// <summary>
        /// Tries to convert a JSON string into its object representation.
        /// </summary>
        /// <param name="response">The WebResponse to parse</param>
        /// <param name="obj">The required object or the default value on error</param>
        /// <returns>Whether the parsing was successfull</returns>
        public bool TryParse(WebResponse response, out TResponse obj)
        {
            using (var streamReader = new StreamReader(response.GetResponseStream()))
            using (var jsonReader = new JsonTextReader(streamReader))
            {
                var serializer = new JsonSerializer();
                if (!Equals((obj = serializer.Deserialize<TResponse>(jsonReader)), default(TResponse)))
                {
                    return true;
                }
            }

            obj = default(TResponse); ;
            return false;
        }
    }
}

using System.IO;

namespace RequestWithLaz0rz.Serializer
{
    public interface ISerializer<TResponse>
    {
        /// <summary>
        /// Tries to convert a string of a specific data format into its object representation.
        /// </summary>
        /// <param name="responseBody">The WebResponse to parse</param>
        /// <param name="obj">The required object or the default value on error</param>
        /// <returns>Whether the parsing was successfull</returns>
        bool TryParse(Stream responseBody, out TResponse obj);
    }
}

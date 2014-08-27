using HttpMethod = RequestWithLaz0rz.Type.HttpMethod;

namespace RequestWithLaz0rz.Exception
{
    public class HttpMethodNotSupportedException : System.Exception
    {
        private readonly HttpMethod _method;

        public HttpMethodNotSupportedException(HttpMethod method)
        {
            _method = method;
        }

        public override string Message
        {
            get { return string.Format("HTTP Method [{0}] is currently not supported", _method.ToString("G")); }
        }
    }
}
